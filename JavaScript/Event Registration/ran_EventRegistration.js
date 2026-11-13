// Namespace
var EventRegistration = EventRegistration || {};

// Store original status value
EventRegistration.originalStatus = null;

/**
 * Validation #1
 * When an Event is selected, automatically fill
 * Event Start Date and Event End Date from
 * the selected Event record.
 * If Event is removed, clear both date fields.
 */
EventRegistration.populateEventDates = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var eventLookup =
        formContext.getAttribute("ran_event");

    if (!eventLookup || eventLookup.getValue() === null) {

        formContext.getAttribute("ran_eventstartdate")
            .setValue(null);

        formContext.getAttribute("ran_eventenddate")
            .setValue(null);

        return;
    }

    var eventId =
        eventLookup.getValue()[0].id
            .replace("{", "")
            .replace("}", "");

    Xrm.WebApi.retrieveRecord(
        "ran_event",
        eventId,
        "?$select=ran_startdate,ran_enddate"
    ).then(
        function success(result) {

            if (result.ran_startdate) {

                formContext
                    .getAttribute("ran_eventstartdate")
                    .setValue(new Date(result.ran_startdate));
            }

            if (result.ran_enddate) {

                formContext
                    .getAttribute("ran_eventenddate")
                    .setValue(new Date(result.ran_enddate));
            }
        },
        function (error) {

            console.log(error.message);
        }
    );
};

/**
 * Validation #3
 * Prevent registration if selected Event
 * is Completed or Cancelled.
 */
EventRegistration.validateEventStatus = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var eventLookup =
        formContext.getAttribute("ran_event");

    if (!eventLookup || eventLookup.getValue() === null) {
        return;
    }

    var eventId =
        eventLookup.getValue()[0].id
            .replace("{", "")
            .replace("}", "");

    Xrm.WebApi.retrieveRecord(
        "ran_event",
        eventId,
        "?$select=ran_eventstatus"
    ).then(
        function success(result) {

            var eventStatus =
                result.ran_eventstatus;

            if (
                eventStatus === 100000002 || // Completed
                eventStatus === 100000003    // Cancelled
            ) {
                Xrm.Navigation.openAlertDialog({
                    text: "Registration is not allowed because the selected Event is Cancelled or Completed."
                });

                formContext.getAttribute("ran_event")
                    .setValue(null);

                formContext.getAttribute("ran_eventstartdate")
                    .setValue(null);

                formContext.getAttribute("ran_eventenddate")
                    .setValue(null);
            }
        },
        function (error) {

            console.log(error.message);
        }
    );
};

/**
 * Validation #7
 * Store current Registration Status on Form Load.
 */
EventRegistration.storeOriginalStatus = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var statusAttribute =
        formContext.getAttribute("ran_status");

    if (!statusAttribute) {
        return;
    }

    EventRegistration.originalStatus =
        statusAttribute.getValue();
};

/**
 * Validation #7
 * Prevent status change from
 * Cancelled -> Registered.
 */
EventRegistration.validateStatusRollback = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var statusAttribute =
        formContext.getAttribute("ran_status");

    if (!statusAttribute) {
        return;
    }

    var currentStatus =
        statusAttribute.getValue();

    var registeredStatus = 100000000;
    var cancelledStatus = 100000001;

    if (
        EventRegistration.originalStatus === cancelledStatus &&
        currentStatus === registeredStatus
    ) {

        Xrm.Navigation.openAlertDialog({
            text: "A cancelled registration cannot be changed back to Registered."
        });

        statusAttribute.setValue(cancelledStatus);
    }
};

/**
 * Validation #6
 * Show error if registration
 * is attempted after Event Start Date and Time.
 */
EventRegistration.validateRegistrationDeadline = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var eventStartDate =
        formContext.getAttribute("ran_eventstartdate").getValue();

    if (!eventStartDate) {
        return;
    }

    var currentDateTime = new Date();

    if (currentDateTime >= eventStartDate) {

        formContext.ui.setFormNotification(
            "Registration cannot be created after the Event Start Date.",
            "ERROR",
            "registrationDeadline"
        );
    }
    else {

        formContext.ui.clearFormNotification(
            "registrationDeadline"
        );
    }
};


/**
 * Validation #6
 * Prevent Save after Event Start Date and Time.
 */
EventRegistration.preventLateRegistration = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var eventArgs =
        executionContext.getEventArgs();

    var eventStartDate =
        formContext.getAttribute("ran_eventstartdate").getValue();

    if (!eventStartDate) {
        return;
    }

    var currentDateTime = new Date();

    if (currentDateTime >= eventStartDate) {

        eventArgs.preventDefault();

        Xrm.Navigation.openAlertDialog({
            text: "Registration cannot be created after the Event Start Date."
        });
    }
};