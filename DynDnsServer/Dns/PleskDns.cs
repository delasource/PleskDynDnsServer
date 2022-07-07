using System.Diagnostics;
using DynDnsServer.Data;

namespace DynDnsServer.Dns;

public class PleskDns : IDnsSetter
{
    public async Task SetDnsAsync(string recordType,
                                  string domain,
                                  string resource,
                                  string previousIp,
                                  string newTargetIp)
    {
        if (!File.Exists(FileDb.AppSettings.PleskCommand))
            throw new ArgumentException("Plesk command not found (usually /usr/sbin/plesk)");

        if (previousIp == newTargetIp)
        {
            Console.WriteLine("IPs are the same, no need to update");
            return;
        }

        // Protect from injection (bash)
        domain      = InjectionProtection(domain);
        resource    = InjectionProtection(resource);
        previousIp  = InjectionProtection(previousIp);
        newTargetIp = InjectionProtection(newTargetIp);

        // start "plesk bin dns -a DOMAIN -a RESOURCE -ip TARGETIP"
        try
        {
            // Try to delete
            await Start(recordType switch
            {
                "a"    => $"bin dns -d \"{domain}\" -a \"{resource}\" -ip \"{previousIp}\"",
                "aaaa" => $"bin dns -d \"{domain}\" -aaaa \"{resource}\" -ip \"{previousIp}\"",
                _      => throw new ArgumentException("Invalid record type")
            });

            // Add
            await Start(recordType switch
            {
                "a"    => $"bin dns -a \"{domain}\" -a \"{resource}\" -ip \"{newTargetIp}\"",
                "aaaa" => $"bin dns -a \"{domain}\" -aaaa \"{resource}\" -ip \"{newTargetIp}\"",
                _      => throw new ArgumentException("Invalid record type")
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static Task Start(string cmd)
    {
        Console.WriteLine($"Executing: plesk {cmd}");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName        = FileDb.AppSettings.PleskCommand,
                Arguments       = cmd,
                UseShellExecute = FileDb.AppSettings.UseShellExecute,
            }
        };
        process.Start();
        return process.WaitForExitAsync();
    }

    // Escape critical bash characters
    private static string InjectionProtection(string s)
        => s.Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "")
            .Replace("|", "")
            .Replace("<", "")
            .Replace(">", "");
}
