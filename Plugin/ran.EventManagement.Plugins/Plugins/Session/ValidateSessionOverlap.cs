using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EventManagement.Plugins
{
    public class ValidateSessionOverlap : IPlugin
    {
        private const string SessionEntityName = "ran_session";
        private const string EventLookupField = "ran_event";
        private const string SessionStartTimeField = "ran_starttime";
        private const string SessionEndTimeField = "ran_endtime";

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity targetEntity))
                {
                    return;
                }

                if (targetEntity.LogicalName != SessionEntityName)
                {
                    return;
                }

                Entity preImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                {
                    preImage = context.PreEntityImages["PreImage"];
                }

                DateTime sessionStartTime =
                    targetEntity.Contains(SessionStartTimeField)
                        ? targetEntity.GetAttributeValue<DateTime>(SessionStartTimeField)
                        : preImage.GetAttributeValue<DateTime>(SessionStartTimeField);

                DateTime sessionEndTime =
                    targetEntity.Contains(SessionEndTimeField)
                        ? targetEntity.GetAttributeValue<DateTime>(SessionEndTimeField)
                        : preImage.GetAttributeValue<DateTime>(SessionEndTimeField);

                EntityReference eventReference =
                    targetEntity.Contains(EventLookupField)
                        ? targetEntity.GetAttributeValue<EntityReference>(EventLookupField)
                        : preImage.GetAttributeValue<EntityReference>(EventLookupField);

                if (eventReference == null)
                {
                    return;
                }

                Guid currentSessionId =
                    context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase)
                    ? context.PrimaryEntityId
                    : Guid.Empty;

                QueryExpression sessionQuery =
                    new QueryExpression(SessionEntityName)
                    {
                        ColumnSet = new ColumnSet(
                            SessionStartTimeField,
                            SessionEndTimeField)
                    };

                sessionQuery.Criteria.AddCondition(
                    EventLookupField,
                    ConditionOperator.Equal,
                    eventReference.Id);

                EntityCollection existingSessions =
                    serviceProvider
                        .GetService(typeof(IOrganizationServiceFactory))
                        is IOrganizationServiceFactory serviceFactory
                        ? serviceFactory.CreateOrganizationService(context.UserId)
                            .RetrieveMultiple(sessionQuery)
                        : null;

                foreach (Entity existingSession in existingSessions.Entities)
                {
                    if (existingSession.Id == currentSessionId)
                    {
                        continue;
                    }

                    DateTime existingStartTime =
                        existingSession.GetAttributeValue<DateTime>(SessionStartTimeField);

                    DateTime existingEndTime =
                        existingSession.GetAttributeValue<DateTime>(SessionEndTimeField);

                    bool isOverlapping =
                        sessionStartTime < existingEndTime &&
                        sessionEndTime > existingStartTime;

                    if (isOverlapping)
                    {
                        throw new InvalidPluginExecutionException(
                            "This session overlaps with another session scheduled for the same event.");
                    }
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "ValidateSessionOverlap Plugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating session overlap.");
            }
        }
    }
}