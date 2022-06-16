namespace CopyTool
{
    public record CopyFolder
    {
        public CopyFolder(string source, string destination)
        {
            Source = source;
            Destination = destination;
        }

        public string Source { get; }
        public string Destination { get; }
    }
}
