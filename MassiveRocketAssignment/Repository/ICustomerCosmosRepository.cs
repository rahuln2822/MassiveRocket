using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Storage
{
    public interface ICustomerCosmosRepository
    {
        Task CreateDatabaseAsync();

        Task CreateContainerAsync();

        Task InsertBulkAsync(IEnumerable<ClientEntity> clientEntities);

        Task<IEnumerable<ClientEntity>> GetClientByFirstName(string firstName);

        Task<int> GetClientsCount();
    }
}
