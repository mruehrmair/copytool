using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Text.Json;

namespace CopyTool
{
    public class SettingsReader : ISettingsReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<SettingsReader> _logger;

        public string? FilePath { get; set; }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SettingsReader(IFileSystem fileSystem, ILogger<SettingsReader> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public T? Load<T>() where T : class, new() => Load(typeof(T)) as T;

        private object? Load(Type type)
        {
            if (!_fileSystem.File.Exists(FilePath))
                return Activator.CreateInstance(type);

            var jsonFile = _fileSystem.File.ReadAllText(FilePath);
            _logger.LogInformation("Using settings file {jsonFile}", FilePath);

            return JsonSerializer.Deserialize(jsonFile, type, _jsonOptions);
        }
    }
}
