using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.AzureFunction.Consumers;

namespace Sample.AzureFunction
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<SubmitOrderFunctions>();
                    services.AddScoped<AuditOrderFunctions>();

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<SubmitOrderConsumer>();
                        x.AddConsumer<AuditOrderConsumer>();

                        x.UsingAzureServiceBus((ctx, cfg) =>
                        {
                            cfg.Host(hostContext.Configuration["ServiceBusConnection"]);
                            cfg.Publish<SubmitOrder>(p =>
                            {
                                p.CreateTopicOptions.Name = "orders";
                                p.CreateTopicOptions.EnablePartitioning = true;
                            });
                            cfg.SubscriptionEndpoint<SubmitOrder>("submit-order", s =>
                            {
                                s.ConfigureConsumer<SubmitOrderConsumer>(ctx);
                            });
                            cfg.ConfigureEndpoints(ctx);
                        });
                        x.AddRider(r =>
                        {
                            r.UsingEventHub((context, cfg) =>
                            {
                                cfg.Host(hostContext.Configuration["EventHubConnection"]);
                                cfg.UseRawJsonSerializer();
                            });
                        });

                        x.AddConfigureEndpointsCallback((_, cfg) =>
                        {
                            cfg.ClearMessageDeserializers();
                            cfg.UseRawJsonSerializer();
                        });
                    });
                });
        }
    }
}