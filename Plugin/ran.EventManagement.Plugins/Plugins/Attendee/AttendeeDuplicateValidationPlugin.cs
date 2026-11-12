using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ran.EventManagement.Plugins
{
    public class AttendeeDuplicateValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                if (!(context.InputParameters.Contains("Target") &&
                      context.InputParameters["Target"] is Entity))
                {
                    return;
                }

                Entity target = (Entity)context.InputParameters["Target"];

                if (target.LogicalName != "ran_attendee")
                {
                    return;
                }

                Entity preImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                {
                    preImage = context.PreEntityImages["PreImage"];
                }

                string email = null;
                string phone = null;

                // Email
                if (target.Contains("ran_attendeeemail"))
                {
                    email = target.GetAttributeValue<string>("ran_attendeeemail");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_attendeeemail"))
                {
                    email = preImage.GetAttributeValue<string>("ran_attendeeemail");
                }

                // Phone
                if (target.Contains("ran_attendeephone"))
                {
                    phone = target.GetAttributeValue<string>("ran_attendeephone");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_attendeephone"))
                {
                    phone = preImage.GetAttributeValue<string>("ran_attendeephone");
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    email = email.Trim().ToLower();
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    phone = phone.Trim();
                }

                Guid currentRecordId = Guid.Empty;

                if (context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase))
                {
                    currentRecordId = target.Id;
                }

                QueryExpression query = new QueryExpression("ran_attendee");
                query.ColumnSet = new ColumnSet(
                    "ran_attendeeemail",
                    "ran_attendeephone");

                FilterExpression filter = new FilterExpression(LogicalOperator.Or);

                if (!string.IsNullOrWhiteSpace(email))
                {
                    filter.AddCondition(
                        "ran_attendeeemail",
                        ConditionOperator.Equal,
                        email);
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    filter.AddCondition(
                        "ran_attendeephone",
                        ConditionOperator.Equal,
                        phone);
                }

                if (filter.Conditions.Count == 0)
                {
                    return;
                }

                query.Criteria.AddFilter(filter);

                EntityCollection existingRecords =
                    service.RetrieveMultiple(query);

                foreach (Entity record in existingRecords.Entities)
                {
                    if (record.Id != currentRecordId)
                    {
                        throw new InvalidPluginExecutionException(
                            "An attendee with the same email or phone number already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw;
            }
        }
    }
}