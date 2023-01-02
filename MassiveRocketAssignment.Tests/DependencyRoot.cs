using MassiveRocketAssignment.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Tests
{
    public static class DependencyRoot
    {
        public static IHost BuildAndRunHost()
        {
            var host = new HostBuilder()
                            .ConfigureAppConfiguration((config) => config.AddJsonFile("appsettings.test.json"))
                            .ConfigureServices((context, serviceCollection) => serviceCollection.AddSingleton<IBatchProcessor<string>, BatchProcessor<string>>())
                            .Start();

            return host;
        }

        
    }
}
