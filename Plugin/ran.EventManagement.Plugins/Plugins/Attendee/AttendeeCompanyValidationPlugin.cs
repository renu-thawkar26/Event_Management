using Microsoft.Xrm.Sdk;
using System;

namespace ran.EventManagement.Plugins
{
    public class AttendeeCompanyValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

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

                string company = null;
                string jobTitle = null;

                // Get Job Title
                if (target.Contains("ran_jobtitle"))
                {
                    jobTitle = target.GetAttributeValue<string>("ran_jobtitle");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_jobtitle"))
                {
                    jobTitle = preImage.GetAttributeValue<string>("ran_jobtitle");
                }

                // Get Company
                if (target.Contains("ran_company"))
                {
                    company = target.GetAttributeValue<string>("ran_company");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_company"))
                {
                    company = preImage.GetAttributeValue<string>("ran_company");
                }

                // Handle spaces
                if (jobTitle != null)
                {
                    jobTitle = jobTitle.Trim();
                }

                if (company != null)
                {
                    company = company.Trim();
                }

                // Validation
                //if (!string.IsNullOrWhiteSpace(jobTitle) &&
                //    string.IsNullOrWhiteSpace(company))
                //{
                //    throw new InvalidPluginExecutionException(
                //        "Company is required when Job Title is entered.");//}

                if (company != null)
                {
                    company = company.Trim();
                }

                bool companyInvalid =
                    string.IsNullOrWhiteSpace(company) ||
                    company.Equals("\"") ||
                    company.Equals("'") ||
                    company.Equals("-") ||
                    company.Equals("_");

                if (!string.IsNullOrWhiteSpace(jobTitle) &&
                    companyInvalid)
                {
                    throw new InvalidPluginExecutionException(
                        "Company is required when Job Title is entered.");
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "AttendeeCompanyValidationPlugin Error: "
                    + ex.ToString());

                throw;
            }
        }
    }
}
