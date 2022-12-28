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
        private readonly IBatchProcessor _batchProcessor;
        public CsvReader(IBatchProcessor batchProcessor)
        {
            _batchProcessor = batchProcessor;
        }

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

        public Dictionary<string, IEnumerable<ClientEntity>> ReadAsEntityBatches(string filepath)
        {
            var csvContent = Read(filepath).Distinct();

            var csvBatches = _batchProcessor.CreateBatches(csvContent);

            return csvBatches;
        }

        //public Dictionary<string,IEnumerable<ClientEntity>> ReadEntityInBatches(string filepath)
        //{
        //    var csvContent = Read(filepath).Distinct();

        //    var clientEntities = csvContent.Select(cc => cc.ToClientEntity());

        //    return _batchProcessor.CreateBatches(clientEntities);
        //}
    }
}
