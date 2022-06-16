using System.IO.Abstractions;
using System.Text.Json;

namespace CopyTool
{
    public class SettingsReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _filePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SettingsReader(IFileSystem fileSystem, string filePath)
        {
            _fileSystem = fileSystem;
            _filePath = filePath;
        }

        public SettingsReader(string filePath) : this(
        fileSystem: new FileSystem(), filePath
        )
        {
        }

        public T? Load<T>() where T : class, new() => Load(typeof(T)) as T;

        private object? Load(Type type)
        {
            if (!_fileSystem.File.Exists(_filePath))
                return Activator.CreateInstance(type);

            var jsonFile = _fileSystem.File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize(jsonFile, type, _jsonOptions);
        }
    }
}
