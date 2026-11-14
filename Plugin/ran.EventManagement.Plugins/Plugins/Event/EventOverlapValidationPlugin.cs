using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ran.EventManagement.Plugins
{
    public class EventOverlapValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = EventPluginHelper.GetContext(serviceProvider);

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

            string location = EventPluginHelper.GetValue(
                target, preImage, "ran_location") as string;

            if (!startDate.HasValue ||
                !endDate.HasValue ||
                string.IsNullOrWhiteSpace(location))
                return;

            var query = new QueryExpression("ran_event")
            {
                ColumnSet = new ColumnSet(
                    "ran_eventname",
                    "ran_startdate",
                    "ran_enddate")
            };

            query.Criteria.AddCondition(
                "ran_location",
                ConditionOperator.Equal,
                location);

            query.Criteria.AddCondition(
                "ran_startdate",
                ConditionOperator.LessEqual,
                endDate.Value);

            query.Criteria.AddCondition(
                "ran_enddate",
                ConditionOperator.GreaterEqual,
                startDate.Value);

            if (context.MessageName == "Update")
            {
                query.Criteria.AddCondition(
                    "ran_eventid",
                    ConditionOperator.NotEqual,
                    target.Id);
            }

            var results = service.RetrieveMultiple(query);

            if (results.Entities.Count > 0)
            {
                EventPluginHelper.Throw(
                    "Another Event already exists at this Location " +
                    "during the selected time period. " +
                    "Please choose a different time or Location.");
            }
        }
    }
}
