using Microsoft.EntityFrameworkCore;
using MusicBaseApp.Data;
using Supabase;  // 🔥🔥🔥 Supabase Namespace Add करें

var builder = WebApplication.CreateBuilder(args);

// 1. Port Setup (Railway के लिए)
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 2. SQL Server Connection String (Local या Somee)
var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. 🔥🔥🔥 Supabase Client Register करें
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:AnonKey"];
builder.Services.AddScoped(_ => new Supabase.Client(supabaseUrl, supabaseKey));

// 4. Other Services
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Database Migrate (Tables अपने-आप बनेंगी)
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

// 6. Uploads Folder Ensure (अब यह जरूरी नहीं, लेकिन रख सकते हैं)
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