using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SIISMinimalAPI.Data;
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
using SIISMinimalAPI.Features.OfficeAccounts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSqlite<AppDbContext>("Data Source=siisdemo.db");
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
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
    .AddPolicy("User", policy => policy.RequireRole("User"));

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
builder.Services.AddScoped<IOfficeAccountService, OfficeAccountHandler>();
builder.Services.AddScoped<IOjtService, OjtHandler>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRegistrationTokenService, RegistrationTokenHandler>();
builder.Services.AddScoped<IOjtListService, OjtListHandler>();
builder.Services.AddScoped<IOjtPerOfficeService, OjtPerOfficehandler>();
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
app.UseRateLimiter();
app.MapOnBoardingEnpoints();
app.MapToApplication();
app.MapToEndorsement();
app.MapToOffice();
app.MapToOfficeAccount();
app.MapToOjt();
app.MapToAuth();
app.MapToUser();
app.MapToRegistrationEndpoint();
app.MapToOjtList();
app.MapToOjtPerOffice();

app.MapIdentityApi<IdentityUser>().RequireCors("AllowFrontend");


//seed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    await MigrateSchemaAsync(dbContext);
    await SeederAdmin.InitAdmin(scope.ServiceProvider);
    await SeederAdmin.InitOffices(scope.ServiceProvider);
    await SeederStudent.InitStudents(scope.ServiceProvider);
}

static async Task MigrateSchemaAsync(AppDbContext dbContext)
{
    try
    {
        await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Offices ADD COLUMN Department TEXT NULL");
    }
    catch (Microsoft.Data.Sqlite.SqliteException) { }

    try
    {
        await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Internship ADD COLUMN AccumulatedHours INTEGER NOT NULL DEFAULT 0");
    }
    catch (Microsoft.Data.Sqlite.SqliteException) { }

    var tableExists = await dbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM sqlite_master WHERE type='table' AND name='OfficeAccounts'").FirstOrDefaultAsync();
    if (tableExists == 0)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE OfficeAccounts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OfficeId INTEGER NOT NULL,
                Username TEXT NOT NULL,
                Email TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                CreateAt TEXT NOT NULL DEFAULT (datetime('now')),
                UpdatedAt TEXT NULL,
                DeletedAt TEXT NULL
            )
        """);
        await dbContext.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IX_OfficeAccounts_Email ON OfficeAccounts(Email)");
        await dbContext.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IX_OfficeAccounts_Username ON OfficeAccounts(Username)");
    }
}

app.MapFallbackToFile("index.html");
app.Run();
