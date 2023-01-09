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
