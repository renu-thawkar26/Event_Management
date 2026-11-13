using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ran.EventManagement.Plugins
{
    public class EventRegistrationValidationPlugin : IPlugin
    {
        private const int CancelledStatus = 100000003;

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = EventPluginHelper.GetContext(serviceProvider);

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "ran_eventregistration")
                return;

            Entity preImage = null;

            if (context.MessageName == "Update" &&
                context.PreEntityImages.Contains("PreImage"))
            {
                preImage = context.PreEntityImages["PreImage"];
            }

            EntityReference eventReference =
                EventPluginHelper.GetReference(
                    target, preImage, "ran_event");

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

            if (eventStatus == CancelledStatus)
            {
                EventPluginHelper.Throw(
                    "Registration is not allowed because this Event " +
                    "has been Cancelled.");
            }
        }
    }
}
