using Esportify.Data;
using Esportify.Data.Initializers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure database based on environment
if (builder.Environment.IsProduction())
{
    // Use PostgreSQL in production
    builder.Services.AddDbContext<EsportifyContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions => 
        {
            npgsqlOptions.EnableRetryOnFailure();
            npgsqlOptions.CommandTimeout(30);
        });
        
        // Disable sensitive data logging in production
        options.EnableSensitiveDataLogging(false);
        options.EnableDetailedErrors(false);
    });
}
else
{
    // Use SQLite in development
    builder.Services.AddDbContext<EsportifyContext>(options =>
        options.UseSqlite(connectionString));
}
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Path = "/";
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var app = builder.Build();

// Initialize database only if not in production or if specific env var is set
if (!app.Environment.IsProduction() || builder.Configuration.GetValue<bool>("INITIALIZE_DATABASE", false))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = services.GetRequiredService<EsportifyContext>();
            logger.LogInformation("Starting database initialization...");
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Initialize with sample data
            await DbInitializer.InitializeAsync(context);
            
            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database: {Message}", ex.Message);
            
            // In production, don't crash the app if DB initialization fails
            if (!app.Environment.IsProduction())
            {
                throw;
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LandingPage}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "profile",
    pattern: "profile/{username}",
    defaults: new { controller = "Profile", action = "Index" });


app.Run();
