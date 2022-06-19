using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace CopyTool;
public class CopyOperation : ICopyOperation
{
    private readonly IFileSystem _fileSystem;
    private readonly ISettingsReader _settingsReader;
    private readonly ILogger<CopyOperation> _logger;

    public CopyOperation(IFileSystem fileSystem, ISettingsReader settingsReader, ILogger<CopyOperation> logger)
    {
        _fileSystem = fileSystem;
        _settingsReader = settingsReader;
        _logger = logger;
    }

    public async Task<bool> FolderCopy(string jsonFilePath)
    {
        if (_settingsReader is not null)
        {
            if (_fileSystem.File.Exists(jsonFilePath))
            {
                CopyFolders? settings = _settingsReader.Load<CopyFolders>(jsonFilePath);
                return await FolderCopy(settings);
            }
            _logger.LogError("Settings file {file} does not exist.", jsonFilePath);
        }
        return false;
    }

    public async Task<bool> FolderCopy(string source, string destination)
    {        
        return await CopyFilesRecursively(source, destination);
    }

    private async Task<bool> FileCopy(string source, string destination)
    {
        await Task.Run(() => _fileSystem.File.Copy(source, destination, true));
        _logger.LogInformation("Copied {source} to {destination}", source, destination);
        return true;       
    }
       
    private async Task<bool> FolderCopy(CopyFolders? copyFolders)
    {
        bool isSuccessful = false;

        if (copyFolders is null)
            throw new ArgumentNullException(nameof(copyFolders));

        if (copyFolders.Folders is not null)
        {
            foreach (CopyFolder folderPair in copyFolders.Folders)
            {
                isSuccessful = await FolderCopy(folderPair);
            }           
        }
        return isSuccessful;
    }

    private async Task<bool> FolderCopy(CopyFolder copyFolder)
    {
        return await CopyFilesRecursively(copyFolder.Source, copyFolder.Destination);
    }

    private async Task<bool> CopyFilesRecursively(string sourcePath, string targetPath)
    {
        try
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
            return true;
        }
        catch (DirectoryNotFoundException ed)
        {
            _logger.LogError("Directory {source} does not exist: {e}", sourcePath, ed.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("FileCopy operation failed: {e}",  e.StackTrace);
        }
        return false;
    }
}
