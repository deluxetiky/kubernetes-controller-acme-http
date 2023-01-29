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
        /// <typeparam name="T">Customresource Definition Object which inherits from V1CustomResourceDefinition</typeparam>
        /// <returns></returns>
        public async Task StartAsync<T>(String ns, Func<WatchEventType, T, IKubernetes,Task> handler,
            CancellationToken token, T cr = null) where T : V1CustomResourceDefinition
        {
            if (token.IsCancellationRequested) return;

            if (cr == null)
                cr = (T) Activator.CreateInstance(typeof(T));
            _logger.Information("Checking kubernetes api versions.");
            
            var versions = await _client.CoreV1.GetAPIResourcesWithHttpMessagesAsync(cancellationToken:token);
            if (versions.Body.GroupVersion.Contains("v1"))
            {
                _logger.Information("Available api versions {@versions}", versions.Body.GroupVersion);

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var ctrlc = new ManualResetEventSlim(false);
                        var customObjects = await _client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
                            group: cr.ApiGroup(),
                            version: cr.ApiGroupVersion(),
                            namespaceParameter: ns,
                            plural: cr.GetKubernetesTypeMetadata().PluralName,
                            watch: true,
                            cancellationToken: token
                        );
                    
                        using (customObjects.Watch<T, object>( (type, item) =>
                                {
                                    _logger.Debug("Received resource. Watch Type: {@type} Item: {@item}", type, item);
                                    try
                                    {
                                        handler.Invoke(type, item, _client).GetAwaiter().GetResult();
                                        _logger.Debug("Crd registered to get activated.");
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex, "Received resource could not be processed.");
                                    }
                                },
                                exception => { _logger.Error(exception, "CRD Watch error"); },
                                () =>
                                {
                                    _logger.Warning("Kubernetes api closed  the connection");
                                    ctrlc.Set();
                                }
                            )
                        )
                        {
                           
                            Console.CancelKeyPress += (sender, eventArgs) => ctrlc.Set();
                            ctrlc.Wait(token);
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