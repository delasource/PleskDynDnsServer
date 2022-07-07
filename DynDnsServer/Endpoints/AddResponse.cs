namespace DynDnsServer.Endpoints;

public class AddResponse
{
    public string Status       { get; set; } = "error";
    public string Fqdn         { get; set; } = "";
    public string SecurityHash { get; set; } = "";
    public string Url          { get; set; } = "";
    public string Curl         { get; set; } = "";
}