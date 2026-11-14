using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EventManagement.Plugins
{
    public class PreventSessionDeletion : IPlugin
    {
        private const string FeedbackEntityName = "ran_feedback";
        private const string SessionLookupField = "ran_session";

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                if (context.MessageName != "Delete")
                {
                    return;
                }

                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is EntityReference sessionReference))
                {
                    return;
                }

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                QueryExpression feedbackQuery =
                    new QueryExpression(FeedbackEntityName)
                    {
                        ColumnSet = new ColumnSet(false)
                    };

                feedbackQuery.Criteria.AddCondition(
                    SessionLookupField,
                    ConditionOperator.Equal,
                    sessionReference.Id);

                EntityCollection feedbackRecords =
                    service.RetrieveMultiple(feedbackQuery);

                if (feedbackRecords.Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException(
                        "This Session cannot be deleted because Feedback records exist for it.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "PreventSessionDeletion Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating Session deletion.");
            }
        }
    }
}