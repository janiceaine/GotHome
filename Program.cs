using GotHome.Hubs;
using GotHome.Models;
using GotHome.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

// --------------------
// Add services
// --------------------
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); // SignalR for real-time chat
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// Scoped services
builder.Services.AddScoped<IPasswordService, BcryptPasswordService>();
builder.Services.AddScoped<IImageService, ImageKitService>();

// Database context
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true); // Allow any origin
    });
});

// --------------------
// Configure URLs
// --------------------
builder.WebHost.UseUrls("http://0.0.0.0:5228", "https://0.0.0.0:7031");

var app = builder.Build();

// --------------------
// Middleware pipeline
// --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
    app.UseStatusCodePagesWithReExecute("/error/{0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Must come before MapHub
app.UseCors();

app.UseAuthorization();
app.UseSession();

// --------------------
// Routing
// --------------------
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// SignalR hub
app.MapHub<EventChatHub>("/eventChatHub");

app.Run();
