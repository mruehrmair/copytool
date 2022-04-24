namespace CopyTool;
public interface ICopyOperation
{
    public Task<bool> FolderCopy(string source, string destination);
}