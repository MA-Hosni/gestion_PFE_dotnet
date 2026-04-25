using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PfeManagement.Application.EventHandlers;
using PfeManagement.Application.Factories;
using PfeManagement.Application.Interfaces;
using PfeManagement.Application.Services;
using PfeManagement.Application.Strategies;
using PfeManagement.Domain.Events;
using PfeManagement.Domain.Interfaces;
using PfeManagement.Infrastructure.Data;
using PfeManagement.Infrastructure.Repositories;
using PfeManagement.Infrastructure.Services;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ───────────────────────────────────────────────
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

// ── DbContext ─────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── Singleton — AppConfigurationManager ──────────────────────
// Lit appsettings.json une seule fois au démarrage du serveur.
// JwtTokenService et EmailNotificationAdapter utilisent Instance.
AppConfigurationManager.Initialize(builder.Configuration);

// ── Infrastructure — Domain & Infrastructure ──────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<INotificationService, EmailNotificationAdapter>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ── SRP — 5 services Auth (remplace l'ancien IAuthService) ───
// AVANT : builder.Services.AddScoped<IAuthService, AuthService>();
// APRÈS : une interface et un service par responsabilité
builder.Services.AddScoped<IUserRegistrationService,  UserRegistrationService>();
builder.Services.AddScoped<ITokenManagementService,   TokenManagementService>();
builder.Services.AddScoped<IUserLoginService,         UserLoginService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IPasswordResetService,     PasswordResetService>();

// ── Application Services ──────────────────────────────────────
builder.Services.AddScoped<ITaskService,       TaskService>();
builder.Services.AddScoped<IProjectService,    ProjectService>();
builder.Services.AddScoped<ISprintService,     SprintService>();
builder.Services.AddScoped<IUserStoryService,  UserStoryService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IMeetingService,    MeetingService>();
builder.Services.AddScoped<IReportService,     ReportService>();

// ── Factory Method (GoF) ──────────────────────────────────────
builder.Services.AddScoped<StudentFactory>();
builder.Services.AddScoped<CompanySupervisorFactory>();
builder.Services.AddScoped<UniversitySupervisorFactory>();
builder.Services.AddScoped<UserFactory>(provider =>
    provider.GetRequiredService<StudentFactory>());

// ── Observer Pattern (GoF) ────────────────────────────────────
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IDomainEventHandler<TaskStatusChangedEvent>,
    TaskHistoryHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskStatusChangedEvent>,
    SupervisorNotificationHandler>();

// ── Strategy Pattern (GoF) ────────────────────────────────────
builder.Services.AddScoped<IValidationStrategy, ReunionValidationStrategy>();
builder.Services.AddScoped<IValidationStrategy, HorsReunionValidationStrategy>();

// ── FluentValidation ──────────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(
    typeof(PfeManagement.Application.Validators.SprintValidator).Assembly);

// ── JWT Authentication — utilise le Singleton ─────────────────
// Plus besoin de builder.Configuration.GetSection("JwtSettings")
// La clé vient directement d'AppConfigurationManager.Instance
var appConfig = AppConfigurationManager.Instance;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(appConfig.JwtSecret)),
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
});

// ── CORS ──────────────────────────────────────────────────────
var corsOrigins = builder.Configuration["CorsOrigins"]
                 ?.Split(',') ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ── Rate Limiting ─────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(
    builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ─────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

// app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowFrontend");
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();