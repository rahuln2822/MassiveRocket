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
        Task<IEnumerable<ClientEntity>> GetClient(string firstName);
        Task AddClientsByCsv(string filePath);
        Task<int> GetClientsCount();
    }
}
