// See https://aka.ms/new-console-template for more information
using Azure.Data.Tables;
using MassiveRocketAssignment;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MassiveRocket;

public class Program
{
    private static string ProjectSampleDataFolder => @$"{AppDomain.CurrentDomain.BaseDirectory}SampleData";

    public static async Task Main(string[] args)
    {
        //mr-cosmos-westus-rg
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Console.WriteLine("Start Time - " + DateTime.Now);

        var host = DependencyRoot.CreateHost(DependencyRoot.RegisterDependency);
        await host.StartAsync();

        Console.WriteLine(Directory.Exists(ProjectSampleDataFolder));

        var client = host.Services.GetService<IClientInfo>();

        if (client != null)
        {
            await client.AddClientsByCsv($@"{ProjectSampleDataFolder}\1M-Sales-Records-Sample-1.csv");
        }
        else
        { 
            throw new TypeInitializationException(typeof(IClientInfo).Name, new Exception("Type not initialized"));
        }

        var result = await client.GetClient("ABC147");
        Console.WriteLine($"ABC147 Count-{result.Count()}");
        //foreach (var item in result)
        //{
        //    Console.WriteLine($"{item.FirstName}-{item.LastName}-{item.ContactNumber}");
        //}

        var result2 = await client.GetClientsCount();

        Console.WriteLine($"Total Count-{result2}");

        stopwatch.Stop();
        Console.WriteLine("Operation Complete. Elapsed time : " + stopwatch.ElapsedMilliseconds);
        Console.WriteLine("End Time - " + DateTime.Now);
        Console.ReadLine();
    }
}