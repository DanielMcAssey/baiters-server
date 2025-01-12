﻿using GLOKON.Baiters.Server.Configuration;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Serilog;
using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core;
using GLOKON.Baiters.Server.HostedServices;
using GLOKON.Baiters.Core.Packets;
using GLOKON.Baiters.Core.Chat;
using Serilog.Events;

namespace GLOKON.Baiters.Server
{
    public class Server
    {
        private readonly IConfiguration _configuration;

        public Server(IConfiguration configuration)
        {
            _configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Internal", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilog();

            var serverSection = _configuration.GetRequiredSection("Server");
            ServerOptions serverOptions = serverSection.Get<ServerOptions>() ?? new();
            services.Configure<ServerOptions>(serverSection);
            services.Configure<WebFishingOptions>(_configuration.GetRequiredSection("WebFishing"));

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
            services.AddSingleton<ChatManager>();
            //services.AddSingleton<BaitersServer, P2PBaitersServer>(); // SteamNetworking Server
            //services.AddSingleton<BaitersServer, SocketBaitersServer>(); // SteamNetworkingSockets Server
            services.AddSingleton<BaitersServer, NetworkMessageBaitersServer>(); // SteamNetworkingMessages Server
            services.AddSingleton<ActorSpawner>();
            services.AddSingleton<GameManager>();
            services.AddHostedService<GameManagerService>();

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
