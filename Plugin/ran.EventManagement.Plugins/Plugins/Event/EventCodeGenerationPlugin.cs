using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;

namespace ran.EventManagement.Plugins
{
    public class EventCodeGenerationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = EventPluginHelper.GetContext(serviceProvider);

            if (context.MessageName != "Create" ||
                !context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "ran_event")
                return;

            if (!string.IsNullOrWhiteSpace(
                target.GetAttributeValue<string>("ran_eventcode")))
                return;

            DateTime? startDate =
                target.GetAttributeValue<DateTime?>("ran_startdate");

            if (!startDate.HasValue)
            {
                EventPluginHelper.Throw(
                    "Start Date is required to generate the Event Code.");
            }

            var service = EventPluginHelper.GetService(
                serviceProvider, context);

            string datePart = startDate.Value.ToString(
                "ddMMyy", CultureInfo.InvariantCulture);

            string prefix = "EN-" + datePart + "-";

            var query = new QueryExpression("ran_event")
            {
                ColumnSet = new ColumnSet("ran_eventcode"),
                TopCount = 1
            };

            query.Criteria.AddCondition(
                "ran_eventcode",
                ConditionOperator.Like,
                prefix + "%");

            query.AddOrder(
                "ran_eventcode",
                OrderType.Descending);

            var results = service.RetrieveMultiple(query);

            int nextNumber = 1;

            if (results.Entities.Count > 0)
            {
                string existingCode =
                    results.Entities[0].GetAttributeValue<string>(
                        "ran_eventcode");

                if (!string.IsNullOrWhiteSpace(existingCode))
                {
                    string[] parts = existingCode.Split('-');

                    if (parts.Length == 3 &&
                        int.TryParse(parts[2], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            string eventCode = prefix +
                nextNumber.ToString("D3");

            target["ran_eventcode"] = eventCode;
        }
    }
}