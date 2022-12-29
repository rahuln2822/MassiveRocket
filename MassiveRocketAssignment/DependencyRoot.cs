using MassiveRocketAssignment.Processors;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment
{
    public static class DependencyRoot
    {
        public static void RegisterDependency(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ICustomerCosmosRepository, CustomerCosmosRepository>();
            serviceCollection.AddSingleton<IReader, CsvReader>();
            serviceCollection.AddSingleton<IClientInfo, ClientInfo>();
            serviceCollection.AddSingleton<IBatchProcessor<string>, BatchProcessor<string>>();
        }

        public static IHost CreateHost(Action<HostBuilderContext, IServiceCollection> serviceHostBuilder) 
        {
            var serviceHost = new HostBuilder()
                                .ConfigureAppConfiguration((config) => config.AddJsonFile("appsettings.json"))
                                .ConfigureServices(serviceHostBuilder)
                                .Build();  

            return serviceHost;
        }
    }
}
