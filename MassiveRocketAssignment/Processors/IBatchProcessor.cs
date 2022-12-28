using MassiveRocketAssignment.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Processors
{
    public interface IBatchProcessor
    {
        Dictionary<string,IEnumerable<ClientEntity>> CreateBatches(IEnumerable<string> entities);
    }
}
