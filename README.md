# DynDns Server for Plesk

A minimalistic server to be used with Plesk DNS. The goal is to have an external client with a dynamic IP that should be made publicly visible with a dyndns-name aka subdomain.


## Setup the Server

* Build this project. For example with

```bash
dotnet publish -c Release -r linux-x64 --self-contained false
```

* Install dotnet 6. If built in advance with above command, only the runtime is required on the server (or if self-contained is set true, not even the runtime must be installed but this increases the executables size).
* Create a service, for example like this: `nano /etc/systemd/system/dyndnsserver.service`
```ini
[Unit]
Description=DynDnsServer with DotNet
After=network.target

[Service]
ExecStart=/opt/dotnet/dyndns/DynDnsServer
Restart=always
User=root
Group=root
Environment=PATH=/usr/bin:/usr/local/bin
WorkingDirectory=/opt/dotnet/dyndns

[Install]
WantedBy=multi-user.target
```

* You could use another user instead of root, but it needs full access to the plesk-command. 
* If your plesk command is not '/usr/sbin/plesk' change the app.db text file (will be generated after the first launch).
* Remember to make the executable executable. for example with `chmod +x DynDnsServer`
* Start and run the service with `systemctl enable dyndnsserver.service && systemctl start dyndnsserver.service`
* Allow specific firewall access to port 5051 (or whatever configured in appsettings.json) or setup nginx proxy.
* Change TTL of the dns record to one hour (3600) in Plesk
* For registration of a dynDNS record, open the site on http://YOUR_SERVER:5051/swagger
* Default credentials (change in app.db text file) are: 
```
apiKey=12345
adminKey=1337
```
* After adding a new dyndns entry you will get an url or curl example code back from the api.
* Separate names for ipv4 and ipv6 for the "controller server" must be set up,
  if both services are required. 
  To update the records, a simple curl-call has to be executed (see below). 
  The "Remote Host" (IP) of the call will be used as dynDNS target IP. 
  So if you need ipv4 or ipv6 specific, you should make sure, that the 
  server-address in the curl-call ONLY resolves to one of these specific.
  For example: 
```dns
ipv4.YOUR_DOMAIN.COM  3600  IN  A     1.2.3.4
ipv6.YOUR_DOMAIN.COM  3600  IN  AAAA  fe80:1234::1
```



## Setup the client

* `crontab -e`
```cron
# Run DynDNS update every hour
0 * * * * curl -X 'POST' 'http://ipv4.YOUR_DOMAIN.COM:5051/dyn/refresh?apiKey=12345&r=testing.mydomain.net&h=3b352e82'
```


## TODO

* Test ipv6
