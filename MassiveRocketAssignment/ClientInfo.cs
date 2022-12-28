using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Storage;
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

        public ClientInfo(IReader reader, ICustomerCosmosRepository customerCosmosRepository)
        {
            _reader = reader;
            _customerCosmosRepository = customerCosmosRepository;
        }

        public async Task AddClientsByCsv(string filePath)
        {
            var results = _reader.ReadAsEntityBatches(filePath);

            await _customerCosmosRepository.InsertBulkAsync(results);
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
