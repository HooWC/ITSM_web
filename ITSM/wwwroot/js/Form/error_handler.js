$(function () {
    // 错误提示处理
    function showErrorAlert(message) {
        var $errorAlert = $("#inc-cre-error-alert");
        // 如果提供了消息，更新错误消息
        if (message) {
            $errorAlert.find(".inc-cre-error-message").text(message);
        }
        // 显示错误提示
        $errorAlert.removeClass("hidden");
    }

    function hideErrorAlert() {
        $("#inc-cre-error-alert").addClass("hidden");
    }

    // 关闭错误提示的点击事件
    $("#inc-cre-error-close").on("click", function () {
        hideErrorAlert();
    });

    // 高级错误提示处理
    function showErrorNotification(message, title) {
        var $errorNotification = $("#error-notification");
        // 如果提供了消息，更新错误消息
        if (message) {
            $errorNotification.find("#error-notification-message").text(message);
        }
        // 如果提供了标题，更新错误标题
        if (title) {
            $errorNotification.find(".error-notification-title").text(title);
        }
        // 显示错误提示
        $errorNotification.removeClass("hidden");
    }

    function hideErrorNotification() {
        $("#error-notification").addClass("hidden");
    }

    // 关闭高级错误提示的点击事件
    $("#error-notification-close").on("click", function () {
        hideErrorNotification();
    });

    // 表单提交前验证
    $("#incidentForm").on("submit", function (e) {
        // 检查所有必填字段
        var isValid = true;
        var errorMessage = "请填写所有必填字段后再提交表单";

        // 检查短描述
        if (!$("#short-description").val().trim()) {
            isValid = false;
            errorMessage = "请填写短描述后再提交表单";
        }

        // 检查分配组
        if (!$("#assignment-group-id").val()) {
            isValid = false;
            errorMessage = "请选择分配组后再提交表单";
        }

        if (!isValid) {
            e.preventDefault(); // 阻止表单提交
            // 使用新的高级错误提示
            showErrorNotification(errorMessage);
            // 滚动到错误提示位置
            $('html, body').animate({
                scrollTop: $("#error-notification").offset().top - 20
            }, 300);
        } else {
            hideErrorNotification();
        }
    });

    // 当用户开始输入时隐藏错误信息
    $("input, select, textarea").on("input change", function () {
        hideErrorNotification();
    });


})