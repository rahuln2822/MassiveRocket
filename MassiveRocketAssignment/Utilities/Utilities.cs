using Azure.Data.Tables;
using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Utilities
{
    public static class Utilities
    {
        public static ClientEntity ToClientEntity(this string csvLine, string partitionKey)
        {            
            var clientEntity = new ClientEntity();
            try
            {
                string[] values = csvLine.Split(',');

                if (values.Length == 4)
                {
                    //clientEntity.Id = Guid.NewGuid();
                    //clientEntity.PartitionKey = partitionKey;
                    clientEntity.FirstName = values[0];
                    clientEntity.LastName = values[1];
                    clientEntity.Email = values[2];
                    clientEntity.ContactNumber = values[3];

                }
                else
                {
                    throw new ArgumentException($"Insufficient data - {csvLine}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return clientEntity;
        }
    }
}
