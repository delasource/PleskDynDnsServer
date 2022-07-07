namespace DynDnsServer.Data.Model;

public record ManagedDnsRecord
{
    public string Type         { get; set; } = "a";
    public string Domain       { get; set; } = "example.com";
    public string Resource     { get; set; } = "";
    public string Ip           { get; set; } = "1.2.3.4";
    public string SecurityHash { get; set; } = "";

    public string Fqdn
        => $"{Resource}.{Domain}";

    public override string ToString()
        => $"{Type}:{Resource}.{Domain} -> {Ip}";
}
