using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using acme_resolver.Repository;
using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using watch.KubeController;
using watch.Model.Challange;
using watch.Model.Challange.Kubernetes;
using static System.Threading.Tasks.Task;

namespace acme_resolver.Services
{
    public class AcmeHttpChallengeService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IKubernetes _kubernetesClient;
        private readonly IChallengeTokenRepository _tokenRepository;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IConfiguration _configuration;

        public AcmeHttpChallengeService(
            IKubernetes kubernetesClient,
            IChallengeTokenRepository tokenRepository,
            IHostApplicationLifetime lifetime,
            IConfiguration configuration
        )
        {
            _logger = Log.ForContext<AcmeHttpChallengeService>();
            _kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            _tokenRepository = tokenRepository;
            _lifetime = lifetime;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"Acme-Resolver activation service is starting...");
            stoppingToken.Register(() => _logger.Information("Acme-Resolver activation service is stopping"));
            var namespaces = _configuration.GetSection("Namespaces").Get<string[]>();
            try
            {
                var controller = new Controller(_kubernetesClient);
                var tasks = new List<Task>();
                foreach (var ns in namespaces)
                {
                    _logger.Information("Controller namespace {@ns} is starting up...", ns);
                    tasks.Add(Run(async () =>
                    {
                        await controller.StartAsync<ChallangeResource>(ns, async (t, crd, client) =>
                            await ResourceHandler(t, crd, client), stoppingToken);
                    }, stoppingToken));
                }
                await WhenAll(tasks.ToArray());

            }
            catch (Exception ex)
            {
                await Delay(100);
                _logger.Error(ex, "Kubernetes Controller start error! Namespace: {@ns} KubernetesClient: {@url}", string.Join(",",namespaces),
                    _kubernetesClient.BaseUri);
                _lifetime.StopApplication();
            }
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
                    _logger.Error("Error on resource {@resouce}", crd.Spec);
                    await _tokenRepository.DeleteTokenAsyc(crd.Spec.Token);

                    break;
                default:
                    _logger.Warning("Unhandled watching event type {@type}", type);
                    break;
            }
        }
    }
}