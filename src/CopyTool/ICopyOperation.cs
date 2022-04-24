namespace CopyTool;
public interface ICopyOperation
{
    public Task<bool> FileCopy(string source, string destination);
}