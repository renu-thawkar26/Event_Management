using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EventManagement.Plugins
{
    public class ValidateSessionDates : IPlugin
    {
        private const string SessionEntityName = "ran_session";
        private const string EventLookupField = "ran_event";
        private const string SessionStartTimeField = "ran_starttime";
        private const string SessionEndTimeField = "ran_endtime";
        private const string EventStartDateField = "ran_startdate";
        private const string EventEndDateField = "ran_enddate";

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

                if (targetEntity.LogicalName != SessionEntityName)
                {
                    return;
                }

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                Entity existingSession = null;

                if (context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase))
                {
                    existingSession = service.Retrieve(
                        SessionEntityName,
                        context.PrimaryEntityId,
                        new ColumnSet(
                            SessionStartTimeField,
                            SessionEndTimeField,
                            EventLookupField));
                }

                DateTime sessionStartTime =
                    targetEntity.Contains(SessionStartTimeField)
                        ? targetEntity.GetAttributeValue<DateTime>(SessionStartTimeField)
                        : existingSession.GetAttributeValue<DateTime>(SessionStartTimeField);

                DateTime sessionEndTime =
                    targetEntity.Contains(SessionEndTimeField)
                        ? targetEntity.GetAttributeValue<DateTime>(SessionEndTimeField)
                        : existingSession.GetAttributeValue<DateTime>(SessionEndTimeField);

                EntityReference eventReference =
                    targetEntity.Contains(EventLookupField)
                        ? targetEntity.GetAttributeValue<EntityReference>(EventLookupField)
                        : existingSession.GetAttributeValue<EntityReference>(EventLookupField);

                if (eventReference == null)
                {
                    return;
                }

                Entity eventRecord = service.Retrieve(
                    eventReference.LogicalName,
                    eventReference.Id,
                    new ColumnSet(
                        EventStartDateField,
                        EventEndDateField));

                DateTime eventStartDate =
                    eventRecord.GetAttributeValue<DateTime>(EventStartDateField);

                DateTime eventEndDate =
                    eventRecord.GetAttributeValue<DateTime>(EventEndDateField);

                // Validation 1:
                // Session End Time must be greater than or equal to Session Start Time
                if (sessionStartTime > sessionEndTime)
                {
                    throw new InvalidPluginExecutionException(
                        "Session End Time must be greater than or equal to Session Start Time.");
                }

                // Validation 2:
                // Session must be scheduled within Event dates
                if (sessionStartTime < eventStartDate ||
                    sessionEndTime > eventEndDate)
                {
                    throw new InvalidPluginExecutionException(
                        "Session Start Date and End Date must be within the selected Event's Start Date and End Date.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "ValidateSessionDates Plugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating Session dates.");
            }
        }
    }
}