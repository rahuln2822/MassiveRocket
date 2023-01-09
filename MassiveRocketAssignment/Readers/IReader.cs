namespace MassiveRocketAssignment.Readers
{
    public interface IReader
    {
        IEnumerable<string> Read(string filepath);
    }
}
