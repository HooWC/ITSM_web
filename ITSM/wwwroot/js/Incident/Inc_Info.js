$(function () {
    var isResolved = incidentState === "Resolved";
    var isClosed = incidentState === "Closed";

    var caller_id = $("#caller_id").val();

    // Binding category change event
    $('#category').on('change', function() {
        const categoryId = parseInt(this.value);
        const subcategorySelect = document.getElementById('subcategory');

        subcategorySelect.innerHTML = '';

        subcategories
            .filter(sub => sub.category === categoryId)
            .forEach(sub => {
                const option = document.createElement('option');
                option.value = sub.id;
                option.text = sub.subcategory;
                subcategorySelect.appendChild(option);
            });
    });

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
    var selectedDepartmentId = incidentAssignmentGroup;
    var departmentList = [];
    var usersByDepartment = {};

    function loadDepartments() {
        $.ajax({
            url: window.AppRoot + 'Ajax/DepartmentData',
            type: 'GET',
            dataType: 'json',
            success: function(data) {
                departmentList = data;
            },
            error: function(xhr, status, error) {
                console.error("Failed to load department data:", error);
            }
        });
    }

    loadDepartments();

    function loadUsersByDepartment(departmentId) {
        $.ajax({
            url: window.AppRoot + 'Ajax/AssignedToData',
            type: 'POST',
            data: {
                departmentId: departmentId,
                caller_id: caller_id
            },
            dataType: 'json',
            success: function(data) {
                if (data && Array.isArray(data)) {
                    usersByDepartment[departmentId] = data;
                    openModal("Select Assign Person", data, $("#assigned-to"));
                }
            },
            error: function(xhr, status, error) {
                console.error("Failed to obtain user data request:", error);
            }
        });
    }

    // Initialize the subcategory drop-down list
    function initializeSubcategories() {
        var allSubcategories = subcategoriesList;
        var currentSubcategoryId = incidentSubcategory;

        function updateSubcategories(categoryId) {
            var subcategorySelect = $('#subcategory');
            subcategorySelect.empty();

            if (categoryId) {
                var filteredSubcategories = allSubcategories.filter(function(sub) {
                    return sub.category === categoryId;
                });

                filteredSubcategories.forEach(function(sub) {
                    subcategorySelect.append($('<option>', {
                        value: sub.id,
                        text: sub.subcategory,
                        selected: sub.id === currentSubcategoryId
                    }));
                });
            }
        }

        var currentCategoryId = parseInt($('#category').val());
        updateSubcategories(currentCategoryId);
    }

    initializeSubcategories();

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

    // Loading department data
    loadDepartments();

    function updateActivityCount() {
        var count = $(".inc-cre-activity-container .inc-cre-activity-item").length;
        $("#activity-count").text(count);
    }

    updateActivityCount();

    //$("#post-note-btn").click(function() {
    //    if (isResolved) {
    //        return;
    //    }

    //    var workNotes = $("#work-notes").val().trim();

    //    if (workNotes) {
    //        $(this).prop("disabled", true);

    //        var incidentId = $("#id").val();

    //        $.ajax({
    //            url: window.AppRoot + 'Ajax/AddNote',
    //            type: 'POST',
    //            data: {
    //                incidentId: incidentId,
    //                message: workNotes
    //            },
    //            dataType: 'json',
    //            success: function(response) {
    //                $("#post-note-btn").prop("disabled", false);

    //                if (response.success) {
    //                    var dateString = response.note.create_date;

    //                    var newActivity = $('<div class="inc-cre-activity-item">' +
    //                        '<div class="inc-cre-user-avatar">' +
    //                        '<img src="' + userPhotoData + '" alt="' + response.note.user_name + '" class="w-px-40 rounded-circle">' +
    //                        '</div>' +
    //                        '<div class="inc-cre-activity-content">' +
    //                        '<div class="inc-cre-activity-header">' +
    //                        '<span class="inc-cre-user-name"><a href="/User/Form_User_Info" target="_blank">' + response.note.user_name + '</a></span>' +
    //                        '<span class="inc-cre-activity-time">Work notes • ' + dateString + '</span>' +
    //                        '</div>' +
    //                        '<div class="inc-cre-activity-details">' +
    //                        '<div class="inc-cre-work-note-content">' + response.note.message.replace(/\n/g, '<br>') + '</div>' +
    //                        '</div>' +
    //                        '</div>' +
    //                        '</div>');

    //                    $(".inc-cre-activity-container").prepend(newActivity);
    //                    $("#work-notes").val('');
    //                    updateActivityCount();
    //                }
    //            },
    //            error: function(xhr, status, error) {
    //                $("#post-note-btn").prop("disabled", false);
    //                console.error("Add note request failed:", error);
    //            }
    //        });
    //    }
    //});

    //function loadNotes() {
    //    var incidentId = $("#id").val();

    //    $.ajax({
    //        url: window.AppRoot + 'Ajax/GetNotesByIncident',
    //        type: 'GET',
    //        data: { incidentId: incidentId },
    //        dataType: 'json',
    //        success: function(response) {
    //            if (response.success && response.notes) {
    //                $(".inc-cre-activity-container").empty();

    //                var isResolvedOrClosed = incidentState === "Resolved" || incidentState === "Closed";
    //                if (isResolvedOrClosed && (!response.notes || response.notes.length === 0)) {
    //                    $(".inc-cre-activity-container").html('<div class="inc-cre-no-history" style="text-align: center; padding: 30px; color: #6c757d; font-style: italic;">No chat history available</div>');
    //                    return;
    //                }

    //                $.each(response.notes, function (i, note) {
    //                    var noteItem = $('<div class="inc-cre-activity-item">' +
    //                        '<div class="inc-cre-user-avatar">' +
    //                        '<img src="' + (note.user_avatar ? 'data:image/jpeg;base64,' + note.user_avatar : 'https://www.diydoutu.com/touxiang/20201222-1.jpg') + '" alt="' + note.user_name + '">' +
    //                        '</div>' +
    //                        '<div class="inc-cre-activity-content">' +
    //                        '<div class="inc-cre-activity-header">' +
    //                        '<span class="inc-cre-user-name"><a class="note_username_a" href="/User/Form_User_Info?id=' + note.user_id + '" target="_blank">' + note.user_name + '</a></span>' +
    //                        '<span class="inc-cre-activity-time">Work notes • ' + note.create_date + '</span>' +
    //                        '</div>' +
    //                        '<div class="inc-cre-activity-details">' +
    //                        '<div class="inc-cre-work-note-content">' + note.message.replace(/\n/g, '<br>') + '</div>' +
    //                        '</div>' +
    //                        '</div>' +
    //                        '</div>');

    //                    $(".inc-cre-activity-container").append(noteItem);
    //                });

    //                updateActivityCount();
    //            }
    //        },
    //        error: function(xhr, status, error) {
    //            console.error("Load note request failed:", error);
    //        }
    //    });
    //}

    //loadNotes();

    $("#resolve-button, #resolve-button-footer").click(function () {
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
            url: window.AppRoot + 'Ajax/ResolveIncident',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    reback_page();
                } else {
                    alert("Failed to resolve incident: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                // alert("Error: " + error);
            }
        });
    });

    function loadResolutionHistory() {
        var incidentId = $("#id").val();

        $.ajax({
            url: window.AppRoot + 'Ajax/GetResolutionHistory',
            type: 'GET',
            data: { incidentId: incidentId },
            dataType: 'json',
            success: function (response) {
                $("#resolution-loading").hide();

                if (response.success && response.resolution) {
                    var res = response.resolution;
                    var userAvatar = res.resolved_by_avatar ? 'data:image/jpeg;base64,' + res.resolved_by_avatar : '/img/avatar/user_avatar.jpg';

                    var historyItem = $('<div class="inc-cre-resolve-item">' +
                        '<div class="inc-cre-resolve-content">' +
                        '<div class="inc-cre-resolve-header">' +
                        '<span class="inc-cre-resolve-type">' + res.resolved_type + '</span>' +
                        '<span class="inc-cre-resolve-time">' + res.resolved_date + '</span>' +
                        '</div>' +
                        '<div class="inc-cre-resolve-user">' +
                        '<img src="' + userAvatar + '" alt="Avatar" class="inc-cre-resolve-user-avatar" />' +
                        '<span>Resolved By: ' + res.resolved_by_name + '</span>' +
                        '</div>' +
                        '<div class="inc-cre-resolve-details">' + res.resolution.replace(/\n/g, '<br>') + '</div>' +
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

    $("#close-button").click(function () {
        console.log("c1");
        if (isResolved || isClosed) {
            return;
        }

        var formData = $("#incidentForm").serialize();

        $.ajax({
            url: window.AppRoot + 'Ajax/CloseIncident',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    reback_page();
                } else {
                    //alert("Close event failed: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                //alert("Error: " + error);
            }
        });
    });

    $("#reopen-button").click(function () {
        var formData = $("#incidentForm").serialize();

        $.ajax({
            url: window.AppRoot + 'Ajax/ReopenIncident',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    reback_page();
                } else {
                    alert("Reopen event failed: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                //alert("Error: " + error);
            }
        });
    });

    function reback_page() {
        let roleBack = $('#roleBack').val();
        if (roleBack.includes("Admin")) {
            window.location.href = '/IncidentManagement/All';
        }
        else if (roleBack.includes("Resolved")) {
            window.location.href = '/IncidentManagement/Resolved_Assigned_To_Me';
        }
        else if (roleBack.includes("ToMe")) {
            window.location.href = '/IncidentManagement/Assigned_To_Me';
        }
        else if (roleBack.includes("ToGroup")) {
            window.location.href = '/IncidentManagement/Assigned_To_Group';
        }
        else if (roleBack.includes("Closed")) {
            window.location.href = '/IncidentManagement/Closed_Assigned_To_Me';
        }
        else if (roleBack.includes("Message")) {
            window.location.href = '/IncidentManagement/Inc_Message';
        }
        else {
            window.location.href = '/IncidentManagement/User_All';
        }
    }

    // initialization Lightbox
    lightbox.option({
        'resizeDuration': 200,
        'wrapAround': true,
        'albumLabel': "图片 %1 / %2",
        'fadeDuration': 300,
        'imageFadeDuration': 300,
        'positionFromTop': 50,
        'disableScrolling': true,
        'showImageNumberLabel': true,
        'alwaysShowNavOnTouchDevices': true,
        'fitImagesInViewport': true,
        'maxWidth': 1200,
        'maxHeight': 800,
        'showImageNumberLabel': false,
        'fadeDuration': 200,
        'imageFadeDuration': 200
    });

    $(document).on('click', '[data-lightbox]', function () {
        setTimeout(function () {
            if ($('.lb-closeBtn').length === 0) {
                $('.lb-container').append('<button class="lb-closeBtn" title="CLose"></button>');
            }
        }, 100);
    });

    // Click the close button to close lightbox
    $(document).on('click', '.lb-closeBtn', function () {
        lightbox.end();
    });

    // Click on the background to close lightbox
    $(document).on('click', '.lb-overlay', function () {
        lightbox.end();
    });

    // ESC close lightbox
    $(document).keyup(function (e) {
        if (e.key === "Escape") {
            lightbox.end();
        }
    });

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
});