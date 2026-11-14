using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using System;

namespace EventManagement.Plugins.Session
{
    public class SessionCancelledEventValidationPlugin : IPlugin
    {
        private const int CancelledStatusValue = 100000003;

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

                if (!targetEntity.Contains("ran_event"))
                {
                    return;
                }

                EntityReference eventReference =
                    targetEntity.GetAttributeValue<EntityReference>("ran_event");

                if (eventReference == null)
                {
                    return;
                }

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                Entity eventRecord = service.Retrieve(
                    "ran_event",
                    eventReference.Id,
                    new ColumnSet("ran_eventstatus"));

                OptionSetValue eventStatus =
                    eventRecord.GetAttributeValue<OptionSetValue>("ran_eventstatus");

                if (eventStatus != null &&
                    eventStatus.Value == CancelledStatusValue)
                {
                    throw new InvalidPluginExecutionException(
                        "Session cannot be created under a Cancelled Event.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "SessionCancelledEventValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating the selected Event.");
            }
        }
    }
}