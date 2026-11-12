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







function validateSessionDescription(executionContext) {

    var formContext = executionContext.getFormContext();

    var sessionTypeAttr = formContext.getAttribute("ran_sessiontype");
    var sessionDescAttr = formContext.getAttribute("ran_sessiondescription");

    if (!sessionTypeAttr || !sessionDescAttr) {
        return;
    }

    var sessionType = sessionTypeAttr.getValue();
    var sessionDescription = sessionDescAttr.getValue();

    // Clear previous notification
    formContext.ui.clearFormNotification("SESSION_DESCRIPTION_REQUIRED");

    // Workshop
    if (sessionType === 100000002) {

        // Show red star
        sessionDescAttr.setRequiredLevel("required");

        var isDescriptionEmpty =
            sessionDescription === null ||
            sessionDescription === undefined ||
            sessionDescription.trim() === "";

        if (isDescriptionEmpty) {

            formContext.ui.setFormNotification(
                "Session Description is mandatory for Workshop sessions.",
                "ERROR",
                "SESSION_DESCRIPTION_REQUIRED"
            );

            var eventArgs = executionContext.getEventArgs();

            if (eventArgs) {
                eventArgs.preventDefault();
            }

            return;
        }
    }
    else {

        // Remove red star for non-workshop sessions
        sessionDescAttr.setRequiredLevel("none");
    }

    formContext.ui.clearFormNotification("SESSION_DESCRIPTION_REQUIRED");
}







function lockSpeakerOnCompleted(executionContext) {

    var formContext = executionContext.getFormContext();

    var sessionStatusAttr = formContext.getAttribute("ran_sessionstatus");
    var speakerCtrl = formContext.getControl("ran_speaker");

    if (!sessionStatusAttr || !speakerCtrl) {
        return;
    }

    var sessionStatus = sessionStatusAttr.getValue();

    if (sessionStatus === null || sessionStatus === undefined) {

        speakerCtrl.setDisabled(false);
        return;
    }

    // Completed
    if (sessionStatus === 100000002) {

        speakerCtrl.setDisabled(true);
    }
    else {

        speakerCtrl.setDisabled(false);
    }
}