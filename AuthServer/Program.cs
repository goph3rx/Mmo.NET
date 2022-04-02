using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Mmo.AuthServer;

WebHost.
    CreateDefaultBuilder(args).
    UseKestrel((context, options) =>
    {
        var port = context.Configuration.GetValue<int>("AuthServer:Port", 2106);
        options.ListenAnyIP(port, builder => builder.UseConnectionHandler<Acceptor>());
    }).
    Configure(app =>
    {
        // Will configure app here
    }).
    Build().
    Run();
