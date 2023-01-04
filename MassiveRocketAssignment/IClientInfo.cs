using MassiveRocketAssignment.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment
{
    public interface IClientInfo
    {
        Task<IEnumerable<ClientEntity>> GetClient(string firstName, int pageSize, int skipRecords);
        Task AddClientsByCsv(IEnumerable<string> csvContent);
        Task<int> GetClientsCount();
        Task<IEnumerable<ClientEntity>> GetAllClient(int pageSize, int skipRecords);
    }
}
