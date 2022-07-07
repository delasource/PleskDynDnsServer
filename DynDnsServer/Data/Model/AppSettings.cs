namespace DynDnsServer.Data.Model;

public record AppSettings
{
    // public string? MailServer        { get; set; }
    // public string? MailSenderAddress { get; set; }
    // public string? MailUser          { get; set; }
    // public string? MailPass          { get; set; }
    // public string? MailPort          { get; set; }

    public string ApiKey   { get; set; } = "12345";
    public string AdminKey { get; set; } = "1337";

    public string PleskCommand    { get; set; } = "/usr/sbin/plesk";
    public bool   UseShellExecute { get; set; } = false;
}
