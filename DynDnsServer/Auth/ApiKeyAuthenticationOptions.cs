using Microsoft.AspNetCore.Authentication;

namespace DynDnsServer.Auth;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public override void Validate()
    {
        base.Validate();
    }

    public override void Validate(string scheme)
    {
        base.Validate(scheme);
    }
}
