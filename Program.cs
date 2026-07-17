using Microsoft.EntityFrameworkCore;
using MusicBaseApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Port Setup (Railway के लिए)
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔥 सीधा appsettings.json से Connection String पढ़ें
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database Migrate (Tables अपने-आप बनेंगी)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// Uploads Folder Ensure
var uploadsPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Upload}/{id?}");

app.MapControllers();

app.Run();