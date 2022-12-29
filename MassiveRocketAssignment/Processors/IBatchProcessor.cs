using MassiveRocketAssignment.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Processors
{
    public interface IBatchProcessor<T>
    {
        IEnumerable<IEnumerable<T>> CreateBatches(IEnumerable<T> entities);
    }
}
