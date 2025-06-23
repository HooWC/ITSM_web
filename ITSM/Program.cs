using ITSM.Controllers;
using ITSM_DomainModelEntity.Function;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400000;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(120);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
}).AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

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

// 添加 CORS 支持
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed((host) => true)
               .AllowCredentials();
    });
});

var app = builder.Build();

app.MapHub<NoteHub>("/noteHub");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseCors("SignalRPolicy");

app.UseSession(); // Enable Session Middleware

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
