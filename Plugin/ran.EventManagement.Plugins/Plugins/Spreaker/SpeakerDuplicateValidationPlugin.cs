using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ran.EventManagement.Plugins
{
    public class SpeakerDuplicateValidationPlugin : IPlugin
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

                // Verify Speaker Table
                if (target.LogicalName != "ran_speaker")
                {
                    return;
                }

                // Get Pre Image
                Entity preImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                {
                    preImage = context.PreEntityImages["PreImage"];
                }

                // Get Email
                string email = null;

                if (target.Contains("ran_speakeremail"))
                {
                    email = target.GetAttributeValue<string>("ran_speakeremail");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_speakeremail"))
                {
                    email = preImage.GetAttributeValue<string>("ran_speakeremail");
                }

                // Get Phone
                string phone = null;

                if (target.Contains("ran_speakerphone"))
                {
                    phone = target.GetAttributeValue<string>("ran_speakerphone");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_speakerphone"))
                {
                    phone = preImage.GetAttributeValue<string>("ran_speakerphone");
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

                if (context.MessageName == "Update")
                {
                    currentRecordId = target.Id;
                }

                // Duplicate Email Check
                if (!string.IsNullOrWhiteSpace(email))
                {
                    QueryExpression emailQuery = new QueryExpression("ran_speaker");
                    emailQuery.ColumnSet = new ColumnSet("ran_speakeremail");

                    emailQuery.Criteria.AddCondition(
                        "ran_speakeremail",
                        ConditionOperator.Equal,
                        email);

                    EntityCollection emailRecords =
                        service.RetrieveMultiple(emailQuery);

                    foreach (Entity record in emailRecords.Entities)
                    {
                        if (record.Id != currentRecordId)
                        {
                            throw new InvalidPluginExecutionException(
                                "A speaker with the same Email already exists.");
                        }
                    }
                }

                // Duplicate Phone Check
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    QueryExpression phoneQuery = new QueryExpression("ran_speaker");
                    phoneQuery.ColumnSet = new ColumnSet("ran_speakerphone");

                    phoneQuery.Criteria.AddCondition(
                        "ran_speakerphone",
                        ConditionOperator.Equal,
                        phone);

                    EntityCollection phoneRecords =
                        service.RetrieveMultiple(phoneQuery);

                    foreach (Entity record in phoneRecords.Entities)
                    {
                        if (record.Id != currentRecordId)
                        {
                            throw new InvalidPluginExecutionException(
                                "A speaker with the same Phone Number already exists.");
                        }
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