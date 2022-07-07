using System.Security.Cryptography;
using System.Text;
using DynDnsServer.Data;
using DynDnsServer.Data.Model;
using FastEndpoints;

namespace DynDnsServer.Endpoints;

public class Add : Endpoint<AddRequest, AddResponse>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("dyn/add");
        Description(b => b
                         .Accepts<AddRequest>("application/json")
                         .Produces<AddResponse>()
                         .ProducesProblem(403)
                         .ProducesProblem(409)
                         .ProducesProblem(500)
        );
        Summary(s =>
        {
            // s.Summary     = "short description";
            // s.Description = "long description";
            s[403] = "admin key missing or invalid";
            s[409] = "record already exists";
            s[500] = "forbidden";

            s.RequestParam(r => r.Resource, "Resource (FQDN)");
        });
        AuthSchemes("ApiKey");
    }

    public override async Task HandleAsync(AddRequest req, CancellationToken ct)
    {
        if (req.AdminKey != FileDb.AppSettings.AdminKey)
        {
            await SendForbiddenAsync(ct);
            return;
        }

        var recordExists = FileDb.Records.Any(r => r.Type == req.Type &&
                                                   r.Domain == req.Domain &&
                                                   r.Resource == req.Resource);

        if (recordExists)
        {
            await SendErrorsAsync(409, ct);
            return;
        }

        var record = new ManagedDnsRecord()
        {
            Type     = req.Type,
            Domain   = req.Domain,
            Resource = req.Resource,
            SecurityHash = BitConverter
                           .ToString(MD5.Create()
                                        .ComputeHash(Encoding.UTF8.GetBytes($"{req.Type}!{req.Resource}.{req.Domain}")))
                           .Replace("-", "")
                           .ToLowerInvariant()[..8]
        };
        FileDb.Records.Add(record);
        // Add the other record (a|aaaa)
        FileDb.Records.Add(new ManagedDnsRecord()
        {
            Type         = req.Type == "a" ? "aaaa" : "a",
            Domain       = req.Domain,
            Resource     = req.Resource,
            SecurityHash = record.SecurityHash
        });

        FileDb.Save();

        string url =
            $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{Refresh.ApiUrl}?apiKey=__APIKEY_SEE_DB_FILE__&r={record.Fqdn}&h={record.SecurityHash}";

        await SendAsync(new AddResponse()
            {
                Status       = "created",
                Fqdn         = record.Fqdn,
                SecurityHash = record.SecurityHash,
                Url          = url,
                Curl         = $"curl -X 'POST' '{url}'",
            },
            cancellation: ct);
    }
}
