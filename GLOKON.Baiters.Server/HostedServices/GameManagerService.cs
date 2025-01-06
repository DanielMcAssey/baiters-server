using GLOKON.Baiters.Core;

namespace GLOKON.Baiters.Server.HostedServices
{
    public class GameManagerService(GameManager gm) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            gm.Setup();
            gm.Start(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            gm.Stop();
            return Task.CompletedTask;
        }
    }
}
