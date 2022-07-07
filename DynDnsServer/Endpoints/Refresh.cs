using System.Net.Sockets;
using DynDnsServer.Data;
using DynDnsServer.Dns;
using FastEndpoints;

namespace DynDnsServer.Endpoints;

public class Refresh : Endpoint<RefreshRequest, RefreshResponse>
{
    public const string ApiUrl = "dyn/refresh";

    private readonly IDnsSetter _dnsSetter;

    public Refresh(IDnsSetter dnsSetter)
    {
        _dnsSetter = dnsSetter;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ApiUrl);
        Description(b => b
                         // .Accepts<RefreshRequest>()
                         .Produces<RefreshResponse>()
                         .ProducesProblem(403)
                         .ProducesProblem(404)
        );
        Summary(s =>
        {
            // s.Summary     = "short description";
            // s.Description = "long description";
            s[403] = "api key or security has does not match";
            s[404] = "no matching record found";

            s.RequestParam(r => r.Resource!, "Resource (FQDN)");
            s.RequestParam(r => r.SecurityHash!, "Security Hash");
        });
        AuthSchemes("ApiKey");
    }

    public override async Task HandleAsync(RefreshRequest req, CancellationToken ct)
    {
        var ipVersion = HttpContext.Connection.RemoteIpAddress?.AddressFamily == AddressFamily.InterNetworkV6
            ? "aaaa"
            : "a";

        var record = FileDb.Records.Find(r => r.Fqdn == req.Resource && r.Type == ipVersion);
        if (string.IsNullOrWhiteSpace(req.Resource) || string.IsNullOrWhiteSpace(req.SecurityHash) || record == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (record.SecurityHash != req.SecurityHash)
        {
            await SendForbiddenAsync(ct);
            return;
        }

        string previousIpString = record.Ip;

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        Console.WriteLine("Refresh requested from: " + remoteIp);

        if (remoteIp == null)
        {
            await SendForbiddenAsync(ct);
            return;
        }

        if (remoteIp.IsIPv4MappedToIPv6)
            remoteIp = remoteIp.MapToIPv4();

        record.Ip = remoteIp.ToString();
        FileDb.Save();

        // Update DNS
        await _dnsSetter.SetDnsAsync(record.Type, record.Domain, record.Resource, previousIpString, record.Ip);

        await SendAsync(new RefreshResponse(), cancellation: ct);
    }
}
