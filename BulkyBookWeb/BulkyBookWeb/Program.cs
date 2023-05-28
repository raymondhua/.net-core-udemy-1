using BulkyBook.CloudStorage.Common;
using BulkyBook.CloudStorage.Repository;
using BulkyBook.CloudStorage.Services;
using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;


StaticLogger.EnsureInitialized();
Log.Information("BulkyBook loading...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"), options => options.MigrationsAssembly("BulkyBook.DataAccess")
        ));
    builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
        .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddSingleton<IEmailSender, EmailSender>();
    builder.Services.AddTransient<IAzureStorage, AzureStorage>();
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
    builder.Services.ConfigureApplicationCookie(options => 
    { 
        options.LoginPath = $"/Identity/Account/Login";
        options.LogoutPath = $"/Identity/Account/Logout";
        options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    }
    );
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
    app.UseAuthentication();

    app.UseAuthorization();
    app.MapRazorPages();

    app.MapControllerRoute(
        name: "default",
        pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

    app.Run();
    Log.Information("Application is loaded");
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled Exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Azure Storage API Shutting Down...");
    Log.CloseAndFlush();
}