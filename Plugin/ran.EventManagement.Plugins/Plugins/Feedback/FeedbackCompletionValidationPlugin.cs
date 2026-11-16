using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace NSQ.EventManagement.Plugins.Plugins.Feedback
{
    public class FeedbackCompletionValidationPlugin : IPlugin
    {
        private const int CompletedStatusValue = 100000002;

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            IPluginExecutionContext pluginContext =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(
                    pluginContext.UserId);

            try
            {
                if (!pluginContext.InputParameters.Contains("Target") ||
                    !(pluginContext.InputParameters["Target"] is Entity))
                {
                    return;
                }

                Entity feedbackRecord =
                    (Entity)pluginContext.InputParameters["Target"];

                EntityReference sessionReference =
                    feedbackRecord.GetAttributeValue<EntityReference>(
                        "ran_session");

                if (sessionReference == null)
                {
                    return;
                }

                Entity sessionRecord =
                    service.Retrieve(
                        "ran_session",
                        sessionReference.Id,
                        new ColumnSet("ran_sessionstatus"));

                OptionSetValue sessionStatus =
                    sessionRecord.GetAttributeValue<OptionSetValue>(
                        "ran_sessionstatus");

                if (sessionStatus == null ||
                    sessionStatus.Value != CompletedStatusValue)
                {
                    throw new InvalidPluginExecutionException(
                        "Feedback cannot be created until the Session is Completed.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "SessionCompletionValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating the session status.");
            }
        }
    }
}