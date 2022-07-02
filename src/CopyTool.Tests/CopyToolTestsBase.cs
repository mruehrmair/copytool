using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

namespace CopyTool.Tests
{
    public abstract class CopyToolTestsBase
    {
        internal const string _jsonConfig = @"{
	""folders"": [
      { ""source"":""c:\\folder1src"", ""destination"": ""c:\\folder1dest"" },
      { ""source"":""c:\\folder2src"", ""destination"": ""c:\\folder2dest"" },
      { ""source"":""c:\\folder3src"", ""destination"": ""c:\\folder3dest"" }
    ]
    }";

        internal const string _jsonConfigNoFoldersToCopy = @"{
	""folders"": []
    }";

    protected CopyFolders? UpdatePathsToNonWindowsCompatibility(CopyFolders? foldersToCopy = null)
    {
        if (foldersToCopy?.Folders is not null)
        {
            var updatedFolders = new List<CopyFolder>();
            foreach (var folder in foldersToCopy.Folders)
            {
                updatedFolders.Add(new CopyFolder(XFS.Path(folder.Source), XFS.Path(folder.Destination)));
            }
            foldersToCopy.Folders = updatedFolders;
        }
        return foldersToCopy;
    }

    }
}
