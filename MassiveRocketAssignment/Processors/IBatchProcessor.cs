namespace MassiveRocketAssignment.Processors
{
    public interface IBatchProcessor<T>
    {
        IEnumerable<IEnumerable<T>> CreateBatches(IEnumerable<T> entities);
    }
}
