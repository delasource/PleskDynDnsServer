namespace DynDnsServer.Dns;

public interface IDnsSetter
{
    Task SetDnsAsync(string recordType,
                     string domain,
                     string resource,
                     string previousIp,
                     string newTargetIp);
}
