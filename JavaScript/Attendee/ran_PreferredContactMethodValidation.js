function validatePreferredContactMethod(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var preferredContactMethod =
        formContext.getAttribute("ran_preferredcontactmethod").getValue();

    formContext.ui.clearFormNotification("PreferredContactValidation");

    if (preferredContactMethod === null) {

        formContext.ui.setFormNotification(
            "Preferred Contact Method is required.",
            "ERROR",
            "PreferredContactValidation"
        );

        eventArgs.preventDefault();
    }
}