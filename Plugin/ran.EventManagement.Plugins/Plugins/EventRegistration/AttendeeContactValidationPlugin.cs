using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.EventRegistration
{
    public class AttendeeContactValidationPlugin : IPlugin
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

                Entity registrationRecord =
                    (Entity)pluginContext.InputParameters["Target"];

                if (!registrationRecord.Contains("ran_attendee"))
                {
                    return;
                }

                Guid attendeeId =
                    registrationRecord.GetAttributeValue<EntityReference>("ran_attendee").Id;

                Entity attendeeRecord =
                    service.Retrieve(
                        "ran_attendee",
                        attendeeId,
                        new ColumnSet(
                            "ran_attendeeemail",
                            "ran_attendeephone",
                            "ran_fullname"));

                string attendeeEmail =
                    attendeeRecord.GetAttributeValue<string>("ran_attendeeemail");

                string attendeePhone =
                    attendeeRecord.GetAttributeValue<string>("ran_attendeephone");

                if (string.IsNullOrWhiteSpace(attendeeEmail) ||
                    string.IsNullOrWhiteSpace(attendeePhone))
                {
                    string attendeeName =
                        attendeeRecord.GetAttributeValue<string>("ran_fullname");

                    throw new InvalidPluginExecutionException(
                        $"Attendee '{attendeeName}' must have both Email and Phone Number before registration.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "AttendeeContactValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating attendee contact information.");
            }
        }
    }
}