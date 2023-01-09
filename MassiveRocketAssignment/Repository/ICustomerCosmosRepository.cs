namespace MassiveRocketAssignment.Storage
{
    public interface ICustomerCosmosRepository
    {
        Task CreateDatabaseAsync();

        Task CreateContainerAsync();

        Task InsertBulkAsync(IEnumerable<ClientEntity> clientEntities);

        Task<IEnumerable<ClientEntity>> GetClientByFirstName(string firstName, int pageSize, int skipRecords);

        Task<int> GetClientsCount(string? firstName = null);

        Task<IEnumerable<ClientEntity>> GetAllClient(int pageSize, int skipRecords);
    }
}
