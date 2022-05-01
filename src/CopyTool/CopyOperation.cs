using System.IO.Abstractions;

namespace CopyTool;
public class CopyOperation : ICopyOperation
{
    private readonly IFileSystem _fileSystem;

    // <summary>Create MyComponent with the given fileSystem implementation</summary>
    public CopyOperation(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>Create MyComponent</summary>
    public CopyOperation() : this(
        fileSystem: new FileSystem() //use default implementation which calls System.IO
    )
    {
    }


    public async Task<bool> FileCopy(string source, string destination)
    {
        await Task.Run(() => _fileSystem.File.Copy(source, destination, true));
        return true;       
    }

    public async Task<bool> FolderCopy(string source, string destination)
    {
        await CopyFilesRecursively(source, destination);
        return true;
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
