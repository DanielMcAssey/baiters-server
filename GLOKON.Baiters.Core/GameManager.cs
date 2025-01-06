using GLOKON.Baiters.Core.Chat;
using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Plugins;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core
{
    public sealed class GameManager(
        IOptions<WebFishingOptions> options,
        ChatManager chat,
        BaitersServer server,
        ActorSpawner spawner)
    {
        public WebFishingOptions Options => options.Value;

        public BaitersServer Server => server;

        public ActorSpawner Spawner => spawner;

        public ChatManager Chat => chat;

        public void Setup()
        {
            Dispatch.OnDebugCallback = (type, str, server) => Log.Debug("\n[Callback {0} {1}]\n{2}", type, (server ? "server" : "client"), str);
            Dispatch.OnException = (e) => Log.Error(e.InnerException, e.Message);

            Server.Setup();
            Spawner.Setup();
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (options.Value.PluginsEnabled)
            {
                PluginLoader.LoadPlugins(this);
            }

            Task.Run(() => Server.RunAsync(cancellationToken), CancellationToken.None);
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
