using GLOKON.Baiters.Core;

namespace GLOKON.Baiters.Server.HostedServices
{
    public class BaitersServerService(BaitersServer server) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            server.Setup();
            Task.Run(async () =>
            {
                await server.RunAsync(cancellationToken);
            }, CancellationToken.None);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            server.Stop();
            return Task.CompletedTask;
        }
    }
}
