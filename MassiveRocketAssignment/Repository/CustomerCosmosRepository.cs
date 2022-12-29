using MassiveRocketAssignment.Utilities;
using MassiveRocketAssignment.Validation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using Container = Microsoft.Azure.Cosmos.Container;

namespace MassiveRocketAssignment.Storage
{
    public class CustomerCosmosRepository : ICustomerCosmosRepository
    {
        private static string? EndpointUri;
        private static string? PrimaryKey;
        private static int MaxRULimit;
        private static CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;
        private string databaseId = "ToDoList";
        private string containerId = "Items";

        public CustomerCosmosRepository(IConfiguration configuration)
        {
            EndpointUri = configuration.GetValue<string>("EndpointUri");
            PrimaryKey = configuration.GetValue<string>("PrimaryKey");
            MaxRULimit = configuration.GetValue<int>("MaxRULimit");

            EndpointUri.ShouldNotBeNull();
            PrimaryKey.ShouldNotBeNull();

            var cosomsClientOptions = new CosmosClientOptions()
            {
                ApplicationName = "MassiveRocketAssignement",
                AllowBulkExecution = true
            };

            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, cosomsClientOptions);

            CreateDatabaseAsync().Wait();
            CreateContainerAsync().Wait();
        }

        public async Task CreateDatabaseAsync()
        {
            _cosmosClient.ShouldNotBeNull();

            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

            _database = databaseResponse.Database;
        }

        public async Task CreateContainerAsync()
        {
            _database.ShouldNotBeNull();

            ContainerProperties containerProperties = new ContainerProperties(containerId, "/partitionKey");
            ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(MaxRULimit);

            _container = await _database.CreateContainerIfNotExistsAsync(containerProperties, autoscaleThroughputProperties);
        }

        public async Task InsertBulkAsync(IEnumerable<ClientEntity> clientEntities)
        {
            int errorCount = 0;

            StringBuilder stringBuilder = new StringBuilder();

            Task CreateTask(ClientEntity clientEntity)
            {
                return RetryManager.WaitAndRetryPolicy
                                   .ExecuteAsync(() => _container.CreateItemAsync(clientEntity, new PartitionKey(clientEntity.PartitionKey))
                                   .ContinueWith(itemResponse =>
                                    {
                                        if (!itemResponse.IsCompletedSuccessfully)
                                        {
                                            AggregateException innerExceptions = itemResponse.Exception.Flatten();
                                            if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                                            {
                                                errorCount++;
                                                stringBuilder.AppendLine($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                                                //File.AppendAllText("D:\\Log.txt", $"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                                                //Console.WriteLine($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                                            }
                                            else
                                            {
                                                errorCount++;
                                                stringBuilder.AppendLine($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                                                //File.AppendAllText("D:\\Log.txt", $"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                                                //Console.WriteLine($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                                            }
                                        }
                                    }));

            }

            Console.WriteLine($"Start Processing batch : {DateTime.Now}");

            var result = clientEntities.Select(ce => CreateTask(ce));
            await Task.WhenAll(result);

            Console.WriteLine($"End Processing batch : {DateTime.Now}");
            Console.WriteLine($"Error Count - {errorCount}");
            File.AppendAllText("D:\\Log.txt", stringBuilder.ToString());
        }

        public async Task<IEnumerable<ClientEntity>> GetClientByFirstName(string firstName)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.FirstName = '{firstName}'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryIterator<ClientEntity>(queryDefinition);
            var list = new List<ClientEntity>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var clientResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var client in clientResultSet)
                {
                    list.Add(client);
                }
            }
            return list;
        }

        public async Task<IEnumerable<ClientEntity>> GetAllClient(string firstName)
        {
            var sqlQueryText = $"SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryIterator<ClientEntity>(queryDefinition);
            var list = new List<ClientEntity>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var clientResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var client in clientResultSet)
                {
                    list.Add(client);
                }
            }
            return list;
        }

        public async Task<int> GetClientsCount()
        {
            var sqlQueryText = $"SELECT Value Count(1) FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryStreamIterator(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                using (ResponseMessage response = await queryResultSetIterator.ReadNextAsync())
                {
                    using (StreamReader sr = new StreamReader(response.Content))
                    {
                        var result = sr.ReadToEnd();
                        var jsObject = JObject.Parse(result);
                        return Convert.ToInt32(((JValue)jsObject.GetValue("Documents")[0]).Value);
                    }

                }
            }
            return 0;
        }
    }
}
