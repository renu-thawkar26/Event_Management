using Microsoft.Xrm.Sdk;
using System;

namespace ran.EventManagement.Plugins
{
    public class EventDateValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = EventPluginHelper.GetContext(serviceProvider);
            var tracing = EventPluginHelper.GetTracing(serviceProvider);

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "ran_event")
                return;

            var service = EventPluginHelper.GetService(
                serviceProvider, context);

            Entity preImage = null;

            if (context.MessageName == "Update" &&
                context.PreEntityImages.Contains("PreImage"))
            {
                preImage = context.PreEntityImages["PreImage"];
            }

            DateTime? startDate = EventPluginHelper.GetDate(
                target, preImage, "ran_startdate");

            DateTime? endDate = EventPluginHelper.GetDate(
                target, preImage, "ran_enddate");

            if (!startDate.HasValue)
                return;

            tracing.Trace("Validating Event dates.");

            if (context.MessageName == "Create" &&
                startDate.Value.Date < DateTime.UtcNow.Date)
            {
                EventPluginHelper.Throw(
                    "Event Start Date cannot be in the past.");
            }

            if (endDate.HasValue &&
                endDate.Value <= startDate.Value)
            {
                EventPluginHelper.Throw(
                    "Event End Date must be later than Start Date.");
            }
        }
    }
}
