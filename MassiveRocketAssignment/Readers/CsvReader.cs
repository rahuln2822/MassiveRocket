using Azure.Data.Tables;
using MassiveRocketAssignment.Processors;
using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Readers
{
    public class CsvReader : IReader
    {
        public IEnumerable<string> Read(string filepath)
        {
            using (var streamReader = new StreamReader(filepath))
            {
                while (!streamReader.EndOfStream)
                {
                    var row = streamReader.ReadLine();

                    if (row != null)
                    {
                        yield return row;
                    }
                }
            }
        }
    }
}
