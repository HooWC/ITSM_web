var currentPage = 1;
var itemsPerPage = 15;
var IncidentItems = $('.incident-item').length;
var IncidentPages = Math.ceil(IncidentItems / itemsPerPage);

phone_function()

var currentFilter = 'number';
var currentStatus = 'all';

let searchFunctionName = $('#for_search').text();

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
        searchAnnouncements();
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
    searchAnnouncements();
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

    $.ajax({
        url: window.AppRoot + 'Ajax/SortAnnouncement',
        method: 'GET',
        data: {
            sortOrder: currentSortOrder
        },
        success: function (data) {
            updateAnnouncementTable(data);

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
        url: window.AppRoot + 'Ajax/DeleteAnnouncements',
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

function searchAnnouncements() {
    var searchTerm = $('#searchInput').val().trim();
    if (searchTerm === '') {
        searchTerm = "re_entrynovalue";
    }

    $.ajax({
        url: window.AppRoot + 'Ajax/SearchAnnouncement',
        method: 'GET',
        data: {
            searchTerm: searchTerm,
            filterBy: currentFilter
        },
        success: function (data) {
            updateAnnouncementTable(data);
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
            searchAnnouncements();
        } else {
            location.reload();
        }
        return;
    }

    var word = "";
    if (filterFunctionName.includes("Assigned_to_me_Assigned_FilterIncident"))
        word = "tome";
    else if (filterFunctionName.includes("Assigned_to_team_Assigned_FilterIncident"))
        word = "toteam";
    else
        word = "filter_basic";

    $.ajax({
        url: window.AppRoot + 'Ajax/FilterFeedbackByStatus',
        method: 'GET',
        data: {
            status: currentStatus,
            filterword: word
        },
        success: function (data) {
            updateAnnouncementTable(data);

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

function updateAnnouncementTable(data) {
    var tableBody = $('#incTableBody');
    tableBody.empty();

    if (data.length === 0) {
        tableBody.append('<tr><td colspan="6" class="text-center">No matching Announcement found</td></tr>');
        IncidentItems = 0;
        IncidentPages = 0;
        updatePaginationInfo();
        updatePaginationButtons();
        return;
    }

    var role_name = "";
    if (searchFunctionName.includes("_admin"))
        role_name = "/Announcement/Ann_Info";
    else
        role_name = "/Announcement/View_Ann_Info";

    $.each(data, function (index, ann) {
        var row = `
                    <tr class="incident-item" data-id="${ann.id}">
                        <td class="inc-tab-incident-number" data-label="Number">
                            <a href="${role_name}?id=${ann.id}">${ann.at_number}</a>
                        </td>
                        <td data-label="title">${ann.ann_title}</td>
                        <td data-label="u_fullname">${ann.fullname}</td>
                        <td data-label="create date" class="phone_announcement_hide_design">${ann.create_date}</td>
                        <td data-label="update date" class="phone_announcement_hide_design">${ann.update_date}</td>
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
        $(".phone_announcement_hide_design").hide();
        $('.inc-tab-incidents-table th.number-column').css('width', '28%');
        $(".inc-tab-incidents-table th.title").css('width', '42%');
        $(".inc-tab-incidents-table th.create-by").css('width', '30%');
        $(".all-title-header-front").css('font-size', '1rem');
    }
}
