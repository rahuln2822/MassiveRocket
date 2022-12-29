using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.Utilities;
using MassiveRocketAssignment.Validation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Processors
{
    public class BatchProcessor<T> : IBatchProcessor<T>
    {
        private readonly int BatchSize;
        public BatchProcessor(IConfiguration configuration) 
        {
            BatchSize = configuration.GetValue<int>("BatchSize");
        } 

        public IEnumerable<IEnumerable<T>> CreateBatches(IEnumerable<T> entities)
        {
            entities.ShouldNotBeNull();

            int batchCount = 0;

            while (batchCount < entities.Count())
            {
                var clientEntityResult = this.GetClientInfoBatch(entities, batchCount);

                batchCount = batchCount + BatchSize;

                yield return clientEntityResult;
            }
        }

        private IEnumerable<T> GetClientInfoBatch(IEnumerable<T> entities, int batchCount)
        {
            var result = entities.Skip(batchCount).Take(BatchSize).Select(batch => batch);

            return result;
        }
    }
}
