using MassiveRocketAssignment.Processors;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.Utilities;
using Microsoft.Extensions.Logging;

namespace MassiveRocketAssignment
{
    public class ClientInfo : IClientInfo
    {
        private IReader _reader;
        private readonly ICustomerCosmosRepository _customerCosmosRepository;
        private readonly IBatchProcessor<string> _batchProcessor;
        private readonly ILogger<ClientInfo> _logger;

        public ClientInfo(IReader reader, ICustomerCosmosRepository customerCosmosRepository, IBatchProcessor<string> batchProcessor, ILogger<ClientInfo> logger)
        {
            _reader = reader;
            _customerCosmosRepository = customerCosmosRepository;
            _batchProcessor = batchProcessor;
            _logger = logger;
        }

        public async Task AddClientsByCsv(IEnumerable<string> csvContent)
        {
            var batches = _batchProcessor.CreateBatches(csvContent);

            foreach (var csvBatch in batches)
            {
                try
                {
                    var clientBatch = ConvertBatchToClientEntity(csvBatch); 

                    await _customerCosmosRepository.InsertBulkAsync(clientBatch);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing a batch - {ex.Message} : {ex.StackTrace}");
                }               
            }
        }

        public async Task<IEnumerable<ClientEntity>> GetClient(string firstName, int pageSize, int skipRecords)
        {
            var result = await _customerCosmosRepository.GetClientByFirstName(firstName, pageSize, skipRecords);

            return result;
        }

        public async Task<IEnumerable<ClientEntity>> GetAllClient(int pageSize, int skipRecords)
        {
            var result = await _customerCosmosRepository.GetAllClient(pageSize, skipRecords);

            return result;
        }

        public async Task<int> GetClientsCount()
        {
            var result = await _customerCosmosRepository.GetClientsCount();

            return result;
        }

        private IEnumerable<ClientEntity> ConvertBatchToClientEntity(IEnumerable<string> csvBatch)
        {
            string customerIdentity = $"{Constants.CustomerName}";

            var result = csvBatch.Select(csv => ToClientEntity(csv, customerIdentity));

            return result.Where(entity => entity != null);
        }

        private ClientEntity ToClientEntity(string csvLine, string partitionKey)
        {
            var clientEntity = new ClientEntity();
            try
            {
                string[] values = csvLine.Split(',');

                if (values.Length == 4)
                {
                    clientEntity.Id = $"{partitionKey}-{values[0]}-{values[3]}";
                    clientEntity.PartitionKey = partitionKey;
                    clientEntity.FirstName = values[0];
                    clientEntity.LastName = values[1];
                    clientEntity.Email = values[2];
                    clientEntity.ContactNumber = values[3];
                    //clientEntity.Timestamp = DateTime.Now;

                }
                else
                {
                    throw new ArgumentException($"Incorrect data - {csvLine}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error converting to ClientEntity - {ex.Message} : {ex.StackTrace}");
                return null;
            }

            return clientEntity;
        }
    }
}
