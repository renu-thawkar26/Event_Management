using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ran.EventManagement.Plugins
{
    public class SessionEventStatusValidationPlugin : IPlugin
    {
        private const int CompletedStatus = 100000002;
        private const int CancelledStatus = 100000003;

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = EventPluginHelper.GetContext(serviceProvider);

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "ran_session")
                return;

            EntityReference eventReference =
                target.GetAttributeValue<EntityReference>("ran_event");

            if (eventReference == null &&
                context.MessageName == "Update" &&
                context.PreEntityImages.Contains("PreImage"))
            {
                eventReference =
                    context.PreEntityImages["PreImage"]
                    .GetAttributeValue<EntityReference>("ran_event");
            }

            if (eventReference == null)
                return;

            var service = EventPluginHelper.GetService(
                serviceProvider, context);

            Entity eventRecord = service.Retrieve(
                "ran_event",
                eventReference.Id,
                new ColumnSet("ran_eventstatus"));

            int eventStatus = eventRecord
                .GetAttributeValue<OptionSetValue>(
                    "ran_eventstatus")?.Value ?? -1;

            if (eventStatus == CompletedStatus)
            {
                EventPluginHelper.Throw(
                    "You cannot create or modify a Session because " +
                    "the related Event is Completed.");
            }

            if (eventStatus == CancelledStatus)
            {
                EventPluginHelper.Throw(
                    "You cannot create or modify a Session because " +
                    "the related Event is Cancelled.");
            }
        }
    }
}
