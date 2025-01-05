using GLOKON.Baiters.Core;

namespace GLOKON.Baiters.Server.HostedServices
{
    public class ActorSpawnerService(ActorSpawner spawner) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            spawner.Setup();
            Task.Run(async () =>
            {
                await spawner.RunAsync(cancellationToken);
            }, CancellationToken.None);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
