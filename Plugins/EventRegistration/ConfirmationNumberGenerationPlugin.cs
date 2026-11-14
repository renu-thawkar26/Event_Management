using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.EventRegistration
{
    public class ConfirmationNumberGenerationPlugin : IPlugin
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

                EntityReference attendeeReference =
                    registrationRecord.GetAttributeValue<EntityReference>("ran_attendee");

                Entity attendeeRecord =
                    service.Retrieve(
                        "ran_attendee",
                        attendeeReference.Id,
                        new ColumnSet(
                            "ran_firstname",
                            "ran_lastname"));

                string firstName =
                    attendeeRecord.GetAttributeValue<string>("ran_firstname") ?? string.Empty;

                string lastName =
                    attendeeRecord.GetAttributeValue<string>("ran_lastname") ?? string.Empty;

                string firstInitial =
                    string.IsNullOrWhiteSpace(firstName)
                    ? "X"
                    : firstName.Substring(0, 1).ToUpper();

                string secondInitial =
                    string.IsNullOrWhiteSpace(lastName)
                    ? "X"
                    : lastName.Substring(0, 1).ToUpper();

                string attendeeInitials =
                    firstInitial + secondInitial;

                string datePortion =
                    DateTime.Now.ToString("ddMM");

                QueryExpression registrationQuery =
                    new QueryExpression("ran_eventregistration");

                registrationQuery.ColumnSet =
                    new ColumnSet(false);

                int nextRunningNumber =
                    service.RetrieveMultiple(registrationQuery)
                    .Entities.Count + 1;

                string runningNumber =
                    nextRunningNumber.ToString("D4");

                string confirmationNumber =
                    $"{attendeeInitials}-EN-{datePortion}-{runningNumber}";

                registrationRecord["ran_confirmationnumber"] =
                    confirmationNumber;
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "ConfirmationNumberGenerationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while generating the confirmation number.");
            }
        }
    }
}