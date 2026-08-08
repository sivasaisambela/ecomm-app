using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessor.Function.Services;

var host = new HostBuilder()
     .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddScoped<IOrderEventProcessor, OrderEventProcessor>();
    })
    .Build();

host.Run();