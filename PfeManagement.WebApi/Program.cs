using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
// Removed Microsoft.OpenApi.Models OpenApi references
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

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
// Swagger temporarily removed due to package issue

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Dependency Injection: Domain & Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<INotificationService, EmailNotificationAdapter>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Dependency Injection: Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ISprintService, SprintService>();
builder.Services.AddScoped<IUserStoryService, UserStoryService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ITaskReportService, TaskReportService>();
builder.Services.AddScoped<ITaskReportDataAccess>(sp => sp.GetRequiredService<IUnitOfWork>());

// Dependency Injection: Factory Method
builder.Services.AddScoped<StudentFactory>();
builder.Services.AddScoped<CompanySupervisorFactory>();
builder.Services.AddScoped<UniversitySupervisorFactory>();
builder.Services.AddScoped<UserFactory>(provider => provider.GetRequiredService<StudentFactory>()); // Sample default

// Dependency Injection: Observer Pattern (Event Dispatcher & Handlers)
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IDomainEventHandler<TaskStatusChangedEvent>, TaskHistoryHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskStatusChangedEvent>, SupervisorNotificationHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskCreatedEvent>, TaskCreatedHistoryHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskCreatedEvent>, TaskCreatedNotificationHandler>();
// A simple dispatcher implementation would go here, we'll implement it manually or map directly.

// Dependency Injection: Strategy Pattern
builder.Services.AddScoped<IValidationStrategy, ReunionValidationStrategy>();
builder.Services.AddScoped<IValidationStrategy, HorsReunionValidationStrategy>();

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(PfeManagement.Application.Validators.SprintValidator).Assembly);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
var corsOrigins = builder.Configuration["CorsOrigins"]?.Split(',') ?? Array.Empty<string>();
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

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
//    app.UseSwagger();
// app.UseSwaggerUI();
}

// Exception Middleware would be registered here
// app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowFrontend");

app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
