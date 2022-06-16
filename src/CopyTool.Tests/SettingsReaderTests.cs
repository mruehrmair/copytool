using FluentAssertions;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CopyTool.Tests;

public class SettingsReaderTests
{
    private readonly SettingsReader _sut;
    private readonly MockFileSystem _fileSystem;
    private const string _file1 = @"c:\testfolder\settings.json";
    private const string _folder1 = @"c:\testfolder";

    private const string _jsonConfig = @"{
	""folders"": [
      { ""source"":""c:\\folder1src"", ""destination"": ""c:\\folder1dest"" },
      { ""source"":""c:\\folder2src"", ""destination"": ""c:\\folder2dest"" },
      { ""source"":""c:\\folder3src"", ""destination"": ""c:\\folder3dest"" }
    ]
    }";

    private const string _jsonConfigNoFoldersToCopy = @"{
	""folders"": []
    }";
        

    public SettingsReaderTests()
    {
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{_folder1}", new MockDirectoryData() },
            { @$"{_file1}", new MockFileData(_jsonConfig) }
           
        });
        _sut = new SettingsReader(_fileSystem, _file1);
    }

    [Fact]
    public void Load_SampleFile_DeserializationOk()
    {
        //given
        //when
        CopyFolders? settings = _sut.Load<CopyFolders>();

        //then
        settings.Should().NotBeNull();
        settings?.Folders.Should().NotBeNull();        
        settings?.Folders?.Count.Should().Be(3);

    }    
}