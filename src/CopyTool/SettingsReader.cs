using System.IO.Abstractions;
using System.Text.Json;

namespace CopyTool
{
    public class SettingsReader : ISettingsReader
    {
        private readonly IFileSystem _fileSystem;
        
        public string? FilePath { get; set; }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SettingsReader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public T? Load<T>() where T : class, new() => Load(typeof(T)) as T;

        private object? Load(Type type)
        {
            if (!_fileSystem.File.Exists(FilePath))
                return Activator.CreateInstance(type);

            var jsonFile = _fileSystem.File.ReadAllText(FilePath);

            return JsonSerializer.Deserialize(jsonFile, type, _jsonOptions);
        }
    }
}
