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
}
