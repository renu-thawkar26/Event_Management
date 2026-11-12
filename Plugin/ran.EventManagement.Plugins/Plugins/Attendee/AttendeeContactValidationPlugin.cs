using Microsoft.Xrm.Sdk;
using System;

namespace ran.EventManagement.Plugins
{
    public class AttendeeContactValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

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

                // Get Email from Target or PreImage
                if (target.Contains("ran_attendeeemail"))
                {
                    email = target.GetAttributeValue<string>("ran_attendeeemail");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_attendeeemail"))
                {
                    email = preImage.GetAttributeValue<string>("ran_attendeeemail");
                }

                // Get Phone from Target or PreImage
                if (target.Contains("ran_attendeephone"))
                {
                    phone = target.GetAttributeValue<string>("ran_attendeephone");
                }
                else if (preImage != null &&
                         preImage.Contains("ran_attendeephone"))
                {
                    phone = preImage.GetAttributeValue<string>("ran_attendeephone");
                }

                // Edge Case Handling

                // Trim leading/trailing spaces
                if (email != null)
                {
                    email = email.Trim();
                }

                if (phone != null)
                {
                    phone = phone.Trim();
                }

                bool isEmailEmpty = string.IsNullOrWhiteSpace(email);
                bool isPhoneEmpty = string.IsNullOrWhiteSpace(phone);

                // Validation
                if (isEmailEmpty && isPhoneEmpty)
                {
                    throw new InvalidPluginExecutionException(
                        "Either Email or Phone Number must be provided.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}