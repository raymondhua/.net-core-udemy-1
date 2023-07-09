using BulkyBook.DataAccess;
using BulkyBook.DataAccess.DbInitializer;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Serilog;
using BulkyBook.CloudStorage.Common;
using BulkyBook.CloudStorage.Repository;
using BulkyBook.CloudStorage.Service;
using Microsoft.Extensions.Hosting;


try
{
    var builder = WebApplication.CreateBuilder(args);
    StaticLogger.EnsureInitialized();
    Log.Information("BulkyBookWeb loading...");

    builder.Host.UseSerilog((_, config) =>
    {
        config.WriteTo.Console()
            .WriteTo.Seq(serverUrl: "http://seq:5341")
            .ReadFrom.Configuration(builder.Configuration);
    });

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddHttpContextAccessor();
    builder.Configuration.AddUserSecrets<Program>();
    string environmentName = builder.Environment.EnvironmentName;
    Log.Information("Environment: " + environmentName);
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
        builder.Configuration.GetConnectionString(environmentName),
        options => options.MigrationsAssembly("BulkyBook.DataAccess")
    ));
    builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
    builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
        .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IDbInitializer, DbInitializer>();
    builder.Services.AddSingleton<IEmailSender, EmailSender>();
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
    builder.Services.AddAuthentication().AddFacebook(options =>
    {
        options.AppId = builder.Configuration.GetValue<string>("FacebookSettings:AppId");
        options.AppSecret = builder.Configuration.GetValue<string>("FacebookSettings:AppSecret");
    });
    builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        }
    );
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(100);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Add Azure Repository Service
    builder.Services.AddTransient<IAzureStorage, AzureStorage>();
    Log.Information("Services has been successfully added...");

    var app = builder.Build();

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

    StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
    SeedDatabase();
    app.UseAuthentication();

    app.UseAuthorization();
    app.UseSession();
    app.MapRazorPages();

    app.MapControllerRoute(
        name: "default",
        pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

    app.Run();

    void SeedDatabase()
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            dbInitializer.Initialize();
        }
    }
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled Exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("BulkyBookWeb Shutting Down...");
    Log.CloseAndFlush();
}