using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using RESTfulAPI.Application.Behaviors;
using RESTfulAPI.Application.Filters;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Infrastructure.Auth;
using RESTfulAPI.Infrastructure.HostedServices;
using RESTfulAPI.Infrastructure.Repositories;
using RESTfulAPI.Infrastructure.Repositories.Interfaces;
using RESTfulAPI.Persistence;
using RESTfulAPI.Presentation.Common;
using RESTfulAPI.Presentation.Conventions;
using RESTfulAPI.Presentation.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────────────────────
// 1) Configure Serilog as our logging provider, reading settings from appsettings.*
// ──────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)   // pull Serilog settings from configuration
    .Enrich.FromLogContext()                         // ensure correlation IDs flow through logs
    .CreateLogger();

builder.Host.UseSerilog();   // replace the default Microsoft logger with Serilog

// ──────────────────────────────────────────────────────────────────────────────
// 2) Read connection string from configuration ("ConnectionStrings": { "Sql": ... })
// ──────────────────────────────────────────────────────────────────────────────
var sql = builder.Environment.IsDevelopment()
            ? builder.Configuration.GetConnectionString("Sql")
            : builder.Configuration.GetConnectionString("AzureSql");

// ──────────────────────────────────────────────────────────────────────────────
// 3) Register EF Core contexts for application data and logs, each with its own
//    migrations table and schema.
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(sql, b =>
    {
        b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
         .MigrationsHistoryTable("__EFMigrationsHistory_App", "app")
         .EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    }));

builder.Services.AddDbContext<LogDbContext>(o =>
o.UseSqlServer(sql, b =>
{
    b.MigrationsAssembly(typeof(LogDbContext).Assembly.FullName)
     .MigrationsHistoryTable("__EFMigrationsHistory_Log", "log")
     .EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
}));

// Wake up prod database upon app launch.
if (!builder.Environment.IsDevelopment())
    builder.Services.AddHostedService<DbWarmupService>();

// ──────────────────────────────────────────────────────────────────────────────
// 4) Configure CORS policy for our SPA frontend on localhost
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Spa", p => p
        .WithOrigins("http://localhost:5000", "https://localhost:5001")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ──────────────────────────────────────────────────────────────────────────────
// 5) Authentication & Authorization
//    - In Development: use a dummy "Dev" scheme that auto-succeeds
//    - In Production: use Azure AD JWT Bearer tokens via Microsoft.Identity.Web
// ──────────────────────────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    // Dummy auth: always pass
    builder.Services
        .AddAuthentication("Dev")
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });

    // Every request is treated as authenticated (so can still decorate with [Authorize])
    builder.Services.AddAuthorization(opt =>
    {
        opt.FallbackPolicy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .Build();
    });
}
else
{
    // Azure AD JWT Bearer
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    // Customize responses for 401/403
    builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                var payload = ApiResponse.Unauthorized("Access token is missing or invalid.");
                ctx.HandleResponse();
                ctx.Response.StatusCode = payload.Status;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsJsonAsync(payload);
            },
            OnForbidden = ctx =>
            {
                var payload = ApiResponse.Forbidden("You do not have permission.");
                ctx.Response.StatusCode = payload.Status;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsJsonAsync(payload);
            }
        };
    });

    builder.Services.AddAuthorization();
}

// ──────────────────────────────────────────────────────────────────────────────
// 6) Configure MVC behavior & Swagger
//    - Suppress default model-state filter and return our ApiResponse envelope on 422
//    - Globally prefix routes with "api"
//    - Slugify route tokens
//    - Add OAuth2 security to Swagger if scopes are configured
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.Configure<ApiBehaviorOptions>(opts =>
{
    opts.SuppressModelStateInvalidFilter = true;
    opts.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
                        .SelectMany(kvp => kvp.Value!.Errors
                                             .Select(e => new ApiError("Validation", e.ErrorMessage, kvp.Key)))
                        .ToList();
        var envelope = ApiResponse.ValidationFailed(errors);
        return new ObjectResult(envelope) { StatusCode = envelope.Status };
    };
});

builder.Services.AddControllers(opts =>
{
    // Automatically prefix every controller route with "api"
    opts.Conventions.Insert(0, new RoutePrefixConvention("api"));
    // Transform action parameter names to lowercase ("slugify")
    opts.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    var scope = builder.Configuration["AzureAd:Scopes"];
    var tenant = builder.Configuration["AzureAd:TenantId"];

    opt.SwaggerDoc("v1", new() { Title = "Gordon's API", Version = "v1" });

    if (!string.IsNullOrWhiteSpace(scope))
    {
        // Add OAuth2 definition for Swagger UI
        opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new()
                {
                    AuthorizationUrl = new($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize"),
                    TokenUrl = new($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token"),
                    Scopes = { [scope] = "Access the API as signed-in user" }
                }
            }
        });
        // Enforce adding 401/403 responses in Swagger for [Authorize] endpoints
        opt.OperationFilter<AuthorizeCheckOperationFilter>();
    }
});

// ──────────────────────────────────────────────────────────────────────────────
// 7) MediatR & FluentValidation pipeline behaviors & DI
//    - SerilogEnrichingBehavior adds RequestName/CorrelationId to log context
//    - ValidationBehavior throws on any FluentValidation failures
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetPatientsRequest>());
// builder.Services.AddValidatorsFromAssemblyContaining<PatientGetAllRequestValidator>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SerilogEnrichingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// ──────────────────────────────────────────────────────────────────────────────
// 8) Build & run the app, wiring up exception handling, swagger, auth, CORS, etc.
// ──────────────────────────────────────────────────────────────────────────────
try
{
    Log.Information("Starting up");

    var app = builder.Build();

    app.UseRequestLogMiddleware();
    app.UseGlobalExceptionHandling();

    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.OAuthClientId(builder.Configuration["Swagger:ClientId"]);
        ui.OAuthUsePkce();
    });

    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors("Spa");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
