using System.Threading.Tasks;
using System.Threading;
using System;
using k8s;
using k8s.Models;

namespace watch.KubeController
{
    public class Controller
    {
        private readonly Kubernetes _client;
        public Controller(Kubernetes client)
        {
            _client = client;
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
        public async Task StartAsync<CRD>(CRD cr, String ns, Action<WatchEventType, CRD, Kubernetes> handler, CancellationTokenSource token) where CRD : V1CustomResourceDefinition
        {
            if (token.IsCancellationRequested) return;

            var customObjects = await _client.ListNamespacedCustomObjectWithHttpMessagesAsync(
               group: cr.ApiGroup(),
               version: cr.ApiGroupVersion(),
               namespaceParameter: ns,
               plural: cr.GetKubernetesTypeMetadata().PluralName,
               watch: true,
               cancellationToken: token.Token
           );
            using (customObjects.Watch<CRD, object>((type, item) =>
                    {
                        handler.Invoke(type, item, _client);
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
