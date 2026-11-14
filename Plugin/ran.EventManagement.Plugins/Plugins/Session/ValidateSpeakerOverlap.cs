using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EventManagement.Plugins
{
    public class ValidateSpeakerOverlap : IPlugin
    {
        private const string SessionEntityName = "ran_session";
        private const string SpeakerField = "ran_speaker";
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

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                Entity preImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                {
                    preImage = context.PreEntityImages["PreImage"];
                }

                EntityReference speakerReference =
                    targetEntity.Contains(SpeakerField)
                        ? targetEntity.GetAttributeValue<EntityReference>(SpeakerField)
                        : preImage?.GetAttributeValue<EntityReference>(SpeakerField);

                if (speakerReference == null)
                {
                    return;
                }

                DateTime sessionStartTime =
                    targetEntity.Contains(SessionStartTimeField)
                        ? targetEntity.GetAttributeValue<DateTime>(SessionStartTimeField)
                        : preImage.GetAttributeValue<DateTime>(SessionStartTimeField);

                DateTime sessionEndTime =
                    targetEntity.Contains(SessionEndTimeField)
                        ? targetEntity.GetAttributeValue<DateTime>(SessionEndTimeField)
                        : preImage.GetAttributeValue<DateTime>(SessionEndTimeField);

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
                    SpeakerField,
                    ConditionOperator.Equal,
                    speakerReference.Id);

                EntityCollection existingSessions =
                    service.RetrieveMultiple(sessionQuery);

                foreach (Entity existingSession in existingSessions.Entities)
                {
                    if (existingSession.Id == currentSessionId)
                    {
                        continue;
                    }

                    DateTime existingStartTime =
                        existingSession.GetAttributeValue<DateTime>(
                            SessionStartTimeField);

                    DateTime existingEndTime =
                        existingSession.GetAttributeValue<DateTime>(
                            SessionEndTimeField);

                    bool isOverlapping =
                        sessionStartTime < existingEndTime &&
                        sessionEndTime > existingStartTime;

                    if (isOverlapping)
                    {
                        throw new InvalidPluginExecutionException(
                            "This Speaker is already assigned to another session during the selected time period.");
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
                    "ValidateSpeakerOverlap Plugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating speaker availability.");
            }
        }
    }
}