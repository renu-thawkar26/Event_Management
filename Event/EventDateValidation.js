function validateEventDates(executionContext) {

    var formContext = executionContext.getFormContext();

    var startDateAttr = formContext.getAttribute("ran_startdate");
    var endDateAttr = formContext.getAttribute("ran_enddate");

    var endDateCtrl = formContext.getControl("ran_enddate");

    if (!startDateAttr || !endDateAttr || !endDateCtrl) {
        return;
    }

    var startDate = startDateAttr.getValue();
    var endDate = endDateAttr.getValue();

    // Clear previous notifications
    formContext.ui.clearFormNotification("PAST_START_DATE");
    formContext.ui.clearFormNotification("INVALID_END_DATE");

    // ==================================================
    // End Date Read-Only Until Start Date Is Selected
    // ==================================================
    if (!startDate) {

        endDateCtrl.setDisabled(true);

        if (endDate) {
            endDateAttr.setValue(null);
        }

        return;
    }

    // Enable End Date when Start Date is entered
    endDateCtrl.setDisabled(false);

    // ==================================================
    // Start Date Cannot Be Past Date
    // ==================================================
    var today = new Date();
    today.setHours(0, 0, 0, 0);

    var selectedStartDate = new Date(startDate);
    selectedStartDate.setHours(0, 0, 0, 0);

    if (selectedStartDate < today) {

        startDateAttr.setValue(null);
        endDateAttr.setValue(null);
        endDateCtrl.setDisabled(true);

        formContext.ui.setFormNotification(
            "Event Start Date cannot be a past date.",
            "ERROR",
            "PAST_START_DATE"
        );

        return;
    }

    // ==================================================
    // End Date Must Be Greater Than Start Date
    // ==================================================
    if (endDate) {

        var selectedEndDate = new Date(endDate);

        selectedEndDate.setHours(0, 0, 0, 0);

        if (selectedEndDate <= selectedStartDate) {

            endDateAttr.setValue(null);

            formContext.ui.setFormNotification(
                "Event End Date must be greater than Start Date.",
                "ERROR",
                "INVALID_END_DATE"
            );

            return;
        }
    }

    // Clear notifications when values become valid
    formContext.ui.clearFormNotification("PAST_START_DATE");
    formContext.ui.clearFormNotification("INVALID_END_DATE");
}