using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using k8s;
using k8s.Models;
using Serilog;

namespace watch.KubeController
{
    public class Controller
    {
        private readonly IKubernetes _client;
        private readonly ILogger _logger;

        public Controller(IKubernetes client)
        {
            _client = client;
            _logger = Log.ForContext<Controller>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cr">Custom Resource</param>
        /// <param name="ns">Namespace</param>
        /// <param name="handler">Handler</param>
        /// <param name="token">Cancellation Token</param>
        /// <typeparam name="CRD">Customresource Definition Object which inherits from V1CustomResourceDefinition</typeparam>
        /// <returns></returns>
        public async Task StartAsync<CRD>(String ns, Action<WatchEventType, CRD, IKubernetes> handler,
            CancellationToken token, CRD cr = null) where CRD : V1CustomResourceDefinition
        {
            if (token.IsCancellationRequested) return;

            if (cr == null)
                cr = (CRD) Activator.CreateInstance(typeof(CRD));
            _logger.Information("Checking kubernetes api versions.");
            var versions = await _client.GetAPIVersionsWithHttpMessagesAsync(cancellationToken: token);
            if (versions.Body.Versions.Any(a => a.Contains("v1")))
            {
                _logger.Information("Available api versions {@versions}", versions.Body.Versions);

                try
                {
                   
                    
                    
                    var customObjects = await _client.ListNamespacedCustomObjectWithHttpMessagesAsync(
                        group: cr.ApiGroup(),
                        version: cr.ApiGroupVersion(),
                        namespaceParameter: ns,
                        plural: cr.GetKubernetesTypeMetadata().PluralName,
                        watch: true,
                        timeoutSeconds:20,
                        cancellationToken: token
                    );
                    using (customObjects.Watch<CRD, object>((type, item) =>
                            {
                                _logger.Debug("Received resource. Watch Type: {@type} Item: {@item}", type, item);
                                try
                                {
                                    handler.Invoke(type, item, _client);
                                    _logger.Debug("Invocation completed.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex, "Received resource could not be processed.");
                                }
                            },
                            exception => { _logger.Error(exception, "CRD Watch error"); },
                            () => { _logger.Warning("Kubernetes api closed  the connection"); }
                        )
                    )
                    {
                        while (!token.IsCancellationRequested)
                        {
                            await Task.Delay(3000, token);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e,"Listing crds error.");
                }
            }
            else
            {
                _logger.Error(
                    "There is no compatible kubernetes api versions. Please check your kubernetes connection configuration.");
            }
        }
    }
}