function validateEmailAndPhoneFormat(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var email = formContext.getAttribute("ran_attendeeemail").getValue();
    var phone = formContext.getAttribute("ran_attendeephone").getValue();

    // Clear previous notifications
    formContext.ui.clearFormNotification("EmailValidation");
    formContext.ui.clearFormNotification("PhoneValidation");

    // Email Validation
    if (email != null && email.trim() !== "") {

        email = email.trim();

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
    if (phone != null && phone.trim() !== "") {

        phone = phone.trim();

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