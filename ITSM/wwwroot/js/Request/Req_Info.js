$(function () {
    var isResolved = requestState === "Completed";
    var isClosed = requestState === "Rejected";

    var caller_id = $("#caller_id").val();

    if (isResolved || isClosed) {
        $("#category, #subcategory, #state, #impact, #urgency").prop("disabled", true);
        $("#assignment-group-search, #assigned-to-search").prop("disabled", true);
        $(".inc-cre-work-notes-area").hide();
        $(".inc-cre-watch-list").hide();
        $(".inc-cre-activities-section").addClass("resolved-mode");
        $(".inc-cre-activities-section h3").text("Chat History");
        $(".inc-cre-tab-section:first .inc-cre-tab").text("Chat History");
        $(".inc-cre-resolve-container .inc-cre-notes-header").hide();
        $("#resolve-input-area").hide();
        $("#resolution-history-section").show().css({
            'padding': '20px',
            'border-radius': '10px',
            'margin-top': '20px',
            'box-shadow': '0 2px 5px rgba(0,0,0,0.05)'
        });
        $(".inc-cre-tab-section:last .inc-cre-tab").text("Resolution Details");
        loadResolutionHistory();
    }

    var currentInputField = null;
    var selectedDepartmentId = requestAssignmentGroup;
    var departmentList = [];
    var usersByDepartment = {};

    function loadUsersByDepartment(departmentId) {
        $.ajax({
            url: window.AppRoot + 'Ajax/AssignedToData',
            type: 'POST',
            data: {
                departmentId: departmentId,
                caller_id: caller_id
            },
            dataType: 'json',
            success: function (data) {
                if (data && Array.isArray(data)) {
                    usersByDepartment[departmentId] = data;
                    openModal("Select Assign Person", data, $("#assigned-to"));
                }
            },
            error: function (xhr, status, error) {
                console.error("Failed to obtain user data request:", error);
            }
        });
    }


    // Open modal
    function openModal(title, data, targetInput) {
        currentInputField = targetInput;

        $("#inc-cre-modal-title").text(title);

        var tableBody = $("#inc-cre-modal-table-body");
        tableBody.empty();

        $.each(data, function (index, item) {
            var id = item.id || item.user_id || index;
            var name = item.name || item.fullname || '';
            var description = item.description || item.title || '';

            tableBody.append(
                '<tr data-id="' + id + '" data-name="' + name + '">' +
                '<td>' + name + '</td>' +
                '<td>' + description + '</td>' +
                '</tr>'
            );
        });

        // Show modal
        $("#inc-cre-modal").fadeIn(200);
    }

    // Close the modal
    function closeModal() {
        $("#inc-cre-modal").fadeOut(200);
        currentInputField = null;
    }

    $("#assignment-group-search").on('click', function () {
        if (isResolved) {
            return;
        }

        if (departmentList && departmentList.length > 0) {
            openModal("Select Assignment Group", departmentList, $("#assignment-group"));
        } else {
            loadDepartments();
            setTimeout(function () {
                if (departmentList && departmentList.length > 0) {
                    openModal("Select Assignment Group", departmentList, $("#assignment-group"));
                } else {
                    // Unable to load department data, please refresh the page and try againalert("Unable to load department data, please refresh the page and try again");
                }
            }, 500);
        }
    });

    $("#assigned-to-search").on('click', function () {
        if (isResolved) {
            return; // If resolved, do nothing
        }

        if (!selectedDepartmentId) {
            // alert("Please select the allocation group first");
            return;
        }

        if (usersByDepartment[selectedDepartmentId]) {
            openModal("Select Assign Person", usersByDepartment[selectedDepartmentId], $("#assigned-to"));
        } else {
            loadUsersByDepartment(selectedDepartmentId);
        }
    });

    $(".inc-cre-modal-close").click(function () {
        closeModal();
    });

    $(document).on('click', '.inc-cre-modal', function (e) {
        if ($(e.target).hasClass('inc-cre-modal')) {
            closeModal();
        }
    });

    $(document).on('click', '.inc-cre-modal-table tbody tr', function () {
        var selectedName = $(this).data('name');
        var selectedId = $(this).data('id');

        if (currentInputField) {
            if (currentInputField.attr('id') === 'assignment-group') {
                selectedDepartmentId = selectedId;
                $("#assignment-group-id").val(selectedId);
                $("#assigned-to-search").prop('disabled', false);
                currentInputField.val(selectedName);
                $("#assigned-to").html('');
                $("#assigned-to-id").val('');
            } else if (currentInputField.attr('id') === 'assigned-to') {
                $("#assigned-to-id").val(selectedId);
                currentInputField.html(selectedName);
                currentInputField.attr('href', window.AppRoot + 'User/Form_User_Info?id=' + selectedId);
            }
        }

        closeModal();
    });

    // Search functionality in a modal
    $("#inc-cre-search-input").on('keyup', function () {
        var value = $(this).val().toLowerCase();
        $(".inc-cre-modal-table tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    function updateActivityCount() {
        var count = $(".inc-cre-activity-container .inc-cre-activity-item").length;
        $("#activity-count").text(count);
    }

    updateActivityCount();

    $("#Completed_req").click(function () {
        if (isResolved) {
            // alert("This incident has been resolved and cannot be resolved again");
            return;
        }

        var resolveType = $("#resolve-type").val();
        var resolveNotes = $("#resolve-notes").val().trim();

        if (!resolveType) {
            alert("Please select a resolution type");
            return;
        }

        if (!resolveNotes) {
            alert("Please enter resolution details");
            return;
        }

        var formData = $("#incidentForm").serialize();
        formData += "&resolveType=" + encodeURIComponent(resolveType);
        formData += "&resolveNotes=" + encodeURIComponent(resolveNotes);

        $.ajax({
            url: window.AppRoot + 'Ajax/CompletedRequest',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    reback_page();
                } else {
                    // alert("Error: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                // alert("Error: " + error);
            }
        });
    });

    $("#Rejected_req").click(function () {

        let reqId = $('#reqId').val();

        $.ajax({
            url: window.AppRoot + 'Ajax/RejectedRequest',
            type: 'POST',
            data: {
                req_id: reqId
            },
            success: function (response) {
                if (response.success) {
                    reback_page();
                } else {
                    // alert("Error: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                // alert(": " + error);
            }
        });
    });

    $("#reopen-button").click(function () {

        let reqId = $('#reqId').val();

        $.ajax({
            url: window.AppRoot + 'Ajax/ReopenRequest',
            type: 'POST',
            data: {
                req_id: reqId
            },
            success: function (response) {
                if (response.success) {
                    reback_page();
                } else {
                    // alert("Error: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                // alert(": " + error);
            }
        });
    });

    function loadResolutionHistory() {
        var reqId = $("#id").val();

        $.ajax({
            url: window.AppRoot + 'Ajax/GetReqResolutionHistory',
            type: 'GET',
            data: { reqId: reqId },
            dataType: 'json',
            success: function (response) {
                $("#resolution-loading").hide();

                if (response.success && response.erp_resolution) {
                    var res = response.erp_resolution;
                    var userAvatar = res.resolved_by_avatar ? 'data:' + res.resolved_photo_type + ';base64,' + res.resolved_by_avatar : window.AppRoot + 'img/avatar/user_avatar.jpg';

                    var historyItem = $('<div class="inc-cre-resolve-item">' +
                        '<div class="inc-cre-resolve-content">' +
                        '<div class="inc-cre-resolve-header">' +
                        '<span class="inc-cre-resolve-type">' + res.erp_resolution_type + '</span>' +
                        '<span class="inc-cre-resolve-time">' + res.erp_resolved_date + '</span>' +
                        '</div>' +
                        '<div class="inc-cre-resolve-user">' +
                        '<img src="' + userAvatar + '" alt="Avatar" class="inc-cre-resolve-user-avatar" />' +
                        '<span>Resolved By: <a href="' + window.AppRoot + 'User/Form_User_Info?id=' + res.user_id + '" target="_blank">' + res.resolved_by_name + '</a></span>' +
                        '</div>' +
                        '<div class="inc-cre-resolve-details">' + res.erp_resolution.replace(/\n/g, '<br>') + '</div>' +
                        '</div>' +
                        '</div>');

                    $("#resolution-history-content").html(historyItem);
                } else {
                    $("#resolution-history-content").html('<div class="text-center">Unable to load solution history</div>');
                    console.error("Failed to load solution history:", response.message);
                }
            },
            error: function (xhr, status, error) {
                $("#resolution-loading").hide();
                $("#resolution-history-content").html('<div class="text-center">Loading failed</div>');
                //console.error("Failed to load solution history request:", error);
            }
        });
    }

    window.showImage = function (src) {
        var modal = document.getElementById('imageModal');
        var modalImg = document.getElementById('modalImage');
        modal.style.display = "block";
        modalImg.src = src;
    }

    $('.image-close-btn').click(function () {
        $('#imageModal').hide();
    });

    $('#imageModal').click(function (e) {
        if (e.target === this || e.target.id === 'modalImage') {
            $(this).hide();
        }
    });

    $(document).keydown(function (e) {
        if (e.key === 'Escape') {
            $('#imageModal').hide();
        }
    });

    function reback_page() {
        let roleBack = $('#roleBack_Name').val();
        if (roleBack.includes("Admin")) {
            window.location.href = window.AppRoot + 'Request/All';
        }
        else if (roleBack.includes("User")) {
            window.location.href = window.AppRoot + 'Request/User_All';
        }
        else if (roleBack.includes("Tome")) {
            window.location.href = window.AppRoot + 'Request/Assigned_To_Me';
        }
        else if (roleBack.includes("AssignWork")) {
            window.location.href = window.AppRoot + 'Request/Manager_Assign_Work';
        }
        else {
            window.location.href = window.AppRoot + 'Request/User_All';
        } 
    }
});