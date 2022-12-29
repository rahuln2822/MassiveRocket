using Azure.Data.Tables;
using MassiveRocketAssignment.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Readers
{
    public interface IReader
    {
        IEnumerable<string> Read(string filepath);
    }
}
