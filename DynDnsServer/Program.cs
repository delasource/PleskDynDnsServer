using DynDnsServer.Auth;
using DynDnsServer.Dns;
using FastEndpoints;
using FastEndpoints.Swagger;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc(addJWTBearerAuth: false,
    settings: s =>
    {
        s.AddAuth("ApiKey",
            new OpenApiSecurityScheme
            {
                Type   = OpenApiSecuritySchemeType.ApiKey,
                Scheme = "ApiKey",
                Name   = "ApiKey",
                In     = OpenApiSecurityApiKeyLocation.Query
            });
    });

// add apikeyauthentication
builder.Services.AddAuthentication(options =>
       {
           options.DefaultAuthenticateScheme = "ApiKey";
           options.DefaultChallengeScheme    = "ApiKey";
       })
       .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey",
           _ =>
           {
           });

builder.Services.AddTransient<IDnsSetter, PleskDns>();


// ----------------------------------------------------------------------------------------
var app = builder.Build();

app.MapGet("/", c => c.Response.SendUnauthorizedAsync());
app.MapGet("robots.txt",
    () => @"User-Agent: *
Disallow: /

");

app.UseDefaultExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseOpenApi();
app.UseSwaggerUi3(s => s.ConfigureDefaults());

app.Run();
