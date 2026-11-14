using Microsoft.Xrm.Sdk;
using System;

namespace NSQ.EventManagement.Plugins.Plugins.Feedback
{
    public class CommentProfanityValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext pluginContext =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                if (!pluginContext.InputParameters.Contains("Target") ||
                    !(pluginContext.InputParameters["Target"] is Entity))
                {
                    return;
                }

                Entity feedbackRecord =
                    (Entity)pluginContext.InputParameters["Target"];

                string comments =
                    feedbackRecord.GetAttributeValue<string>("ran_comments");

                if (string.IsNullOrWhiteSpace(comments))
                {
                    return;
                }

                string[] forbiddenWords =
                {
                    "badword",
                    "abuse",
                    "stupid",
                    "idiot"
                };

                foreach (string forbiddenWord in forbiddenWords)
                {
                    if (comments.IndexOf(
                            forbiddenWord,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new InvalidPluginExecutionException(
                            "Comments contain inappropriate language. Please revise and submit again.");
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
                    "CommentProfanityValidationPlugin Error: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred while validating feedback comments.");
            }
        }
    }
}