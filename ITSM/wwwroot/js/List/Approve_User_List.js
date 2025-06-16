var currentPage = 1;
var itemsPerPage = 15;
var IncidentItems = $('.incident-item').length;
var IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

phone_function()

var currentFilter = 'emp_id';
var currentStatus = 'all';

initPagination();

$(document).click(function (e) {
    if (!$(e.target).closest('.inc-tab-dropdown').length) {
        $('.inc-tab-dropdown-menu').hide();
    }
});

$('#filterDropdown').click(function (e) {
    e.stopPropagation();
    $('#filterMenu').toggle();
    $('#statusMenu').hide();
});

$('#statusDropdown').click(function (e) {
    e.stopPropagation();
    $('#statusMenu').toggle();
    $('#filterMenu').hide();
});

$('.inc-tab-dropdown-item[data-filter]').click(function (e) {
    e.preventDefault();
    var filter = $(this).data('filter');
    $('#filterDropdown').text($(this).text() + ' ');
    $('#filterDropdown').append('<i class="fas fa-chevron-down"></i>');
    $('#filterMenu').hide();

    currentFilter = filter;

    if ($('#searchInput').val().trim() !== '') {
        searchUsers();
    }
});

$('.inc-tab-dropdown-item[data-status]').click(function (e) {
    e.preventDefault();
    var status = $(this).data('status');
    $('#statusDropdown').text($(this).text() + ' ');
    $('#statusDropdown').append('<i class="fas fa-chevron-down"></i>');
    $('#statusMenu').hide();

    currentStatus = status;

    filterByStatus();
});

$('#table-select-all, #select-all').change(function () {
    var isChecked = $(this).prop('checked');
    $('.item-checkbox').prop('checked', isChecked);

    if (this.id === 'table-select-all') {
        $('#select-all').prop('checked', isChecked);
    } else {
        $('#table-select-all').prop('checked', isChecked);
    }
});

$('#firstPageBtn').click(function () {
    if ($(this).prop('disabled')) return;
    currentPage = 1;
    applyPagination();
    updatePaginationInfo();
    updatePaginationButtons();
});

$('#prevPageBtn').click(function () {
    if ($(this).prop('disabled')) return;
    currentPage--;
    applyPagination();
    updatePaginationInfo();
    updatePaginationButtons();
});

$('#nextPageBtn').click(function () {
    if ($(this).prop('disabled')) return;
    currentPage++;
    applyPagination();
    updatePaginationInfo();
    updatePaginationButtons();
});

$('#lastPageBtn').click(function () {
    if ($(this).prop('disabled')) return;
    currentPage = IncidentPages;
    applyPagination();
    updatePaginationInfo();
    updatePaginationButtons();
});

$('#searchInput').on('keyup', function () {
    searchUsers();
});

$('#refreshButton').click(function () {
    location.reload();
});

function initPagination() {
    IncidentItems = $('.incident-item').length;
    IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

    applyPagination();

    updatePaginationInfo();
    updatePaginationButtons();
}

var currentSortOrder = 'asc'; 

$('#sortByNumber').click(function () {
    currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';

    if (currentSortOrder === 'asc') {
        $('#sortIcon').removeClass('fa-arrow-down').addClass('fa-arrow-up');
    } else {
        $('#sortIcon').removeClass('fa-arrow-up').addClass('fa-arrow-down');
    }

    sortUsers();
});

function sortUsers() {

    $.ajax({
        url: '/Ajax/SortUser',
        method: 'GET',
        data: {
            sortOrder: currentSortOrder
        },
        success: function (data) {
            updateUserTable(data);
            resetPagination();
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Sort Error:', error);
        }
    });
}

function checkSelection() {
    var hasChecked = $('.item-checkbox:checked').length > 0;
    $('#deleteButton').prop('disabled', !hasChecked);
    $('#approveButton').prop('disabled', !hasChecked);
}

$(document).on('change', '.item-checkbox', function () {
    checkSelection();
});

$(document).on('change', '#table-select-all, #select-all', function () {
    var isChecked = $(this).prop('checked');
    $('.item-checkbox').prop('checked', isChecked);

    if (this.id === 'table-select-all') {
        $('#select-all').prop('checked', isChecked);
    } else {
        $('#table-select-all').prop('checked', isChecked);
    }

    checkSelection();
});

$('#deleteButton').click(function (e) {
    e.stopPropagation();

    const overlay = document.getElementById('error-box-overlay');
    overlay.classList.remove('hidden');

    const deleteButton = document.querySelector('.error-box-button-box');
    deleteButton.addEventListener('click', function () {
        DeleteItem();
        overlay.classList.add('hidden');
    });

    overlay.addEventListener('click', function () {
        overlay.classList.add('hidden');
    });

    const errorBox = document.getElementById('error-box');
    errorBox.addEventListener('click', function (e) {
        e.stopPropagation();
    });

});

function DeleteItem() {
    if ($(this).prop('disabled')) return;

    // Get all selected todo IDs
    var selectedIds = [];
    $('.item-checkbox:checked').each(function () {
        var todoId = $(this).closest('tr').attr('data-id');
        if (todoId) {
            selectedIds.push(parseInt(todoId));
        }
    });

    // If no items selected, do nothing
    if (selectedIds.length === 0) {
        return;
    }

    // Send delete request
    $.ajax({
        url: '/Ajax/DeleteUser',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (response) {
            if (response.success) {
                // Remove deleted items from table
                $('.item-checkbox:checked').closest('tr').remove();

                // Uncheck select-all checkboxes
                $('#select-all, #table-select-all').prop('checked', false);

                // Disable delete button
                $('#deleteButton').prop('disabled', true);

                // Reset pagination
                resetPagination();
            }
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Delete Error:', error);
        }
    });
}

// Approve button click event
$('#approveButton').click(function () {
    if ($(this).prop('disabled')) return;

    var selectedIds = [];
    $('.item-checkbox:checked').each(function () {
        var todoId = $(this).closest('tr').attr('data-id');
        if (todoId) {
            selectedIds.push(parseInt(todoId));
        }
    });

    if (selectedIds.length === 0) {
        return;
    }

    $.ajax({
        url: '/Ajax/ApproveUsers',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (data) {
            updateUserTable(data);
            resetPagination();
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Delete Error:', error);
        }
    });
});

function applyPagination() {
    $('.incident-item').hide();

    var start = (currentPage - 1) * itemsPerPage;
    var end = start + itemsPerPage;

    $('.incident-item').each(function (index) {
        if (index >= start && index < end) {
            $(this).show();
        }
    });
}

function updatePaginationInfo() {
    var IncidentItems = $('.incident-item').length;

    if (IncidentItems === 0) {
        $('#paginationInfo').text('0 items');
        return;
    }

    var start = (currentPage - 1) * itemsPerPage + 1;
    var end = Math.min(currentPage * itemsPerPage, IncidentItems);

    $('#paginationInfo').text(start + ' to ' + end + ' of ' + IncidentItems);
}

function resetPagination() {
    currentPage = 1;

    IncidentItems = $('.incident-item').length;
    IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

    applyPagination();
    updatePaginationInfo();
    updatePaginationButtons();
}

function updatePaginationButtons() {
    $('#firstPageBtn, #prevPageBtn').prop('disabled', currentPage === 1);
    $('#nextPageBtn, #lastPageBtn').prop('disabled', currentPage === IncidentPages || IncidentItems === 0);
}

function searchUsers() {
    var searchTerm = $('#searchInput').val().trim();
    if (searchTerm === '') {
        searchTerm = "re_entrynovalue";
    }

    $.ajax({
        url: '/Ajax/SearchUser',
        method: 'GET',
        data: {
            searchTerm: searchTerm,
            filterBy: currentFilter
        },
        success: function (data) {
            updateUserTable(data);
            if (currentStatus !== 'all') {
                filterTableByStatus(currentStatus);
            }

            resetPagination();
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Search Error:', error);
        }
    });
}

function filterByStatus() {
    if (currentStatus === 'all') {
        if ($('#searchInput').val().trim() !== '') {
            searchUsers();
        } else {
            location.reload();
        }
        return;
    }

    $.ajax({
        url: '/Ajax/FilterUserByStatus',
        method: 'GET',
        data: {
            status: currentStatus
        },
        success: function (data) {
            updateUserTable(data);

            resetPagination();
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Filter Error:', error);
        }
    });
}

function filterTableByStatus(status) {
    if (status === 'all') return;

    var rows = $('#incTableBody tr');
    rows.each(function () {
        var statusCell = $(this).find('td:last-child').text().trim();
        if ((status === 'doing' && statusCell === 'Doing') ||
            (status === 'completed' && statusCell === 'Completed')) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });

    resetPagination();
}

function updateUserTable(data) {
    var tableBody = $('#incTableBody');
    tableBody.empty();

    if (data.length === 0) {
        tableBody.append('<tr><td colspan="7" class="text-center">No matching User found</td></tr>');
        IncidentItems = 0;
        IncidentPages = 0;
        updatePaginationInfo();
        updatePaginationButtons();
        return;
    }

    $.each(data, function (index, u) {
        var row = `
                    <tr class="incident-item" data-id="${u.id}">
                        <td><input type="checkbox" class="item-checkbox"></td>
                        <td class="inc-tab-incident-number" data-label="Number">
                            <a href="/User/User_Info?id=${u.id}">${u.emp_id}</a>
                        </td>
                        <td data-label="Fullname">${u.fullname}</td>
                        <td data-label="Gender" class="phone_user_hide_design">${u.gender}</td>
                        <td data-label="D_Name">${u.departmentName}</td>
                        <td data-label="Mobile_Phone" class="phone_user_hide_design">${u.mobile_phone}</td>
                        <td data-label="approve" class="phone_user_hide_design">${u.approve}</td>
                    </tr>
                `;
        tableBody.append(row);
        phone_function()
    });

    initPagination();

    checkSelection();
}

function errorLogin(error) {
    const msg = error?.responseJSON?.message;

    if (msg === "Not logged in") {
        window.location.href = "/Auth/Login";
    }
}

function phone_function() {
    // Phone Design
    if (/Mobi|Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        $(".phone_user_hide_design").hide();
        $('.inc-tab-incidents-table th.number-column').css('width', '28%');
        $(".inc-tab-incidents-table th.u_fullname").css('width', '32%');
        $(".inc-tab-incidents-table th.u_department").css('width', '24%');
        $(".inc-tab-incidents-table th.u_active").css('width', '16%');
        $(".all-title-header-front").css('font-size', '1rem');
    }
}
