using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using acme_resolver.Services;
using k8s;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace acme_resolver
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
		   .SetBasePath(Directory.GetCurrentDirectory())
		   .AddJsonFile("appsettings.production.json", optional: false, reloadOnChange: true)
		   .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production"}.json", optional: true)
		   .AddEnvironmentVariables()
		   .Build();
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(Log.Logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })                
                .ConfigureServices(services =>
                {
                    KubernetesClientConfiguration kubeConfig=null;
                    if( Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")=="production")
                        kubeConfig = KubernetesClientConfiguration.InClusterConfig();
                    else
                        kubeConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(Environment.GetEnvironmentVariable("KUBECONFIG"));

                    services.AddSingleton(kubeConfig);
                    // Setup the http client
                    services.AddHttpClient("K8s")
                        .AddTypedClient<IKubernetes>((httpClient, serviceProvider) => new Kubernetes(kubeConfig));

                    // Controller Watching service
                    services.AddHostedService<AcmeHttpChallengeService>();
                });
    }
}
