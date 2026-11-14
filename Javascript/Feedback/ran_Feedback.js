// Namespace
var Feedback = Feedback || {};

/**
 * Validation #1
 * Show and enable Comments when Rating is less than 3.
 * Hide and disable Comments when Rating is 3 or more.
 */
Feedback.handleCommentsVisibility = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var ratingAttribute =
        formContext.getAttribute("ran_rating");

    var commentsAttribute =
        formContext.getAttribute("ran_comments");

    var commentsControl =
        formContext.getControl("ran_comments");

    if (!ratingAttribute ||
        !commentsAttribute ||
        !commentsControl) {
        return;
    }

    var rating =
        ratingAttribute.getValue();

    if (rating === null) {

        commentsControl.setVisible(false);
        commentsControl.setDisabled(true);

        return;
    }

    if (rating < 3) {

        commentsControl.setVisible(true);
        commentsControl.setDisabled(false);
    }
    else {

        commentsAttribute.setValue(null);

        commentsControl.setVisible(false);
        commentsControl.setDisabled(true);
    }
};

/**
 * Validation #6
 * Rating must be between 1 and 5.
 */
Feedback.validateRating = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var ratingAttribute =
        formContext.getAttribute("ran_rating");

    if (!ratingAttribute) {
        return;
    }

    var rating =
        ratingAttribute.getValue();

    if (rating === null) {
        return;
    }

    if (rating < 1 || rating > 5) {

        Xrm.Navigation.openAlertDialog({
            text: "Rating must be between 1 and 5."
        });

        ratingAttribute.setValue(null);
    }
};

/**
 * Validation #7
 * Submitted On field should remain read-only.
 */
Feedback.disableSubmittedOn = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var submittedOnControl =
        formContext.getControl("ran_submittedon");

    if (!submittedOnControl) {
        return;
    }

    submittedOnControl.setDisabled(true);
};

/**
 * Validation #9
 * Comments are mandatory when Rating is 1 or 2.
 */
Feedback.validateCommentsRequirement = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var ratingAttribute =
        formContext.getAttribute("ran_rating");

    var commentsAttribute =
        formContext.getAttribute("ran_comments");

    if (!ratingAttribute || !commentsAttribute) {
        return;
    }

    var rating =
        ratingAttribute.getValue();

    if (rating === 1 || rating === 2) {

        commentsAttribute.setRequiredLevel("required");
    }
    else {

        commentsAttribute.setRequiredLevel("none");
    }
};