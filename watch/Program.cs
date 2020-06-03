﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Newtonsoft.Json;
using watch.KubeController;
using watch.Model.Challange;

namespace watch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var client = new Kubernetes(config);

            var cr = new ChallangeResource();
            var controller = new Controller(client);

            var cts = new CancellationTokenSource();
            Console.WriteLine($"PID: {Process.GetCurrentProcess().Id}");
            await controller.StartAsync<ChallangeResource>(cr,"app-gw",(type,crd,client)=>{
                Console.WriteLine($"{type} Dns: {crd.Spec.DnsName} Key: {crd.Spec.Key}");
            },cts);



            var ctrlc = new ManualResetEventSlim(false);
            Console.CancelKeyPress += (sender, eventArgs) => ctrlc.Set();
            ctrlc.Wait();

         
        }
    }
}
