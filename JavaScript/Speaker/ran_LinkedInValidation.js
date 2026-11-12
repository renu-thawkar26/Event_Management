function validateLinkedInUrl(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var linkedInUrl = formContext.getAttribute("ran_linkedinprofile").getValue();

    // Clear old notification
    formContext.ui.clearFormNotification("LinkedInValidation");

    // Validate only when value is entered
    if (linkedInUrl && linkedInUrl.trim() !== "") {

        var linkedInRegex = /^(https?:\/\/)(www\.)?linkedin\.com\/.+$/i;

        if (!linkedInRegex.test(linkedInUrl)) {

            formContext.ui.setFormNotification(
                "Please enter a valid LinkedIn Profile URL. Example: https://www.linkedin.com/in/johndoe",
                "ERROR",
                "LinkedInValidation"
            );

            eventArgs.preventDefault();
            return;
        }
    }
}