using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 添加HttpContext访问器
builder.Services.AddHttpContextAccessor();

// 添加Session服务
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // 设置非常长的超时时间（100年）实际上相当于永久
    options.IdleTimeout = TimeSpan.FromDays(365 * 100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // 移除Expiration设置，因为它与MaxAge不能同时使用
    // 设置Cookie为持久性Cookie（使用MaxAge）
    options.Cookie.MaxAge = TimeSpan.FromDays(365 * 100);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // 启用Session中间件

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
