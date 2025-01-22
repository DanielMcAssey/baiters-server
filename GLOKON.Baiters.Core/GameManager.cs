using GLOKON.Baiters.Core.Chat;
using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Exceptions;
using GLOKON.Baiters.Core.Packets;
using GLOKON.Baiters.Core.Plugins;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core
{
    public sealed class GameManager(
        IOptions<WebFishingOptions> _options,
        PacketManager packet,
        ChatManager chat,
        BaitersServer server,
        ActorSpawner spawner,
        ActorActioner actioner)
    {
        private readonly WebFishingOptions options = _options.Value;

        public WebFishingOptions Options => options;

        public BaitersServer Server => server;

        public ActorSpawner Spawner => spawner;

        public ActorActioner Actioner => actioner;

        public ChatManager Chat => chat;

        /// <summary>
        /// Called when the server has stopped for any reason except for manual stop
        /// </summary>
        public event Action? OnServerStop;

        public void Setup()
        {
            if (Options.SteamDebug)
            {
                Dispatch.OnDebugCallback = (type, str, server) => Log.Debug("\n[Callback {0} {1}]\n{2}", type, (server ? "server" : "client"), str);
            }

            Dispatch.OnException = (e) => Log.Error(e.InnerException, e.Message);

            packet.Setup();
            Server.Setup();
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (options.PluginsEnabled)
            {
                PluginLoader.LoadPlugins(this);
            }

            Task.Run(async () =>
            {
                try
                {
                    await Server.RunAsync(cancellationToken);
                }
                catch (GameShutdownException ex)
                {
                    Log.Error(ex, "Server encountered an error");
                    OnServerStop?.Invoke();
                }
            }, CancellationToken.None);
            Task.Run(() => Spawner.RunAsync(cancellationToken), CancellationToken.None);
        }

        public void Stop()
        {
            try
            {
                Server.Stop();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to stop server");
            }

            PluginLoader.UnloadPlugins();
        }
    }
}
