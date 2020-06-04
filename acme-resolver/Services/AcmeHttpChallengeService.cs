using System;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public AcmeHttpChallengeService(
            IKubernetes kubernetesClient,
            IChallengeTokenRepository tokenRepository,
            IConfiguration configuration
            )
        {
            _logger = Log.ForContext<AcmeHttpChallengeService>();
            if (kubernetesClient is null) throw new ArgumentNullException(nameof(kubernetesClient));
            _kubernetesClient = kubernetesClient;
            _tokenRepository = tokenRepository;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"Acme-Resolver activation service is starting...");
            stoppingToken.Register(() => _logger.Information("Acme-Resolver activation service is stopping"));
            var controller = new Controller(_kubernetesClient);
            var ns = _configuration.GetValue<String>("Namespaces:ApplicationGw");
            await controller.StartAsync<ChallangeResource>(ns, async (t, crd, client) => await ResourceHandler(t, crd, client), stoppingToken);
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
