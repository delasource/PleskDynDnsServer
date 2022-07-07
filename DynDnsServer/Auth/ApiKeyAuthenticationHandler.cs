using System.Security.Claims;
using System.Text.Encodings.Web;
using DynDnsServer.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace DynDnsServer.Auth;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                                       ILoggerFactory                               logger,
                                       UrlEncoder                                   encoder,
                                       ISystemClock                                 clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = Request.Query["ApiKey"];
        if (apiKey.Count == 0)
            return AuthenticateResult.Fail("No ApiKey found.");

        if (apiKey[0] != FileDb.AppSettings.ApiKey)
            return AuthenticateResult.Fail("Invalid ApiKey.");

        await Task.Delay(1);

        var identity = new ClaimsIdentity("ApiKey");
        identity.AddClaim(new Claim(ClaimTypes.Name, "ApiKeyUser"));
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "ApiKey"));
    }
}
