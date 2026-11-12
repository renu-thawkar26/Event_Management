function validateFullNameLength(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var fullName = formContext.getAttribute("ran_fullname").getValue();

    // Clear old notification
    formContext.ui.clearFormNotification("FullNameValidation");

    // Check if field is empty
    if (fullName == null) {
        return;
    }

    // Remove leading and trailing spaces
    fullName = fullName.trim();

    // Prevent spaces-only value
    if (fullName.length === 0) {

        formContext.ui.setFormNotification(
            "Full Name cannot be empty.",
            "ERROR",
            "FullNameValidation"
        );

        eventArgs.preventDefault();
        return;
    }

    // Validate maximum length
    if (fullName.length > 100) {

        formContext.ui.setFormNotification(
            "Full Name cannot exceed 100 characters.",
            "ERROR",
            "FullNameValidation"
        );

        eventArgs.preventDefault();
        return;
    }
}