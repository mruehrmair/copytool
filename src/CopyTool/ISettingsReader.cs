namespace CopyTool
{
    public interface ISettingsReader
    {
        public string? FilePath { get; set; }
        public T? Load<T>() where T : class, new();
    }
}
