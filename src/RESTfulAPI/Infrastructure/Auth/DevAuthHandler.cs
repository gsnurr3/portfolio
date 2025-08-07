using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace RESTfulAPI.Infrastructure.Auth
{
    /// <summary>
    /// A development‐only authentication handler that automatically
    /// succeeds every request by issuing a fixed “dev-user” principal.
    /// This allows local testing of [Authorize]-protected endpoints
    /// without real tokens.
    /// </summary>
    internal sealed class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DevAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user"),
                new Claim(ClaimTypes.Name,            "Development User")
            }, Scheme.Name);

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}