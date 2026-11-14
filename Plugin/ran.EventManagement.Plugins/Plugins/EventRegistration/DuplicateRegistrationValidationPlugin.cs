using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.EventRegistration
{
    public class DuplicateRegistrationValidationPlugin : IPlugin
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

                EntityReference eventReference =
                    registrationRecord.GetAttributeValue<EntityReference>("ran_event");

                EntityReference attendeeReference =
                    registrationRecord.GetAttributeValue<EntityReference>("ran_attendee");

                if (eventReference == null || attendeeReference == null)
                {
                    return;
                }

                Guid eventId = eventReference.Id;
                Guid attendeeId = attendeeReference.Id;

                QueryExpression registrationQuery =
                    new QueryExpression("ran_eventregistration");

                registrationQuery.ColumnSet =
                    new ColumnSet(false);

                registrationQuery.Criteria.AddCondition(
                    "ran_event",
                    ConditionOperator.Equal,
                    eventId);

                registrationQuery.Criteria.AddCondition(
                    "ran_attendee",
                    ConditionOperator.Equal,
                    attendeeId);

                EntityCollection existingRegistrations =
                    service.RetrieveMultiple(registrationQuery);

                if (existingRegistrations.Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException(
                        "Attendee is already registered for this event.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "DuplicateRegistrationValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating duplicate event registrations.");
            }
        }
    }
}