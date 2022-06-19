namespace CopyTool
{
    public interface ISettingsReader
    {       
        public T? Load<T>(string filePath) where T : class, new();
    }
}
