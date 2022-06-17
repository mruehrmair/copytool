namespace CopyTool;
public interface ICopyOperation
{
    public Task<bool> FileCopy(string source, string destination);
    public Task<bool> FolderCopy(CopyFolder copyFolder);
    public Task<bool> FolderCopy(CopyFolders? copyFolders);
    public Task<bool> FolderCopy(string jsonFilePath);

}