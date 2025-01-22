using GLOKON.Baiters.Core;
using Serilog;

namespace GLOKON.Baiters.Server.HostedServices
{
    public class GameManagerService(GameManager gm, IHostApplicationLifetime appLifetime) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                gm.Setup();
                gm.OnServerStop += Gm_OnServerStop;
                gm.Start(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Server encountered an error");
                appLifetime.StopApplication();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            gm.Stop();
            return Task.CompletedTask;
        }

        private void Gm_OnServerStop()
        {
            appLifetime.StopApplication();
        }
    }
}
