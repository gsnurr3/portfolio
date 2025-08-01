using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using RESTfulAPI.Infrastructure.Auth;
using RESTfulAPI.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────
// CORS
// ─────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Spa", p => p
        .WithOrigins("http://localhost:5000",
                     "https://localhost:5001")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ─────────────────────────────────────────────────────────
// Authentication  +  Authorization
// ─────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    // Dummy scheme that always succeeds – lets you keep [Authorize] attributes
    builder.Services
        .AddAuthentication("Dev")
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });

    // Optional: mark every request as authorized even if no [Authorize] attribute
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
                // Skip the default WWW-Authenticate header if you don't need it
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

// ─────────────────────────────────────────────────────────
// MVC  +  Swagger
// ─────────────────────────────────────────────────────────
builder.Services.Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);

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

    opt.SwaggerDoc("v1", new() { Title = "Demo API", Version = "v1" });

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

// ── MediatR + FluentValidation ───────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<PingRequest>());
builder.Services.AddValidatorsFromAssemblyContaining<PingRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// ─────────────────────────────────────────────────────────
// Middleware pipeline
// ─────────────────────────────────────────────────────────
// Global exception handling
app.UseGlobalExceptionHandling();

// if (app.Environment.IsDevelopment()) // commented out to test without frontend in production for the time being
// {
app.UseSwagger();
app.UseSwaggerUI(ui =>
{
    ui.OAuthClientId(builder.Configuration["Swagger:ClientId"]);
    ui.OAuthUsePkce();
});
// }

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();    // always present – scheme differs by env
app.UseAuthorization();
app.UseCors("Spa");

app.MapControllers();
app.Run();