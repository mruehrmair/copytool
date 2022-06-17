using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace CopyTool;
public class CopyOperation : ICopyOperation
{
    private readonly IFileSystem _fileSystem;
    private readonly ISettingsReader _settingsReader;
    //private readonly ILogger<CopyOperation>? _logger;

    public CopyOperation(IFileSystem fileSystem, ISettingsReader settingsReader)
    {
        _fileSystem = fileSystem;
        _settingsReader = settingsReader;
    }
        
    public async Task<bool> FileCopy(string source, string destination)
    {
        await Task.Run(() => _fileSystem.File.Copy(source, destination, true));
        //_logger?.LogInformation("Copied {source} to {destination}", source, destination);
        return true;       
    }

    public async Task<bool> FolderCopy(CopyFolder copyFolder)
    {
        await CopyFilesRecursively(copyFolder.Source, copyFolder.Destination);
        return true;
    }

    public async Task<bool> FolderCopy(CopyFolders? copyFolders)
    {
        if (copyFolders is null)
            throw new ArgumentNullException(nameof(copyFolders));

        if (copyFolders.Folders is not null)
        {
            foreach (CopyFolder folderPair in copyFolders.Folders)
            {
                await FolderCopy(folderPair);
            }
            return true;
        }           
        return false;
    }

    public async Task<bool> FolderCopy(string jsonFilePath)
    {      
        if (_settingsReader is not null)
        {
            _settingsReader.FilePath = jsonFilePath;
            CopyFolders? settings = _settingsReader.Load<CopyFolders>();

           return await FolderCopy(settings);
        }
        return false;
    }

    private async Task CopyFilesRecursively(string sourcePath, string targetPath)
    {
        _ = _fileSystem.Directory.CreateDirectory(targetPath);
        //Now Create all of the directories
        foreach (string dirPath in _fileSystem.Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            string? newDir = dirPath.Replace(sourcePath, targetPath);
            _ = _fileSystem.Directory.CreateDirectory(newDir);
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in _fileSystem.Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            string? src = newPath;
            string? dest = newPath.Replace(sourcePath, targetPath);
            await FileCopy(src, dest);
        }
    }
}
