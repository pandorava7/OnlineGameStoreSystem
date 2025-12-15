using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddDbContext<MyDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDbContext<DB>(options =>
//    options.UseSqlServer($@"Server=(LocalDB)\MSSQLLocalDB;AttachDbFilename={builder.Environment.ContentRootPath}\DB.mdf;Database=OnlineGameStoreDB;Trusted_Connection=True;MultipleActiveResultSets=true")
//);
builder.Services.AddSqlServer<DB>($@"
    Data Source=(LocalDB)\MSSQLLocalDB;
    AttachDbFilename={builder.Environment.ContentRootPath}\DB.mdf;
");

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
});


// OTP
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Recommendation services
builder.Services.AddScoped<RecommendationService>();

// Invoice services
builder.Services.AddScoped<InvoiceService>();

// ?? HttpContextAccessor
builder.Services.AddHttpContextAccessor();
// ?? SecurityHelper
builder.Services.AddScoped<SecurityHelper>();
// Authentication - 必须加
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";   // 未登录自动跳转
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);       
        options.SlidingExpiration = true; // 让 Cookie 在用户“继续活动时自动延长时间”
    });

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();
// inside


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


