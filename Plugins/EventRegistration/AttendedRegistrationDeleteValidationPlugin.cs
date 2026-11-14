using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.EventRegistration
{
    public class AttendedRegistrationDeleteValidationPlugin : IPlugin
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

                EntityReference registrationReference =
                    (EntityReference)pluginContext.InputParameters["Target"];

                Entity registrationRecord =
                    service.Retrieve(
                        "ran_eventregistration",
                        registrationReference.Id,
                        new ColumnSet("ran_status"));

                OptionSetValue registrationStatus =
                    registrationRecord.GetAttributeValue<OptionSetValue>("ran_status");

                if (registrationStatus != null &&
                    registrationStatus.Value == 100000002)
                {
                    throw new InvalidPluginExecutionException(
                        "Attended registrations cannot be deleted.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "AttendedRegistrationDeleteValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating registration deletion.");
            }
        }
    }
}