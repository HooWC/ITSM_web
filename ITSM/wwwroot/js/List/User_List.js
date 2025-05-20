var currentPage = 1;
var itemsPerPage = 18;
var IncidentItems = $('.incident-item').length;
var IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

let searchFunctionName = $('#forAjaxGetFunctionName_search').text();
let sortFunctionName = $('#forAjaxGetFunctionName_sort').text();
/*let filterFunctionName = $('#forAjaxGetFunctionName_filter').text();*/
let DeleteFunctionName = $('#forAjaxGetFunctionName_delete').text();

// Set default filter field and status
var currentFilter = 'emp_id';
var currentStatus = 'all';

// Initialize pagination
initPagination();

// Close all dropdown menus when clicking outside
$(document).click(function (e) {
    if (!$(e.target).closest('.inc-tab-dropdown').length) {
        $('.inc-tab-dropdown-menu').hide();
    }
});

// Toggle filter dropdown menu
$('#filterDropdown').click(function (e) {
    e.stopPropagation();
    $('#filterMenu').toggle();
    $('#statusMenu').hide();
});

// Toggle status dropdown menu
$('#statusDropdown').click(function (e) {
    e.stopPropagation();
    $('#statusMenu').toggle();
    $('#filterMenu').hide();
});

// Click filter option
$('.inc-tab-dropdown-item[data-filter]').click(function (e) {
    e.preventDefault();
    var filter = $(this).data('filter');
    $('#filterDropdown').text($(this).text() + ' ');
    $('#filterDropdown').append('<i class="fas fa-chevron-down"></i>');
    $('#filterMenu').hide();

    // Update current filter field
    currentFilter = filter;

    // If search box is not empty, perform search
    if ($('#searchInput').val().trim() !== '') {
        searchUsers();
    }
});

// Click status option
$('.inc-tab-dropdown-item[data-status]').click(function (e) {
    e.preventDefault();
    var status = $(this).data('status');
    $('#statusDropdown').text($(this).text() + ' ');
    $('#statusDropdown').append('<i class="fas fa-chevron-down"></i>');
    $('#statusMenu').hide();

    // Update current status
    currentStatus = status;

    // Perform status filtering
    filterByStatus();
});

// Select all checkbox functionality
$('#table-select-all, #select-all').change(function () {
    var isChecked = $(this).prop('checked');
    $('.item-checkbox').prop('checked', isChecked);

    // Synchronize another Select All checkbox
    if (this.id === 'table-select-all') {
        $('#select-all').prop('checked', isChecked);
    } else {
        $('#table-select-all').prop('checked', isChecked);
    }
});

// Pagination button event
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

// Search box input event
$('#searchInput').on('keyup', function () {
    searchUsers();
});

// Refresh button click event
$('#refreshButton').click(function () {
    location.reload();
});

// Initialize the paging system
function initPagination() {
    // Calculate total items and pages
    IncidentItems = $('.incident-item').length;
    IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

    // Show first page content
    applyPagination();

    // Update paging information and button status
    updatePaginationInfo();
    updatePaginationButtons();
}

// Sort variable
var currentSortOrder = 'asc'; // Default ascending order

// Sort by Number click event
$('#sortByNumber').click(function () {
    // Toggle sort order
    currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';

    // Update sort icon
    if (currentSortOrder === 'asc') {
        $('#sortIcon').removeClass('fa-arrow-down').addClass('fa-arrow-up');
    } else {
        $('#sortIcon').removeClass('fa-arrow-up').addClass('fa-arrow-down');
    }

    // Call the sort API
    sortUsers();
});

// Sort todos function
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

// Check selection and toggle delete button
function checkSelection() {
    var hasChecked = $('.item-checkbox:checked').length > 0;
    $('#blockButton').prop('disabled', !hasChecked);
    $('#activeButton').prop('disabled', !hasChecked);
}

// Handle checkbox changes
$(document).on('change', '.item-checkbox', function () {
    checkSelection();
});

// Handle table-select-all checkbox
$(document).on('change', '#table-select-all, #select-all', function () {
    var isChecked = $(this).prop('checked');
    $('.item-checkbox').prop('checked', isChecked);

    // Sync the other select-all checkbox
    if (this.id === 'table-select-all') {
        $('#select-all').prop('checked', isChecked);
    } else {
        $('#table-select-all').prop('checked', isChecked);
    }

    // Check if delete button should be enabled
    checkSelection();
});

// Block button click event
$('#blockButton').click(function () {
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
        url: '/Ajax/BlockUsers',
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

// Active button click event
$('#activeButton').click(function () {
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
        url: '/Ajax/ActiveUsers',
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

// Apply paging Function
function applyPagination() {
    // Hide all items
    $('.incident-item').hide();

    // Calculate the start and end index of the current page
    var start = (currentPage - 1) * itemsPerPage;
    var end = start + itemsPerPage;

    // Show only items on the current page
    $('.incident-item').each(function (index) {
        if (index >= start && index < end) {
            $(this).show();
        }
    });
}

// Update paging information Function
function updatePaginationInfo() {
    // Calculates the range of items currently displayed
    var IncidentItems = $('.incident-item').length;

    if (IncidentItems === 0) {
        $('#paginationInfo').text('0 items');
        return;
    }

    // Calculate the range of items displayed on the current page
    var start = (currentPage - 1) * itemsPerPage + 1;
    var end = Math.min(currentPage * itemsPerPage, IncidentItems);

    // Total items
    $('#paginationInfo').text(start + ' to ' + end + ' of ' + IncidentItems);
}

// Reset pagination Function
function resetPagination() {
    // Reset to first page
    currentPage = 1;

    // Recalculate total items and pages
    IncidentItems = $('.incident-item').length;
    IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

    // Apply paging
    applyPagination();
    updatePaginationInfo();
    updatePaginationButtons();
}

// Update the paging button status
function updatePaginationButtons() {
    $('#firstPageBtn, #prevPageBtn').prop('disabled', currentPage === 1);
    $('#nextPageBtn, #lastPageBtn').prop('disabled', currentPage === IncidentPages || IncidentItems === 0);
}

// Search Todo List Function
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

            // Reset pagination
            resetPagination();
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Search Error:', error);
        }
    });
}

// Filter Select
function filterByStatus() {
    if (currentStatus === 'all') {
        // Filter All active
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

            // Reset pagination
            resetPagination();
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Filter Error:', error);
        }
    });
}

// Filter Active
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

    // Reset pagination
    resetPagination();
}

// Update Todo table function
function updateUserTable(data) {
    var tableBody = $('#incTableBody');
    tableBody.empty();

    if (data.length === 0) {
        tableBody.append('<tr><td colspan="11" class="text-center">No matching User found</td></tr>');
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
                        <td data-label="Email">${u.email}</td>
                        <td data-label="Gender">${u.gender}</td>
                        <td data-label="D_Name">${u.departmentName}</td>
                        <td data-label="Title">${u.title}</td>
                        <td data-label="Mobile_Phone">${u.mobile_phone}</td>
                        <td data-label="R_Role">${u.role}</td>
                        <td data-label="Race">${u.race}</td>
                        <td data-label="Active">${u.active}</td>
                    </tr>
                `;
        tableBody.append(row);
    });

    // Reinitialize paging
    initPagination();

    // Check for selections
    checkSelection();
}

// No login Function
function errorLogin(error) {
    const msg = error?.responseJSON?.message;

    if (msg === "Not logged in") {
        window.location.href = "/Auth/Login";
    }
}