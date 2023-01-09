using MassiveRocketAssignment.Storage;
using Microsoft.Extensions.Logging;

namespace MassiveRocketAssignment
{
    public class ClientInfo : IClientInfo
    {
        private readonly ICustomerCosmosRepository _customerCosmosRepository;
        private readonly ILogger<ClientInfo> _logger;

        public int TotalRecordCount { get; set; }

        public ClientInfo(ICustomerCosmosRepository customerCosmosRepository, ILogger<ClientInfo> logger)
        {
            _customerCosmosRepository = customerCosmosRepository;
            _logger = logger;
        }

        public async Task AddClients(IEnumerable<ClientEntity> clientEntities)
        {
            try
            {
                await _customerCosmosRepository.InsertBulkAsync(clientEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing a batch - {ex.Message} : {ex.StackTrace}");
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

        public async Task<int> GetClientsCount(string? firstName = null)
        {
            var result = await _customerCosmosRepository.GetClientsCount(firstName);

            return result;
        }
    }
}
