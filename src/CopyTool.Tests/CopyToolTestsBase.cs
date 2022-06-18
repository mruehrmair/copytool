using System.IO.Abstractions.TestingHelpers;

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
    }
}
