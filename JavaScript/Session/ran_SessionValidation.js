function validateSessionDates(executionContext) {

    var formContext = executionContext.getFormContext();

    var startTimeAttr = formContext.getAttribute("ran_starttime");
    var endTimeAttr = formContext.getAttribute("ran_endtime");

    var endTimeCtrl = formContext.getControl("ran_endtime");

    if (!startTimeAttr || !endTimeAttr || !endTimeCtrl) {
        return;
    }

    var startTime = startTimeAttr.getValue();
    var endTime = endTimeAttr.getValue();

    // Clear previous notifications
    formContext.ui.clearFormNotification("INVALID_SESSION_END_TIME");

    // =====================================
    // End Time Read Only Until Start Time
    // =====================================
    if (!startTime) {

        endTimeCtrl.setDisabled(true);

        if (endTime) {
            endTimeAttr.setValue(null);
        }

        return;
    }

    // Enable End Time once Start Time is selected
    endTimeCtrl.setDisabled(false);

    // =====================================
    // End Time Must Be Greater Than Start Time
    // =====================================
    if (endTime) {

        if (endTime <= startTime) {

            endTimeAttr.setValue(null);

            formContext.ui.setFormNotification(
                "Session End Time must be greater than Session Start Time.",
                "ERROR",
                "INVALID_SESSION_END_TIME"
            );

            var eventArgs = executionContext.getEventArgs();

            if (eventArgs) {
                eventArgs.preventDefault();
            }

            return;
        }
    }

    formContext.ui.clearFormNotification("INVALID_SESSION_END_TIME");
}