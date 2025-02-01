using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
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
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Parse("127.0.0.1"), 5000);
    serverOptions.Listen(IPAddress.Parse("127.0.0.1"), 5001, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// Disable IPv6
builder.WebHost.UseUrls("http://127.0.0.1:5000", "https://127.0.0.1:5001");

// Configure timezone
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("pl-PL");
    var supportedCultures = new[] { new System.Globalization.CultureInfo("pl-PL") };
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configure HTTPS
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 5001;
});

// Add custom middleware to handle HTTP to HTTPS redirection for all ports
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
});

// Configure HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    options.ExcludedHosts.Clear();
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
            .SetIsOriginAllowed(_ => true)
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
}

// Order is important here
app.UseForwardedHeaders();
app.UseHsts();

// Custom middleware to ensure all HTTP traffic is redirected to HTTPS
app.Use(async (context, next) =>
{
    if (!context.Request.IsHttps)
    {
        var host = context.Request.Host.Host;
        if (host.Contains("localhost") || host.Contains("::1"))
        {
            host = "127.0.0.1";
        }
        var httpsUrl = $"https://{host}:5001{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(httpsUrl, true);
        return;
    }
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add CORS before auth middleware but after HTTPS redirection
app.UseCors("SignalRPolicy");

app.UseRouting();
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
