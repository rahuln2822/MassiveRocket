using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.Utilities;
using MassiveRocketAssignment.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Processors
{
    public class BatchProcessor : IBatchProcessor
    {
        private static readonly int BatchSize = 20000;

        public Dictionary<string, IEnumerable<ClientEntity>> CreateBatches(IEnumerable<string> entities)
        {
            entities.ShouldNotBeNull();

            var clientBatch = new Dictionary<string, IEnumerable<ClientEntity>>();
            int batchCount = 0;
            string batchIdentifier;
            while (batchCount < entities.Count())
            {
                batchIdentifier = $"Batch-{Guid.NewGuid()}";

                var clientEntityResult = GetClientInfoBatch(entities, batchCount, batchIdentifier);

                clientBatch.Add(batchIdentifier, clientEntityResult);

                batchCount = batchCount + BatchSize;
            }

            return clientBatch;
        }

        private static IEnumerable<ClientEntity> GetClientInfoBatch(IEnumerable<string> entities, int batchCount, string partitionKey)
        {
            var result = entities.Skip(batchCount).Take(BatchSize).Select(batch => batch);

            return result.Select(csv => csv.ToClientEntity(partitionKey));
        }
    }
}
