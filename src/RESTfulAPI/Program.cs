using FluentValidation;
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
using RESTfulAPI.Infrastructure.Auth;
using RESTfulAPI.Presentation.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Replace default logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)   // reads the Serilog section above
    .Enrich.FromLogContext()                         // ensures CorrelationId etc. flow
    .CreateLogger();

builder.Host.UseSerilog();   // plug Serilog into ASP.NET logging

var sql = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("Sql")
    : builder.Configuration["AZURE_SQL_CONNECTIONSTRING"];

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(sql,
        b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
              .MigrationsHistoryTable("__EFMigrationsHistory_App", "app")));

builder.Services.AddDbContext<LogDbContext>(o =>
    o.UseSqlServer(sql,
        b => b.MigrationsAssembly(typeof(LogDbContext).Assembly.FullName)
              .MigrationsHistoryTable("__EFMigrationsHistory_Log", "log")));

// CORS
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Spa", p => p
        .WithOrigins("http://localhost:5000",
                     "https://localhost:5001")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Authentication  +  Authorization
if (builder.Environment.IsDevelopment())
{
    // Dummy scheme that always succeeds – lets you keep [Authorize] attributes
    builder.Services
        .AddAuthentication("Dev")
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });

    // Mark every request as authorized even if no [Authorize] attribute
    builder.Services.AddAuthorization(opt =>
    {
        opt.FallbackPolicy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .Build();
    });
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                // Skip the default WWW-Authenticate header. Currenlty not needed.
                // ctx.Response.Headers.Remove("WWW-Authenticate");

                var payload = ApiResponse.Unauthorized("Access token is missing or invalid.");
                ctx.HandleResponse();                         // stop default processing
                ctx.Response.StatusCode = payload.Status;    // 401
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsJsonAsync(payload);
            },

            OnForbidden = ctx =>
            {
                var payload = ApiResponse.Forbidden("You do not have permission.");
                ctx.Response.StatusCode = payload.Status;    // 403
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsJsonAsync(payload);
            }
        };
    });

    builder.Services.AddAuthorization();
}

// MVC  +  Swagger
builder.Services.Configure<ApiBehaviorOptions>(opts =>
{
    opts.SuppressModelStateInvalidFilter = true;

    opts.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e =>
                            new ApiError("Validation",
                                         e.ErrorMessage,
                                         kvp.Key)))
                        .ToList();

        var envelope = ApiResponse.ValidationFailed(errors);
        return new ObjectResult(envelope) { StatusCode = envelope.Status }; // 422
    };
});

builder.Services.AddControllers(opts =>
{
    opts.Conventions.Insert(0, new RoutePrefixConvention("api"));
    opts.Conventions.Add(
        new RouteTokenTransformerConvention(
            new SlugifyParameterTransformer()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    var scope = builder.Configuration["AzureAd:Scopes"];
    var tenant = builder.Configuration["AzureAd:TenantId"];

    opt.SwaggerDoc("v1", new() { Title = "Gordon's API", Version = "v1" });

    if (!string.IsNullOrWhiteSpace(scope))
    {
        opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new()
                {
                    AuthorizationUrl = new($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize"),
                    TokenUrl = new($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token"),
                    Scopes = { [scope] = "Access the API as signed‑in user" }
                }
            }
        });
        opt.OperationFilter<AuthorizeCheckOperationFilter>();
    }
});

// ── MediatR + FluentValidation
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<PingRequest>());
builder.Services.AddValidatorsFromAssemblyContaining<PingRequestValidator>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>),
                              typeof(SerilogEnrichingBehavior<,>));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>),
                              typeof(ValidationBehavior<,>));

try
{
    Log.Information("Starting up");

    var app = builder.Build();

    // ── middleware pipeline
    app.UseGlobalExceptionHandling();

    // if (app.Environment.IsDevelopment()) commented out for testing PROD as no frontend for the time being
    // {
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.OAuthClientId(builder.Configuration["Swagger:ClientId"]);
        ui.OAuthUsePkce();
    });
    // }

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
    // Will capture anything that prevents the host from starting or crashes it later
    Log.Fatal(ex, "API terminated unexpectedly");
}
finally
{
    // Flush and close sinks (important when using file / Seq / etc.)
    Log.CloseAndFlush();
}