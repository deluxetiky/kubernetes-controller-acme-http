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
        /// <typeparam name="A">Customresource Definition Object which inherits from V1CustomResourceDefinition</typeparam>
        /// <returns></returns>
        public async Task StartAsync<CRD>(CRD cr, String ns, Action<WatchEventType, CRD, IKubernetes> handler, CancellationToken token) where CRD : V1CustomResourceDefinition
        {
            if (token.IsCancellationRequested) return;

            var customObjects = await _client.ListNamespacedCustomObjectWithHttpMessagesAsync(
               group: cr.ApiGroup(),
               version: cr.ApiGroupVersion(),
               namespaceParameter: ns,
               plural: cr.GetKubernetesTypeMetadata().PluralName,
               watch: true,
               cancellationToken: token
           );
            using (customObjects.Watch<CRD, object>((type, item) =>
                    {
                        _logger.Debug("Received resource. Watch Type: {@type} Item: {@item}",type,item);
                        try
                        {
                            handler.Invoke(type, item, _client);
                            _logger.Debug("Invocation completed.");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex,"Received resource could not be processed.");
                        }
                    }
                )
              )
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(1500);
                }
            }


        }
    }
}
