using Microsoft.Xrm.Sdk;
using System;

namespace ran.EventManagement.Plugins
{
    public class EventSessionValidationPlugin : IPlugin
    {
        private const int ScheduledStatus = 100000001;

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = EventPluginHelper.GetContext(serviceProvider);

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "ran_event")
                return;

            if (context.MessageName != "Update")
                return;

            Entity preImage = null;

            if (context.PreEntityImages.Contains("PreImage"))
                preImage = context.PreEntityImages["PreImage"];

            int status = EventPluginHelper.GetOption(
                target,
                preImage,
                "ran_eventstatus");

            if (status != ScheduledStatus)
                return;

            var service = EventPluginHelper.GetService(
                serviceProvider, context);

            if (!EventPluginHelper.HasRelatedRecord(
                service,
                "ran_session",
                "ran_event",
                target.Id))
            {
                EventPluginHelper.Throw(
                    "You cannot schedule this Event without at least " +
                    "one Session. Please create a Session first.");
            }
        }
    }
}
