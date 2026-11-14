using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EventManagement.Plugins.Session
{
    public class SessionCompletionValidation : IPlugin
    {
        private const int CompletedStatusValue = 100000002;

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

                if (targetEntity.LogicalName != "ran_session")
                {
                    return;
                }

                if (!targetEntity.Contains("ran_sessionstatus"))
                {
                    return;
                }

                OptionSetValue sessionStatus =
                    targetEntity.GetAttributeValue<OptionSetValue>("ran_sessionstatus");

                if (sessionStatus == null ||
                    sessionStatus.Value != CompletedStatusValue)
                {
                    return;
                }

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                Entity sessionRecord = service.Retrieve(
                    "ran_session",
                    targetEntity.Id,
                    new ColumnSet("ran_endtime"));

                DateTime? sessionEndTime =
                    sessionRecord.GetAttributeValue<DateTime?>("ran_endtime");

                if (!sessionEndTime.HasValue)
                {
                    return;
                }

                if (DateTime.UtcNow < sessionEndTime.Value.ToUniversalTime())
                {
                    throw new InvalidPluginExecutionException(
                        "Session cannot be marked as Completed before its End Time.");
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
                    "An error occurred while validating the session completion status.");
            }
        }
    }
}