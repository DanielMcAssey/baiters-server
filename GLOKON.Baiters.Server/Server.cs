using GLOKON.Baiters.Server.Configuration;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Serilog;
using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core;
using GLOKON.Baiters.Server.HostedServices;
using GLOKON.Baiters.Core.Packets;

namespace GLOKON.Baiters.Server
{
    public class Server(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilog();

            var serverSection = Configuration.GetRequiredSection("Server");
            ServerOptions serverOptions = serverSection.Get<ServerOptions>() ?? new();
            services.Configure<ServerOptions>(serverSection);
            services.Configure<WebFishingOptions>(Configuration.GetRequiredSection("WebFishing"));

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            services.AddSingleton<PacketManager>();
            //services.AddSingleton<BaitersServer, SocketBaitersServer>(); // TODO: When GameServer supports SteamNetworkingSockets
            services.AddSingleton<BaitersServer, P2PBaitersServer>();
            services.AddSingleton<ActorSpawner>();
            services.AddHostedService<BaitersServerService>();
            services.AddHostedService<ActorSpawnerService>();

            if (serverOptions.LetsEncrypt.IsEnabled())
            {
                services.AddLettuceEncrypt(options =>
                {
                    options.AcceptTermsOfService = true;
                    options.DomainNames = [.. serverOptions.LetsEncrypt.Domains];
                    options.EmailAddress = serverOptions.LetsEncrypt.EmailAddress;
                    options.UseStagingServer = serverOptions.LetsEncrypt.UseStagingServer;
                });
            }

            services.AddDataProtection()
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
            services.AddControllers();
            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<ServerOptions> serverOptionsVal)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            if (serverOptionsVal.Value is ServerOptions serverOptions && serverOptions.IsUsingHttps())
            {
                if (serverOptions.UseHsts)
                {
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
            }

            app.UseDefaultFiles()
                .UseRouting()
                .UseCors()
                .UseAuthentication()
                .UseAuthorization()
                .UseSerilogRequestLogging()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                })
                .UseStaticFiles();
        }
    }
}
