using System;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Microsoft.Extensions.Hosting;
using Serilog;
using watch.KubeController;
using watch.Model.Challange;
using watch.Model.Challange.Kubernetes;

namespace acme_resolver.Services
{
    public class AcmeHttpChallengeService : BackgroundService, IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IKubernetes _kubernetesClient;
        private readonly IChallengeTokenRepository _tokenRepository;

        public AcmeHttpChallengeService(
            IKubernetes kubernetesClient,
            IChallengeTokenRepository tokenRepository
            )
        {
            _logger = Log.ForContext<AcmeHttpChallengeService>();
            if (kubernetesClient is null) throw new ArgumentNullException(nameof(kubernetesClient));
            _kubernetesClient = kubernetesClient;
            _tokenRepository = tokenRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"Acme-Resolver activation service is starting...");
            stoppingToken.Register(() => _logger.Information("Acme-Resolver activation service is stopping"));
            var controller = new Controller(_kubernetesClient);
            //todo: move namespace to appsetting
            await controller.StartAsync<ChallangeResource>("app-gw", async (t, crd, client) => await ResourceHandler(t, crd, client), stoppingToken);
        }

        private async Task ResourceHandler(WatchEventType type, ChallangeResource crd, IKubernetes client)
        {
            _logger.Information($"{type} Dns: {crd.Spec.DnsName} Key: {crd.Spec.Key}");

            switch (type)
            {
                case WatchEventType.Added:
                    await _tokenRepository.RegisterTokenAsync(crd.Spec);
                    break;
                case WatchEventType.Deleted:
                    await _tokenRepository.DeleteTokenAsyc(crd.Spec.Token);
                    break;
                case WatchEventType.Modified:
                    await _tokenRepository.UpdateTokenAsync(crd.Spec);
                    break;
                case WatchEventType.Error:
                _logger.Error("Error on resource {@resouce}",crd.Spec);
                    await _tokenRepository.DeleteTokenAsyc(crd.Spec.Token);
                    
                    break;
                default:
                    _logger.Warning("Unhandled watching event type {@type}",type);
                break;
            }
        }
    }
}
