using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Options;
using System.Net;
using GLOKON.Baiters.Server.Configuration;
using Serilog;

namespace GLOKON.Baiters.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder<Server>(args)
                .SuppressStatusMessages(true)
                .ConfigureKestrel((context, kestrelOptions) =>
                {
                    kestrelOptions.AddServerHeader = false;

                    var serverOptionsVal = kestrelOptions.ApplicationServices.GetRequiredService<IOptions<ServerOptions>>();

                    if (serverOptionsVal.Value is ServerOptions serverOptions)
                    {
                        if (serverOptions.MaxUploadSize > 0)
                        {
                            kestrelOptions.Limits.MaxRequestBodySize = serverOptions.MaxUploadSize;
                        }
                        else
                        {
                            kestrelOptions.Limits.MaxRequestBodySize = null;
                        }

                        if (!string.IsNullOrEmpty(serverOptions.ListenOn))
                        {
                            IPAddress listenAddress = IPAddress.Parse(serverOptions.ListenOn);
                            kestrelOptions.Listen(listenAddress, serverOptions.HttpPort);
                            Log.Information("Listening (HTTP): http://{0}:{1}", listenAddress, serverOptions.HttpPort);

                            if (serverOptions.LetsEncrypt.IsEnabled())
                            {
                                kestrelOptions.Listen(listenAddress, serverOptions.HttpsPort, listenOptions =>
                                {
                                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                                    listenOptions.UseHttps(httpsOptions =>
                                    {
                                        httpsOptions.UseLettuceEncrypt(kestrelOptions.ApplicationServices);
                                    });
                                });

                                Log.Information("Listening (LetsEncrypt): https://{0}:{1}", listenAddress, serverOptions.HttpsPort);
                            }
                            else if (serverOptions.SSL.IsEnabled())
                            {
                                if (!File.Exists(serverOptions.SSL.CertificatePath))
                                {
                                    Log.Error("Failed to find SSL certificate: {0}", serverOptions.SSL.CertificatePath);
                                }
                                else
                                {
                                    kestrelOptions.Listen(listenAddress, serverOptions.HttpsPort, listenOptions =>
                                    {
                                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                                        listenOptions.UseHttps(serverOptions.SSL.CertificatePath, serverOptions.SSL.CertificatePassword);
                                    });

                                    Log.Information("Listening (SSL): https://{0}:{1}", listenAddress, serverOptions.HttpsPort);
                                }
                            }
                            else if (context.HostingEnvironment.IsDevelopment())
                            {
                                // Use development certificate
                                kestrelOptions.Listen(listenAddress, serverOptions.HttpsPort, listenOptions =>
                                {
                                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                                    listenOptions.UseHttps();
                                });

                                Log.Information("Listening (DevSSL): https://{0}:{1}", listenAddress, serverOptions.HttpsPort);
                            }
                        }

                        if (!string.IsNullOrEmpty(serverOptions.ListenOnSocket))
                        {
                            kestrelOptions.ListenUnixSocket(serverOptions.ListenOnSocket);
                            Log.Information("Listening (Unix Socket): {0}", serverOptions.ListenOnSocket);
                        }

                        if (!string.IsNullOrEmpty(serverOptions.ListenOnNamedPipe))
                        {
                            kestrelOptions.ListenNamedPipe(serverOptions.ListenOnNamedPipe);
                            Log.Information("Listening (Named Pipe): {0}", serverOptions.ListenOnNamedPipe);
                        }
                    }
                })
                .UseUrls();

            var app = builder.Build();
            Log.Information("Baiters Server is now running");
            app.Run();
        }
    }
}
