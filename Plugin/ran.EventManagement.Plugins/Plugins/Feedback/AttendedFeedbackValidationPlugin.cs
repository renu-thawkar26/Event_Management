using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.Feedback
{
    public class AttendedFeedbackValidationPlugin : IPlugin
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

                Entity sessionRecord =
                    service.Retrieve(
                        "ran_session",
                        sessionId,
                        new ColumnSet("ran_event"));

                EntityReference eventReference =
                    sessionRecord.GetAttributeValue<EntityReference>("ran_event");

                if (eventReference == null)
                {
                    throw new InvalidPluginExecutionException(
                        "The selected session is not linked to an event.");
                }

                Guid eventId = eventReference.Id;

                QueryExpression registrationQuery =
                    new QueryExpression("ran_eventregistration");

                registrationQuery.ColumnSet =
                    new ColumnSet("ran_status");

                registrationQuery.Criteria.AddCondition(
                    "ran_event",
                    ConditionOperator.Equal,
                    eventId);

                registrationQuery.Criteria.AddCondition(
                    "ran_attendee",
                    ConditionOperator.Equal,
                    attendeeId);

                EntityCollection registrationRecords =
                    service.RetrieveMultiple(registrationQuery);

                if (registrationRecords.Entities.Count == 0)
                {
                    throw new InvalidPluginExecutionException(
                        "Attendee is not registered for this event.");
                }

                OptionSetValue registrationStatus =
                    registrationRecords.Entities[0]
                    .GetAttributeValue<OptionSetValue>("ran_status");

                if (registrationStatus == null ||
                    registrationStatus.Value != 100000002)
                {
                    throw new InvalidPluginExecutionException(
                        "Only attendees with Attended status can submit feedback.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "AttendedFeedbackValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating attendee feedback.");
            }
        }
    }
}