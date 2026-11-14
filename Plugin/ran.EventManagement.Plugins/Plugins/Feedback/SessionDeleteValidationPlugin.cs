using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.Feedback
{
    public class SessionDeleteValidationPlugin : IPlugin
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
                    !(pluginContext.InputParameters["Target"] is EntityReference))
                {
                    return;
                }

                EntityReference sessionReference =
                    (EntityReference)pluginContext.InputParameters["Target"];

                QueryExpression feedbackQuery =
                    new QueryExpression("ran_feedback");

                feedbackQuery.ColumnSet =
                    new ColumnSet(false);

                feedbackQuery.Criteria.AddCondition(
                    "ran_session",
                    ConditionOperator.Equal,
                    sessionReference.Id);

                EntityCollection feedbackRecords =
                    service.RetrieveMultiple(feedbackQuery);

                if (feedbackRecords.Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException(
                        "This session cannot be deleted because feedback records exist.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "SessionDeleteValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating session deletion.");
            }
        }
    }
}