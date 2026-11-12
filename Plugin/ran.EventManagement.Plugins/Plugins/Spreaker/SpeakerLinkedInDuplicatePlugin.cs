using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ran.EventManagement.Plugins
{
    public class SpeakerLinkedInDuplicatePlugin : IPlugin
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

                if (target.LogicalName != "ran_speaker")
                {
                    return;
                }

                Entity preImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                {
                    preImage = context.PreEntityImages["PreImage"];
                }

                string linkedInProfile = null;

                // Get LinkedIn value
                if (target.Contains("ran_linkedinprofile"))
                {
                    linkedInProfile =
                        target.GetAttributeValue<string>("ran_linkedinprofile");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_linkedinprofile"))
                {
                    linkedInProfile =
                        preImage.GetAttributeValue<string>("ran_linkedinprofile");
                }

                // Skip if blank
                if (string.IsNullOrWhiteSpace(linkedInProfile))
                {
                    return;
                }

                // Normalize value
                linkedInProfile = linkedInProfile.Trim().ToLower();

                Guid currentRecordId = Guid.Empty;

                if (context.MessageName.Equals("Update",
                    StringComparison.OrdinalIgnoreCase))
                {
                    currentRecordId = target.Id;
                }

                // Retrieve all speakers having LinkedIn value
                QueryExpression query =
                    new QueryExpression("ran_speaker");

                query.ColumnSet = new ColumnSet(
                    "ran_speakerid",
                    "ran_linkedinprofile");

                query.Criteria.AddCondition(
                    "ran_linkedinprofile",
                    ConditionOperator.NotNull);

                EntityCollection speakers =
                    service.RetrieveMultiple(query);

                foreach (Entity speaker in speakers.Entities)
                {
                    string existingLinkedIn =
                        speaker.GetAttributeValue<string>(
                            "ran_linkedinprofile");

                    if (string.IsNullOrWhiteSpace(existingLinkedIn))
                    {
                        continue;
                    }

                    existingLinkedIn =
                        existingLinkedIn.Trim().ToLower();

                    // Ignore current record during update
                    if (speaker.Id == currentRecordId)
                    {
                        continue;
                    }

                    if (existingLinkedIn == linkedInProfile)
                    {
                        throw new InvalidPluginExecutionException(
                            "This LinkedIn Profile is already associated with another speaker.");
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "SpeakerLinkedInDuplicatePlugin Error: " +
                    ex.ToString());

                throw;
            }
        }
    }
}