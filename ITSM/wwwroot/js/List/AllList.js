var currentPage = 1;
var itemsPerPage = 15;
var IncidentItems = $('.incident-item').length;
var IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

phone_function()

let searchFunctionName = $('#forAjaxGetFunctionName_search').text();
let sortFunctionName = $('#forAjaxGetFunctionName_sort').text();
let filterFunctionName = $('#forAjaxGetFunctionName_filter').text();

var currentFilter = 'number';
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
        searchIncidents();
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
    searchIncidents();
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

    sortIncidents();
});

function sortIncidents() {
    var word = "";
    if (sortFunctionName.includes("Resolved_Assigned_SortIncident"))
        word = "resolve";
    else if (sortFunctionName.includes("Closed_Assigned_SortIncident"))
        word = "closed";
    else if (sortFunctionName.includes("Assigned_to_me_Assigned_SortIncident"))
        word = "tome";
    else if (sortFunctionName.includes("Assigned_to_team_Assigned_SortIncident"))
        word = "toteam";
    else if (sortFunctionName.includes("SortIncident_user"))
        word = "user";
    else if (sortFunctionName.includes("SortAssignWork"))
        word = "assign_work";
    else
        word = "sort_basic";

    $.ajax({
        url: window.AppRoot + 'Ajax/SortIncident',
        method: 'GET',
        data: {
            sortOrder: currentSortOrder,
            sortWord: word
        },
        success: function (data) {
            updateIncidentTable(data);

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
        // console.log("Delete confirmed");
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
        url: window.AppRoot + 'Ajax/DeleteIncidents',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (response) {
            if (response.success) {
                $('.item-checkbox:checked').closest('tr').remove();

                $('#select-all, #table-select-all').prop('checked', false);

                $('#deleteButton').prop('disabled', true);

                resetPagination();
            }
        },
        error: function (error) {
            errorLogin(error);
            // console.error('Delete Error:', error);
        }
    });
}

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

function searchIncidents() {
    var searchTerm = $('#searchInput').val().trim();
    if (searchTerm === '') {
        searchTerm = "re_entrynovalue";
    }

    var word = "";
    if (searchFunctionName.includes("Resolved_Assigned_SearchIncident"))
        word = "resolve";
    else if (searchFunctionName.includes("Closed_Assigned_SearchIncident"))
        word = "closed";
    else if (searchFunctionName.includes("Assigned_to_me_Assigned_SearchIncident"))
        word = "tome";
    else if (searchFunctionName.includes("Assigned_to_team_Assigned_SearchIncident"))
        word = "toteam";
    else if (searchFunctionName.includes("SearchIncident_user"))
        word = "user";
    else if (searchFunctionName.includes("SearchAssignWork"))
        word = "assign_work";
    else
        word = "search_basic";

    $.ajax({
        url: window.AppRoot + 'Ajax/SearchIncident',
        method: 'GET',
        data: {
            searchTerm: searchTerm,
            filterBy: currentFilter,
            searchWord: word
        },
        success: function (data) {
            updateIncidentTable(data);
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
            searchIncidents();
        } else {
            location.reload();
        }
        return;
    }

    var word = "";
    if (filterFunctionName.includes("Filter_Item_Admin"))
        word = "admin";
    else if (filterFunctionName.includes("Assigned_to_me_Assigned_FilterIncident"))
        word = "tome"
    else if (filterFunctionName.includes("Assigned_to_team_Assigned_FilterIncident"))
        word = "toteam"
    else if (filterFunctionName.includes("Closed_Assigned_FilterIncident"))
        word = "closed"
    else if (filterFunctionName.includes("FilterIncident_user"))
        word = "user";
    else
        word = "filter_basic";

    $.ajax({
        url: window.AppRoot + 'Ajax/FilterIncidentByStatus',
        method: 'GET',
        data: {
            status: currentStatus,
            filterword: word
        },
        success: function (data) {
            updateIncidentTable(data);

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

function formatDate(dateString) {
    if (!dateString) return '-';
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
}

function updateIncidentTable(data) {
    var tableBody = $('#incTableBody');
    tableBody.empty();

    if (data.length === 0) {
        tableBody.append('<tr><td colspan="10" class="text-center">No matching Incidents found</td></tr>');
        IncidentItems = 0;
        IncidentPages = 0;
        updatePaginationInfo();
        updatePaginationButtons();
        return;
    }

    var typeName = $('#typeName').val();

    $.each(data, function (index, inc) {
        var row = `
                    <tr class="incident-item" data-id="${inc.id}">
                        <td class="inc-tab-incident-number" data-label="Number">
                            <a href="/IncidentManagement/Inc_Info_Form?id=${inc.id}&&type=${typeName}">${inc.inc_number}</a>
                        </td>
                        <td data-label="urgency">${inc.urgency}</td>
                        <td data-label="state" style="color: ${
                            inc.state === 'Closed' ? 'darkred' :
                            inc.state === 'Resolved' ? 'chocolate' :
                            inc.state === 'In Progress' ? 'dodgerblue' : '#C4C6C7'
                            }">${inc.state}</td>
                        <td data-label="category" class="phone_incident_hide_design">${inc.category}</td>
                        <td data-label="subcategory" class="phone_incident_hide_design">${inc.subcategory}</td>
                        <td data-label="assignment group" class="phone_incident_hide_design">${inc.assignment_group}</td>
                        <td data-label="create date" class="phone_incident_hide_design">${inc.create_date}</td>
                        <td data-label="update date" class="phone_incident_hide_design">${inc.update_date}</td>
                    </tr>
                `;
        tableBody.append(row);
        phone_function();
    });

    initPagination();

    checkSelection();
}

// No login Function
function errorLogin(error) {
    const msg = error?.responseJSON?.message;

    if (msg === "Not logged in") {
        window.location.href = "/Auth/Login";
    }
}

function phone_function() {
    // Phone Design
    if (/Mobi|Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        $(".phone_incident_hide_design").hide();
        $('.inc-tab-incidents-table th.number-column').css('width', '28%');
        $(".inc-tab-incidents-table th.i_urgency-column").css('width', '45%');
        $(".inc-tab-incidents-table th.i_state-column").css('width', '27%');
        $(".all-title-header-front").css('font-size', '1rem');
    }
}
