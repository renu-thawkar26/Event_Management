using System;
using Microsoft.Xrm.Sdk;

namespace EventManagement.Plugins
{
    public class ValidateSessionSpeaker : IPlugin
    {
        private const string SessionEntityName = "ran_session";
        private const string SessionStatusField = "ran_sessionstatus";
        private const string SpeakerField = "ran_speaker";

        private const int PlannedStatusValue = 100000001;
        private const int CompletedStatusValue = 100000002;

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity targetEntity))
                {
                    return;
                }

                if (targetEntity.LogicalName != SessionEntityName)
                {
                    return;
                }

                Entity preImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                {
                    preImage = context.PreEntityImages["PreImage"];
                }

                OptionSetValue sessionStatus = null;

                if (targetEntity.Contains(SessionStatusField))
                {
                    sessionStatus =
                        targetEntity.GetAttributeValue<OptionSetValue>(SessionStatusField);
                }
                else if (preImage != null)
                {
                    sessionStatus =
                        preImage.GetAttributeValue<OptionSetValue>(SessionStatusField);
                }

                EntityReference speakerReference = null;

                if (targetEntity.Contains(SpeakerField))
                {
                    speakerReference =
                        targetEntity.GetAttributeValue<EntityReference>(SpeakerField);
                }
                else if (preImage != null)
                {
                    speakerReference =
                        preImage.GetAttributeValue<EntityReference>(SpeakerField);
                }

                if (sessionStatus != null &&
                    (sessionStatus.Value == PlannedStatusValue ||
                     sessionStatus.Value == CompletedStatusValue))
                {
                    if (speakerReference == null)
                    {
                        throw new InvalidPluginExecutionException(
                            "A Speaker must be assigned before marking the Session as Planned or Completed.");
                    }
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace(
                    "ValidateSessionSpeaker Plugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating the Session Speaker.");
            }
        }
    }
}