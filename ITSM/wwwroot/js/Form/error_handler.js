$(function () {
    
    function showErrorAlert(message) {
        var $errorAlert = $("#inc-cre-error-alert");
        
        if (message) {
            $errorAlert.find(".inc-cre-error-message").text(message);
        }

        $errorAlert.removeClass("hidden");
    }

    function hideErrorAlert() {
        $("#inc-cre-error-alert").addClass("hidden");
    }

    $("#inc-cre-error-close").on("click", function () {
        hideErrorAlert();
    });

    function showErrorNotification(message, title) {
        var $errorNotification = $("#error-notification");
       
        if (message) {
            $errorNotification.find("#error-notification-message").text(message);
        }
        
        if (title) {
            $errorNotification.find(".error-notification-title").text(title);
        }
        
        $errorNotification.removeClass("hidden");
    }

    function hideErrorNotification() {
        $("#error-notification").addClass("hidden");
    }

    $("#error-notification-close").on("click", function () {
        hideErrorNotification();
    });

    $("#incidentForm").on("submit", function (e) {
        
        var isValid = true;
        var errorMessage = "Please fill in all required fields before submitting the form";

        if (!$("#short-description").val().trim()) {
            isValid = false;
            errorMessage = "Please fill in a short description before submitting the form";
        }

        if (!$("#assignment-group-id").val()) {
            isValid = false;
            errorMessage = "Please select the assignment group before submitting the form";
        }

        if (!isValid) {
            e.preventDefault(); 
            showErrorNotification(errorMessage);
            $('html, body').animate({
                scrollTop: $("#error-notification").offset().top - 20
            }, 300);
        } else {
            hideErrorNotification();
        }
    });

    $("input, select, textarea").on("input change", function () {
        hideErrorNotification();
    });

})