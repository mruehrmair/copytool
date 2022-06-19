using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Text.Json;

namespace CopyTool
{
    public class SettingsReader : ISettingsReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<SettingsReader> _logger;
                
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SettingsReader(IFileSystem fileSystem, ILogger<SettingsReader> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public T? Load<T>(string filePath) where T : class, new() => Load(typeof(T), filePath) as T;

        private object? Load(Type type, string filePath)
        {
            if (!_fileSystem.File.Exists(filePath))
                return Activator.CreateInstance(type);

            var jsonFile = _fileSystem.File.ReadAllText(filePath);
            _logger.LogInformation("Using settings file {jsonFile}", filePath);

            return JsonSerializer.Deserialize(jsonFile, type, _jsonOptions);
        }
    }
}
