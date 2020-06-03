using System;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using watch.KubeController;
using watch.Model.Challange;
using watch.Model.Challange.Kubernetes;

namespace acme_resolver.Services
{
    public class AcmeHttpChallengeService :BackgroundService, IHostedService, IDisposable
    {
        private readonly ILogger<AcmeHttpChallengeService> _logger;
        private readonly IKubernetes _kubernetesClient;
        private readonly ChallangeResource _cr ;
        private readonly IChallengeTokenRepository _tokenRepository;

        public AcmeHttpChallengeService(
            ILogger<AcmeHttpChallengeService> logger,
            IKubernetes kubernetesClient,
            IChallengeTokenRepository tokenRepository
            )
        {
            _logger = logger;
            if (kubernetesClient is null) throw new ArgumentNullException(nameof(kubernetesClient));
            _kubernetesClient = kubernetesClient;
            _tokenRepository = tokenRepository;
            _cr = new ChallangeResource();            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Acme-Resolver activation service is starting...");
            stoppingToken.Register(()=>_logger.LogInformation("Acme-Resolver activation service is stopping"));
            var controller = new Controller(_kubernetesClient);
            
            await controller.StartAsync<ChallangeResource>(_cr,"app-gw",async (type,crd,client)=>{
                _logger.LogInformation($"{type} Dns: {crd.Spec.DnsName} Key: {crd.Spec.Key}");
                await _tokenRepository.RegisterTokenAsync(new TokenEntity(){
                    DnsName = crd.Spec.DnsName,
                    Key = crd.Spec.Key,
                    Token = crd.Spec.Token
                });
            },stoppingToken);
        }
    }
}
