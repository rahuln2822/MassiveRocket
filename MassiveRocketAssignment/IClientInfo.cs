using MassiveRocketAssignment.Storage;

namespace MassiveRocketAssignment
{
    public interface IClientInfo
    {
        public int TotalRecordCount { get; set; }
        Task<IEnumerable<ClientEntity>> GetClient(string firstName, int pageSize, int skipRecords);
        Task AddClients(IEnumerable<ClientEntity> clientEntities);
        Task<int> GetClientsCount(string? firstName = null);
        Task<IEnumerable<ClientEntity>> GetAllClient(int pageSize, int skipRecords);
    }
}
