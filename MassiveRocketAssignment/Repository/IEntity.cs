using Azure;
using Newtonsoft.Json;

namespace MassiveRocketAssignment.Storage
{
    public interface IEntity
    {
        public Guid Id { get; set; }
        public string PartitionKey { get; set; }
    }
}