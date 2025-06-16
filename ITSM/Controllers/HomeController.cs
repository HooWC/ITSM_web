using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ITSM.Models;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_DomainModelEntity.Models;

namespace ITSM.Controllers;

public class HomeController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserService _userService;
    private readonly Auth_api _authApi;
    private readonly User_api _userApi;
    private readonly Todo_api _todoApi;
    private readonly Feedback_api _feedbackApi;
    private readonly Incident_api _incApi;
    private readonly Knowledge_api _knowledgeApi;
    private readonly Request_api _reqApi;
    private readonly Department_api _depApi;
    private readonly Role_api _roleApi;
    private readonly Category_api _categoryApi;
    private readonly Product_api _productApi;
    private readonly Department_api _departmentApi;
    private readonly Announcement_api _announApi;

    public HomeController(IHttpContextAccessor httpContextAccessor, UserService userService)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _authApi = new Auth_api(httpContextAccessor);
        _userApi = new User_api(httpContextAccessor);
        _todoApi = new Todo_api(httpContextAccessor);
        _feedbackApi = new Feedback_api(httpContextAccessor);
        _incApi = new Incident_api(httpContextAccessor);
        _knowledgeApi = new Knowledge_api(httpContextAccessor);
        _reqApi = new Request_api(httpContextAccessor);
        _depApi = new Department_api(httpContextAccessor);
        _roleApi = new Role_api(httpContextAccessor);
        _categoryApi = new Category_api(httpContextAccessor);
        _productApi = new Product_api(httpContextAccessor);
        _departmentApi = new Department_api(httpContextAccessor);
        _announApi = new Announcement_api(httpContextAccessor);
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        var noteMessageCount = await _userService.GetNoteAsync();

        var incTask = _incApi.GetAllIncident_API();
        var requestTask = _reqApi.GetAllRequest_API();
        var kbTask = _knowledgeApi.GetAllKnowledge_API();
        var fdTask = _feedbackApi.GetAllFeedback_API();
        var annTask = _announApi.GetAllAnnouncement_API();
        var todoTask = _todoApi.GetAllTodo_API();
        await Task.WhenAll(incTask, requestTask, kbTask, fdTask, annTask, todoTask);

        var allInc = incTask.Result;
        var allReq = requestTask.Result;
        var allKB = kbTask.Result;
        var allFD = fdTask.Result;
        var allAnn = annTask.Result;
        var allTodo = todoTask.Result;

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1); // Get tomorrow's date to calculate today's data

        // Calculate request statistics for this year
        var startOfYear = new DateTime(today.Year, 1, 1);
        var endOfYear = new DateTime(today.Year, 12, 31, 23, 59, 59);

        // Get all requests for this year
        var yearlyRequests = allReq.Where(x => x.create_date >= startOfYear && x.create_date <= endOfYear).ToList();
        var yearlyRequestsTotal = yearlyRequests.Count;

        // Count completed requests
        var completedRequests = yearlyRequests.Count(x => x.state == "Completed");
        var completedRequestsPercent = yearlyRequestsTotal > 0 
            ? Math.Round((double)completedRequests / yearlyRequestsTotal * 100, 2) 
            : 0;

        // Request to calculate other status
        var otherStateRequests = yearlyRequestsTotal - completedRequests;
        var otherStateRequestsPercent = yearlyRequestsTotal > 0 
            ? Math.Round((double)otherStateRequests / yearlyRequestsTotal * 100, 2) 
            : 0;

        // Calculate today's statistics
        var today_inc = allInc
            .Where(x => x.create_date >= today && x.create_date < tomorrow)
            .Count();

        var today_req = allReq
            .Where(x => x.create_date >= today && x.create_date < tomorrow)
            .Count();

        var today_kb = allKB
            .Where(x => x.create_date >= today && x.create_date < tomorrow)
            .Count();

        var today_fd = allFD
            .Where(x => x.create_date >= today && x.create_date < tomorrow)
            .Count();

        // Calculate historical average data (excluding today)
        var past_inc_total = allInc.Count(x => x.create_date < today);
        var past_req_total = allReq.Count(x => x.create_date < today);
        var past_kb_total = allKB.Count(x => x.create_date < today);
        var past_fd_total = allFD.Count(x => x.create_date < today);

        // Calculate percentage change
        double CalcTodayShare(double today_value, double history_total)
        {
            if (history_total == 0)
            {
                return today_value > 0 ? 100 : 0;
            }

            return (today_value / history_total) * 100;
        }

        var inc_percent = Math.Round(CalcTodayShare(today_inc, past_inc_total), 2);
        var req_percent = Math.Round(CalcTodayShare(today_req, past_req_total), 2);
        var kb_percent = Math.Round(CalcTodayShare(today_kb, past_kb_total), 2);
        var fd_percent = Math.Round(CalcTodayShare(today_fd, past_fd_total), 2);

        // Get the Monday of this week (Monday is the first day of the week)
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

        // Sunday this week
        var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1); // 23:59:59

        // Calculate weekly data
        var inc_c = allInc.Count(x => x.create_date >= startOfWeek && x.create_date <= endOfWeek && x.sender == currentUser.id);
        var req_c = allReq.Count(x => x.create_date >= startOfWeek && x.create_date <= endOfWeek && x.sender == currentUser.id);
        var kb_c = allKB.Count(x => x.create_date >= startOfWeek && x.create_date <= endOfWeek && x.author == currentUser.id);
        var fd_c = allFD.Count(x => x.create_date >= startOfWeek && x.create_date <= endOfWeek && x.user_id == currentUser.id);

        // Calculate todo data
        var todo_c = allTodo.Count(x =>
            x.create_date >= startOfYear &&
            x.create_date <= endOfYear &&
            x.user_id == currentUser.id);

        var todo_d = allTodo.Count(x =>
            x.create_date >= startOfYear &&
            x.create_date <= endOfYear &&
            x.active &&
            x.user_id == currentUser.id);

        var todo_percent = todo_c == 0 ? 0 : Math.Round((double)todo_d / todo_c * 100, 2);

        // Calculate monthly todo statistics
        var monthlyTodoStats = new List<MonthlyTodoStats>();
        var currentMonth = today.Month;
        var currentYear = today.Year;

        // Get the last 6 months (including current month)
        for (int i = 0; i < 6; i++)
        {
            var targetDate = today.AddMonths(-i);
            var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var activeCount = allTodo.Count(x => 
                x.create_date >= monthStart && 
                x.create_date <= monthEnd && 
                x.active &&
                x.user_id == currentUser.id);

            monthlyTodoStats.Add(new MonthlyTodoStats
            {
                MonthName = targetDate.ToString("MMM"),
                ActiveCount = activeCount
            });
        }

        // Reverse the list so it's in chronological order
        monthlyTodoStats.Reverse();

        // Calculate request statistics for each month of this year
        var monthlyRequestStats = new List<MonthlyRequestStats>();
        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(currentYear, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var requestCount = allReq.Count(x => 
                x.create_date >= monthStart && 
                x.create_date <= monthEnd);

            monthlyRequestStats.Add(new MonthlyRequestStats
            {
                MonthName = monthStart.ToString("MMM"),
                RequestCount = requestCount
            });
        }

        var yearlyStats = new Dictionary<int, Dictionary<int, (int resolvedCount, int otherCount)>>();

        // Initialize three years of data structure
        for (int year = currentYear - 2; year <= currentYear; year++)
        {
            yearlyStats[year] = new Dictionary<int, (int resolvedCount, int otherCount)>();
            for (int month = 1; month <= 12; month++)
            {
                yearlyStats[year][month] = (0, 0);
            }
        }

        foreach (var incident in allInc)
        {
            var year = incident.create_date.Year;
            var month = incident.create_date.Month;

            if (yearlyStats.ContainsKey(year))
            {
                var currentCounts = yearlyStats[year][month];
                if (incident.state == "Resolved")
                {
                    yearlyStats[year][month] = (currentCounts.resolvedCount + 1, currentCounts.otherCount);
                }
                else
                {
                    yearlyStats[year][month] = (currentCounts.resolvedCount, currentCounts.otherCount + 1);
                }
            }
        }

        var yearlyIncidentStats = new List<YearlyStats>();
        foreach (var yearData in yearlyStats)
        {
            var yearStats = new YearlyStats { Year = yearData.Key };
            int totalResolved = 0;
            int totalIncidents = 0;

            foreach (var monthData in yearData.Value)
            {
                yearStats.MonthlyData[monthData.Key] = new MonthlyStats
                {
                    Month = monthData.Key,
                    ResolvedCount = monthData.Value.resolvedCount,
                    OtherCount = monthData.Value.otherCount
                };

                // Cumulative resolved and total incidents
                totalResolved += monthData.Value.resolvedCount;
                totalIncidents += monthData.Value.resolvedCount + monthData.Value.otherCount;
            }

            // Calculate Growth (percentage of resolved incidents)
            yearStats.Growth = totalIncidents > 0 ? Math.Round((double)totalResolved / totalIncidents * 100, 1) : 0;

            yearlyIncidentStats.Add(yearStats);
        }

        var model = new AllModelVM()
        {
            user = currentUser,
            fd_count = fd_c,
            inc_count = inc_c,
            kb_count = kb_c,
            req_count = req_c,
            total_count = fd_c + inc_c + kb_c + req_c,
            AnnouncementList = allAnn.OrderByDescending(x => x.id).Take(6).ToList(),
            todo_count = todo_c,
            todo_d_count = todo_percent,
            YearlyIncidentStats = yearlyIncidentStats,
            MonthlyTodoStats = monthlyTodoStats,
            today_inc_count = today_inc,
            today_inc_percent = inc_percent,
            today_req_count = today_req,
            today_req_percent = req_percent,
            today_kb_count = today_kb,
            today_kb_percent = kb_percent,
            today_fd_count = today_fd,
            today_fd_percent = fd_percent,
            yearly_req_total = yearlyRequestsTotal,
            yearly_req_completed_count = completedRequests,
            yearly_req_completed_percent = completedRequestsPercent,
            yearly_req_other_count = otherStateRequests,
            yearly_req_other_percent = otherStateRequestsPercent,
            MonthlyRequestStats = monthlyRequestStats,
            noteMessageCount = noteMessageCount
        };

        if (currentUser.Role.role.ToLower() == "user")
            return View("Index_User", model);
        else
            return View(model);
    }

    public async Task<IActionResult> Index_User(AllModelVM model)
    {
        return View(model);
    }
}
