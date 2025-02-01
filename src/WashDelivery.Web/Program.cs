using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WashDelivery.Infrastructure;
using WashDelivery.Infrastructure.Data;
using WashDelivery.Domain.Entities;
using WashDelivery.Infrastructure.Hubs;
using WashDelivery.Application.Services;
using WashDelivery.Application.Interfaces;
using WashDelivery.Infrastructure.Services;
using WashDelivery.Web.Middleware;
using Microsoft.Extensions.FileProviders;
using AutoMapper;
using WashDelivery.Application.Mapping;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);  // HTTP
    serverOptions.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// Configure timezone
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("pl-PL");
    var supportedCultures = new[] { new System.Globalization.CultureInfo("pl-PL") };
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to handle UTC dates
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonDateTimeConverter());
    });

// Configure antiforgery
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:5000", "https://localhost:5001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure security stamp validation interval
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Validate security stamp every 1 second to ensure quick invalidation
    options.ValidationInterval = TimeSpan.FromSeconds(1);
    options.OnRefreshingPrincipal = async context => 
    {
        // Force re-validation of the security stamp
        var newPrincipal = context.CurrentPrincipal;
        if (newPrincipal?.Identity is ClaimsIdentity identity)
        {
            // Clear the current principal to force re-validation
            context.CurrentPrincipal = null;
        }
    };
});

// Configure cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.Name = ".WashDelivery.Auth";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
});

// Add background services and notifications
builder.Services.AddHostedService<OrderAssignmentBackgroundService>();
builder.Services.AddScoped<ICourierNotificationService, CourierNotificationService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(OrderMappingProfile).Assembly);

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var laundryRepository = services.GetRequiredService<IRepository<Laundry>>();
    
    await DbInitializer.SeedTestData(userManager, roleManager, laundryRepository);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add CORS before auth middleware
app.UseCors("SignalRPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Ensure middleware is registered after auth but before endpoints
app.UseMiddleware<LaundryAuthorizationMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
        
    endpoints.MapHub<LaundryOrderHub>("/hubs/laundryOrder");
    endpoints.MapHub<CourierOrderHub>("/hubs/courier");
});

app.Run();
