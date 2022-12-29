using MassiveRocketAssignment.Processors;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.Utilities;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment
{
    public class ClientInfo : IClientInfo
    {
        private IReader _reader;
        private readonly ICustomerCosmosRepository _customerCosmosRepository;
        private readonly IBatchProcessor<string> _batchProcessor;

        public ClientInfo(IReader reader, ICustomerCosmosRepository customerCosmosRepository, IBatchProcessor<string> batchProcessor)
        {
            _reader = reader;
            _customerCosmosRepository = customerCosmosRepository;
            _batchProcessor = batchProcessor;
        }

        public async Task AddClientsByCsv(string filePath)
        {
            var results = _reader.Read(filePath);

            var batches = _batchProcessor.CreateBatches(results);

            string customerIdentity = "Customer1";

            foreach (var csvBatch in batches)
            {
                customerIdentity = $"{customerIdentity}-{Guid.NewGuid()}";

                var clientBatch = csvBatch.Select(csv => csv.ToClientEntity(customerIdentity));

                await _customerCosmosRepository.InsertBulkAsync(clientBatch);
            }
        }

        public async Task<IEnumerable<ClientEntity>> GetClient(string firstName)
        {
            var result = await _customerCosmosRepository.GetClientByFirstName(firstName);

            return result;
        }

        public async Task<int> GetClientsCount()
        {
            var result = await _customerCosmosRepository.GetClientsCount();

            return result;
        }
    }
}
