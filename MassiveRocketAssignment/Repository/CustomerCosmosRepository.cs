using MassiveRocketAssignment.Utilities;
using MassiveRocketAssignment.Validation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text;
using Container = Microsoft.Azure.Cosmos.Container;

namespace MassiveRocketAssignment.Storage
{
    public class CustomerCosmosRepository : ICustomerCosmosRepository
    {
        private static string? EndpointUri;
        private static string? PrimaryKey;
        private static int MaxRULimit;
        private static bool AllowBulkInsert;
        private static CosmosClient? _cosmosClient;
        private Database? _database;
        private Container? _container;
        private string databaseId = Constants.DatabaseName;
        private string containerId = Constants.CustomerName;
        private readonly ILogger<CustomerCosmosRepository> _logger;

        public CustomerCosmosRepository(IConfiguration configuration, ILogger<CustomerCosmosRepository> logger)
        {
            EndpointUri = configuration.GetValue<string>("EndpointUri");
            PrimaryKey = configuration.GetValue<string>("PrimaryKey");
            MaxRULimit = configuration.GetValue<int?>("MaxRULimit") ?? 2000;
            AllowBulkInsert = configuration.GetValue<bool?>("AllowBulkInsert") ?? false;

            EndpointUri.ShouldNotBeNull();
            PrimaryKey.ShouldNotBeNull();

            var cosomsClientOptions = new CosmosClientOptions()
            {
                ApplicationName = Constants.ApplicationName,
                AllowBulkExecution = AllowBulkInsert
            };

            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, cosomsClientOptions);
            _logger = logger;

            CreateDatabaseAsync().Wait();
            CreateContainerAsync().Wait();
        }

        public async Task CreateDatabaseAsync()
        {
            _cosmosClient.ShouldNotBeNull();

            var databaseResponse = await _cosmosClient?.CreateDatabaseIfNotExistsAsync(databaseId);

            _database = databaseResponse?.Database;
        }

        public async Task CreateContainerAsync()
        {
            _database.ShouldNotBeNull();

            ContainerProperties containerProperties = new ContainerProperties(containerId, "/partitionKey");
            ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(MaxRULimit);

            _container = await _database?.CreateContainerIfNotExistsAsync(containerProperties, autoscaleThroughputProperties);
        }

        public async Task InsertBulkAsync(IEnumerable<ClientEntity> clientEntities)
        {
            int errorCount = 0;

            StringBuilder stringBuilder = new StringBuilder();

            Task CreateTask(ClientEntity clientEntity)
            {
                return RetryManager.WaitAndRetryPolicy
                                   .ExecuteAsync(() => _container?.UpsertItemAsync(clientEntity, new PartitionKey(clientEntity.PartitionKey))
                                   .ContinueWith(itemResponse =>
                                    {
                                        if (!itemResponse.IsCompletedSuccessfully)
                                        {
                                            AggregateException? innerExceptions = itemResponse?.Exception?.Flatten();
                                            if (innerExceptions != null)
                                            {
                                                if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                                                {
                                                    errorCount++;
                                                    stringBuilder.AppendLine($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                                                    _logger.LogError($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                                                }
                                                else
                                                {
                                                    errorCount++;
                                                    stringBuilder.AppendLine($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                                                    _logger.LogError($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                                                }
                                            }
                                            else
                                            {
                                                _logger.LogError("Exception - Something unexpected has happened.");
                                            }
                                        }
                                    }));

            }

            Console.WriteLine($"Start Processing batch : {DateTime.Now}");

            var result = clientEntities.Select(ce => CreateTask(ce));
            await Task.WhenAll(result).ConfigureAwait(false);

            Console.WriteLine($"End Processing batch : {DateTime.Now}");
            Console.WriteLine($"Error Count - {errorCount}");
            File.AppendAllText("D:\\Log.txt", stringBuilder.ToString());
        }

        public async Task<IEnumerable<ClientEntity>> GetClientByFirstName(string firstName, int pageSize, int skipRecords)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE Contains(c.FirstName,'{firstName}', true) OFFSET {skipRecords} LIMIT {pageSize}";
            List<ClientEntity> list = await GetEntities<ClientEntity>(sqlQueryText);
            return list;
        }

        public async Task<IEnumerable<ClientEntity>> GetAllClient(int pageSize, int skipRecords)
        {
            var sqlQueryText = $"SELECT * FROM c OFFSET {skipRecords} LIMIT {pageSize}";
            List<ClientEntity> list = await GetEntities<ClientEntity>(sqlQueryText);
            return list;
        }

        public async Task<int> GetClientsCount(string? firstName = null)
        {
            string sqlQueryText;
            if (firstName == null)
            {
                sqlQueryText = $"SELECT Value Count(1) FROM c";
            }
            else
            {
                sqlQueryText = $"SELECT Value Count(1) FROM c WHERE Contains(c.FirstName,'{firstName}', true)";
            }

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container?.GetItemQueryStreamIterator(queryDefinition);

            if (queryResultSetIterator != null)
            {
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
            }
            return 0;
        }

        private async Task<List<T>> GetEntities<T>(string sqlQueryText)
        {
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container?.GetItemQueryIterator<T>(queryDefinition);
            var list = new List<T>();
            if (queryResultSetIterator != null)
            {
                while (queryResultSetIterator.HasMoreResults)
                {
                    var clientResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (var client in clientResultSet)
                    {
                        list.Add(client);
                    }
                }
            }

            return list;
        }
    }
}
