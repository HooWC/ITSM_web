using ITSM.Controllers;
using ITSM_DomainModelEntity.Function;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Add Session service
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Setting a very long timeout (100 years) is actually equivalent to forever
    options.IdleTimeout = TimeSpan.FromDays(365 * 100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // Remove Expiration setting because it cannot be used at the same time as MaxAge
    // Set Cookie to persistent cookie (using MaxAge)
    options.Cookie.MaxAge = TimeSpan.FromDays(365 * 100);
});

builder.Services.AddScoped<UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Enable Session Middleware

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
