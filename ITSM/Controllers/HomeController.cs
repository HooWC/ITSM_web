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

        // Get the Monday of this week (Monday is the first day of the week)
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

        // Sunday this week
        var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1); // 23:59:59

        var fd_c = allFD
                   .Where(x => x.user_id == currentUser.id && x.create_date >= startOfWeek && x.create_date <= endOfWeek)
                   .Count();

        var inc_c = allInc
                   .Where(x => x.sender == currentUser.id && x.create_date >= startOfWeek && x.create_date <= endOfWeek)
                   .Count();

        var req_c = allReq
                   .Where(x => x.sender == currentUser.id && x.create_date >= startOfWeek && x.create_date <= endOfWeek)
                   .Count();

        var kb_c = allKB
                   .Where(x => x.author == currentUser.id && x.create_date >= startOfWeek && x.create_date <= endOfWeek)
                   .Count();

        var thisYear = DateTime.Now.Year;

        var year_todo = allTodo.Where(x => x.create_date.Year == thisYear).ToList();

        var todo_c = year_todo.Where(x => x.user_id == currentUser.id).Count();

        var todo_d_c = year_todo.Where(x => x.user_id == currentUser.id && x.active == true).Count();

        double todo_percent = todo_c == 0 ? 0 : (double)todo_d_c / todo_c * 100;

        // Get event statistics for the current year and the previous two years
        var currentYear = DateTime.Now.Year;
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
            YearlyIncidentStats = yearlyIncidentStats
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
