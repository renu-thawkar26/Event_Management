using Microsoft.Xrm.Sdk;
using System;

namespace ran.EventManagement.Plugins
{
    public class EventTypeChangeValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = EventPluginHelper.GetContext(serviceProvider);

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "ran_event" ||
                context.MessageName != "Update")
                return;

            if (!target.Contains("ran_eventtype"))
                return;

            if (!context.PreEntityImages.Contains("PreImage"))
                EventPluginHelper.Throw(
                    "Plugin configuration error: PreImage is required.");

            Entity preImage = context.PreEntityImages["PreImage"];

            var oldType = preImage.GetAttributeValue<OptionSetValue>(
                "ran_eventtype");

            var newType = target.GetAttributeValue<OptionSetValue>(
                "ran_eventtype");

            if (oldType == null || newType == null ||
                oldType.Value == newType.Value)
                return;

            var service = EventPluginHelper.GetService(
                serviceProvider, context);

            if (EventPluginHelper.HasRelatedRecord(
                service,
                "ran_session",
                "ran_event",
                target.Id))
            {
                EventPluginHelper.Throw(
                    "Event Type cannot be changed because Sessions " +
                    "have already been created for this Event.");
            }
        }
    }
}
