namespace DynDnsServer.Endpoints;

public class AddRequest
{
    public string AdminKey { get; set; } = "";

    public string Type     { get; set; } = "a";
    public string Domain   { get; set; } = "example.com";
    public string Resource { get; set; } = "";
    public string Ip       { get; set; } = "1.1.1.1";
}