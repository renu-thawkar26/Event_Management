function validateEmailAndPhone(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var email = formContext.getAttribute("ran_speakeremail").getValue();
    var phone = formContext.getAttribute("ran_speakerphone").getValue();

    // Clear old notifications
    formContext.ui.clearFormNotification("EmailValidation");
    formContext.ui.clearFormNotification("PhoneValidation");

    // Email Validation
    if (email != null && email !== "") {

        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!emailRegex.test(email)) {

            formContext.ui.setFormNotification(
                "Please enter a valid email address. Example: abc@example.com",
                "ERROR",
                "EmailValidation"
            );

            eventArgs.preventDefault();
            return;
        }
    }

    // Phone Validation
    if (phone != null && phone !== "") {

        var phoneRegex = /^\+\d+$/;

        if (!phoneRegex.test(phone)) {

            formContext.ui.setFormNotification(
                "Phone number must start with '+' and contain only numeric characters.",
                "ERROR",
                "PhoneValidation"
            );

            eventArgs.preventDefault();
            return;
        }
    }
}