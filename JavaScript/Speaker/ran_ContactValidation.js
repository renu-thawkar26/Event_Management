function validateEmailOrPhone(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var email = formContext.getAttribute("ran_speakeremail").getValue();
    var phone = formContext.getAttribute("ran_speakerphone").getValue();

    // Clear old notification
    formContext.ui.clearFormNotification("ContactValidation");

    // Both are empty
    if ((!email || email.trim() === "") &&
        (!phone || phone.trim() === "")) {

        formContext.ui.setFormNotification(
            "Either Email or Phone Number must be provided.",
            "ERROR",
            "ContactValidation"
        );

        eventArgs.preventDefault();
        return;
    }
}