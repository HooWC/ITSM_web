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
            url: '/Ajax/AssignedToData',
            type: 'POST',
            data: {
                departmentId: departmentId,
                caller_id: caller_id
            },
            dataType: 'json',
            success: function (data) {
                if (data && Array.isArray(data)) {
                    console.log("进来了")
                    console.log("数据："+data)
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
                currentInputField.attr('href', '/User/Form_User_Info?id=' + selectedId);
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
});