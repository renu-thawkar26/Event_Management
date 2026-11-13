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







function lockFormOnCompletedOrCancelled(executionContext) {

    var formContext = executionContext.getFormContext();

    var status = formContext.getAttribute("ran_eventstatus");

    if (!status) {
        return;
    }

    var statusValue = status.getValue();

    var lockForm =
        statusValue === 100000002 || // Completed
        statusValue === 100000003;   // Cancelled

    formContext.ui.controls.forEach(function (control) {

        if (control &&
            typeof control.setDisabled === "function") {

            control.setDisabled(lockForm);
        }
    });
}







function validateEventDescription(executionContext) {

    var formContext = executionContext.getFormContext();

    var eventTypeAttr = formContext.getAttribute("ran_eventtype");
    var descriptionAttr = formContext.getAttribute("ran_description");

    if (!eventTypeAttr || !descriptionAttr) {
        return;
    }

    var eventType = eventTypeAttr.getValue();
    var description = descriptionAttr.getValue();

    // Clear existing notification
    formContext.ui.clearFormNotification("DESCRIPTION_REQUIRED");

    // Conference or Workshop
    if (eventType === 100000000 || eventType === 100000002) {

        // Make Description mandatory
        descriptionAttr.setRequiredLevel("required");

        var isDescriptionEmpty =
            description === null ||
            description === undefined ||
            description.trim() === "";

        if (isDescriptionEmpty) {

            formContext.ui.setFormNotification(
                "Description is mandatory for Conference and Workshop events.",
                "ERROR",
                "DESCRIPTION_REQUIRED"
            );

            var eventArgs = executionContext.getEventArgs();

            if (eventArgs) {
                eventArgs.preventDefault();
            }

            return;
        }
    }
    else {

        // Make Description optional
        descriptionAttr.setRequiredLevel("none");
    }

    formContext.ui.clearFormNotification("DESCRIPTION_REQUIRED");
}







async function warnNoSessionsWithin24Hours(executionContext) {

    var formContext = executionContext.getFormContext();

    try {

        formContext.ui.clearFormNotification("NO_SESSION_WARNING");

        var startDate = formContext.getAttribute("ran_startdate").getValue();

        if (!startDate) {
            return;
        }

        var eventId = formContext.data.entity.getId();

        if (!eventId) {
            return;
        }

        eventId = eventId.replace("{", "").replace("}", "");

        var currentDate = new Date();

        var millisecondsDifference = startDate.getTime() - currentDate.getTime();

        var hoursDifference = millisecondsDifference / (1000 * 60 * 60);

        // Check only when event starts within next 24 hours
        if (hoursDifference > 24 || hoursDifference < 0) {
            return;
        }

        var query =
            "?$select=ran_sessionid" +
            "&$filter=_ran_event_value eq " +
            eventId;

        var result = await Xrm.WebApi.retrieveMultipleRecords(
            "ran_session",
            query
        );

        if (result.entities.length === 0) {

            formContext.ui.setFormNotification(
                "Warning: Event starts within 24 hours and no Sessions are configured.",
                "WARNING",
                "NO_SESSION_WARNING"
            );
        }
    }
    catch (error) {

        console.log(
            "Error while checking Session records: ",
            error.message
        );
    }
}





function switchEventBPF(executionContext) {

    var formContext = executionContext.getFormContext();

    var eventTypeAttr = formContext.getAttribute("ran_eventtype");

    if (!eventTypeAttr) {
        return;
    }

    var eventType = eventTypeAttr.getValue();

    if (eventType === null) {
        return;
    }

    // Public Event Process
    var publicBpfId = "0c0bc378-a8db-f111-84e8-6045bdcebf96";

    // Corporate Event Process
    var corporateBpfId = "5a2a0837-b8db-f111-84e8-6045bdcebf96";

    var activeProcess = formContext.data.process.getActiveProcess();

    if (!activeProcess) {
        return;
    }

    var currentProcessId = activeProcess.getId().toLowerCase();

    // Clear previous notification
    formContext.ui.clearFormNotification("BPF_SWITCH");

    // Conference, Webinar, Workshop
    if (
        eventType === 100000000 ||
        eventType === 100000001 ||
        eventType === 100000002
    ) {

        if (currentProcessId !== publicBpfId.toLowerCase()) {

            formContext.data.process.setActiveProcess(
                publicBpfId,
                function (result) {

                    if (result === "success") {

                        formContext.ui.setFormNotification(
                            "Public Event Process has been activated based on the selected Event Type.",
                            "INFO",
                            "BPF_SWITCH"
                        );
                    }
                }
            );
        }
    }

    // Corporate
    else if (eventType === 100000003) {

        if (currentProcessId !== corporateBpfId.toLowerCase()) {

            formContext.data.process.setActiveProcess(
                corporateBpfId,
                function (result) {

                    if (result === "success") {

                        formContext.ui.setFormNotification(
                            "Corporate Event Process has been activated based on the selected Event Type.",
                            "INFO",
                            "BPF_SWITCH"
                        );
                    }
                }
            );
        }
    }
}