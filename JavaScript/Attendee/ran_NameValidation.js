function validateNames(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var firstName = formContext.getAttribute("ran_firstname").getValue();
    var lastName = formContext.getAttribute("ran_lastname").getValue();

    // Clear old notifications
    formContext.ui.clearFormNotification("FirstNameValidation");
    formContext.ui.clearFormNotification("LastNameValidation");

    // Regex to detect any number
    var numberRegex = /\d/;

    // First Name Validation
    if (firstName != null && numberRegex.test(firstName))
    {
        formContext.ui.setFormNotification(
            "First Name should not contain number.",
            "ERROR",
            "FirstNameValidation"
        );

        eventArgs.preventDefault();
        return;
    }

    // Last Name Validation
    if (lastName != null && numberRegex.test(lastName))
    {
        formContext.ui.setFormNotification(
            "Last Name should not contain numbers.",
            "ERROR",
            "LastNameValidation"
        );

        eventArgs.preventDefault();
        return;
    }
}