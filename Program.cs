using Microsoft.EntityFrameworkCore;
using MusicBaseApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Port Setup
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Railway DATABASE_URL Parse
var rawDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(rawDbUrl))
{
    var uri = new Uri(rawDbUrl);
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = string.Join(":", userInfo.Skip(1).ToArray());
    var host = uri.Host;
    var database = uri.AbsolutePath.TrimStart('/');
    var portNumber = uri.Port > 0 ? uri.Port : 5432;

    connectionString = $"Host={host};Database={database};Username={username};Password={password};Port={portNumber};SSL Mode=Require;Trust Server Certificate=true;";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

// Ensure uploads directory exists on startup
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