using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.Feedback
{
    public class DuplicateFeedbackValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext pluginContext =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(pluginContext.UserId);

            try
            {
                if (!pluginContext.InputParameters.Contains("Target") ||
                    !(pluginContext.InputParameters["Target"] is Entity))
                {
                    return;
                }

                Entity feedbackRecord =
                    (Entity)pluginContext.InputParameters["Target"];

                EntityReference attendeeReference =
                    feedbackRecord.GetAttributeValue<EntityReference>("ran_attendee");

                EntityReference sessionReference =
                    feedbackRecord.GetAttributeValue<EntityReference>("ran_session");

                if (attendeeReference == null || sessionReference == null)
                {
                    return;
                }

                Guid attendeeId = attendeeReference.Id;
                Guid sessionId = sessionReference.Id;

                QueryExpression feedbackQuery =
                    new QueryExpression("ran_feedback");

                feedbackQuery.ColumnSet =
                    new ColumnSet(false);

                feedbackQuery.Criteria.AddCondition(
                    "ran_attendee",
                    ConditionOperator.Equal,
                    attendeeId);

                feedbackQuery.Criteria.AddCondition(
                    "ran_session",
                    ConditionOperator.Equal,
                    sessionId);

                EntityCollection existingFeedbackRecords =
                    service.RetrieveMultiple(feedbackQuery);

                if (existingFeedbackRecords.Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException(
                        "You have already submitted feedback for this session.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "DuplicateFeedbackValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating duplicate feedback submissions.");
            }
        }
    }
}