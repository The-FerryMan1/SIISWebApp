using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.Application;
using SIISMinimalAPI.Features.Auth.Logout;
using SIISMinimalAPI.Features.Auth.User;
using SIISMinimalAPI.Features.Endorsement;
using SIISMinimalAPI.Features.Offices;
using SIISMinimalAPI.Features.Ojt;
using SIISMinimalAPI.Features.OnBoarding;
using SIISMinimalAPI.Features.RegistrationToken;
using SIISMinimalAPI.Features.Report.OjtList;
using SIISMinimalAPI.Features.Report.OjtPerOffice;
using SIISMinimalAPI.Features.Report.OfficeReport;
using SIISMinimalAPI.Features.OfficeDashboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSqlite<AppDbContext>("Data Source=siisdemo.db");
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        var isApiRequest = context.Request.Path.StartsWithSegments("/api") ||
                           context.Request.Path.StartsWithSegments("/user") ||
                            context.Request.Path.StartsWithSegments("/register") ||
                           context.Request.Path.StartsWithSegments("/auth") ||
                           context.Request.Path.StartsWithSegments("/application") ||
                           context.Request.Path.StartsWithSegments("/onboading") ||
                           context.Request.Path.StartsWithSegments("/office") ||
                           context.Request.Path.StartsWithSegments("/endorsement") ||
                           context.Request.Path.StartsWithSegments("/ojt") ||
                           context.Request.Headers.Accept.Any(header => header.Contains("application/json", StringComparison.OrdinalIgnoreCase));

        if (isApiRequest)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("OPG", policy => policy.RequireRole("OPG"))
    .AddPolicy("Officer", policy => policy.RequireRole("Officer"));

builder.Services.AddControllers();

    
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
                policy =>
                {
                    policy
                        .WithOrigins("http://localhost:8080", "http://localhost:5173", "http://100.10.1.201:80", "https://588rl34b-8080.asse.devtunnels.ms")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // only if using cookies/auth
                });
});

builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("standard", opt =>
    {
        opt.PermitLimit = 100;           // 100 requests
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.Window = TimeSpan.FromMinutes(1);  // per 1 minute
        opt.QueueLimit = 10;             // queue 10 extra requests
    });
});

builder.Services.AddScoped<IOnBoadringService, OnBoardingHandler>();
builder.Services.AddScoped<IApplicationService, ApplicationHandler>();
builder.Services.AddScoped<IEndorsementService, EndorsementHandler>();
builder.Services.AddScoped<IOfficeService, OfficeHandler>();
builder.Services.AddScoped<IOjtService, OjtHandler>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRegistrationTokenService, RegistrationTokenHandler>();
builder.Services.AddScoped<IOjtListService, OjtListHandler>();
builder.Services.AddScoped<IOjtPerOfficeService, OjtPerOfficehandler>();
builder.Services.AddScoped<IOfficeDashboardService, OfficeDashboardHandler>();
builder.Services.AddScoped<IOfficeReportService, OfficeReportHandler>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", async (SignInManager<User> signInManager, UserManager<User> userManager, HttpContext context, LoginRequest request) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user == null)
    {
        return Results.Unauthorized();
    }

    var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: true, lockoutOnFailure: false);
    if (!result.Succeeded)
    {
        return Results.Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new
    {
        userId = user.Id,
        email = user.Email,
        roles = roles.ToArray()
    });
})
.RequireCors("AllowFrontend");

app.MapPost("/register", async (UserManager<User> userManager, RegisterRequest request) =>
{
    var user = new User
    {
        Email = request.Email,
        UserName = request.Email,
        EmailConfirmed = true
    };
    var result = await userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors);
    }
    return Results.Ok();
})
.RequireCors("AllowFrontend");

app.UseRateLimiter();
app.MapOnBoardingEnpoints();
app.MapToApplication();
app.MapToEndorsement();
app.MapToOffice();
app.MapToOjt();
app.MapToAuth();
app.MapToUser();
app.MapToRegistrationEndpoint();
app.MapToOjtList();
app.MapToOjtPerOffice();
app.MapToOfficeDashboard();
app.MapToOfficeReport();

//seed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    await SeederAdmin.InitAdmin(scope.ServiceProvider);
    await SeederStudent.InitStudents(scope.ServiceProvider);
}

app.MapFallbackToFile("index.html");
app.Run();

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password);