using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CopyTool.Tests;

public class SettingsReaderTests : CopyToolTestsBase
{
    private readonly MockFileSystem _fileSystem;

    private SettingsReader _sut;
    private readonly Mock<ILogger<SettingsReader>> _logger;
    private const string _file1 = @"c:\testfolder\settings.json";
    private const string _file2 = @"c:\testfolder\settingsEmpty.json";
    private const string _folder1 = @"c:\testfolder";

    public SettingsReaderTests() 
    {
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{_folder1}", new MockDirectoryData() },
            { @$"{_file1}", new MockFileData(_jsonConfig) },
            { @$"{_file2}", new MockFileData(_jsonConfigNoFoldersToCopy) }

        });
        _logger = new Mock<ILogger<SettingsReader>>();
        _sut = new SettingsReader(_fileSystem, _logger.Object);
    }

    [Fact]
    public void Load_SampleFile_DeserializationOk()
    {
        //given
        //when
        CopyFolders? settings = _sut.Load<CopyFolders>(_file1);

        //then
        settings.Should().NotBeNull();
        settings?.Folders.Should().NotBeNull();        
        settings?.Folders?.Count.Should().Be(3);

    }

    [Fact]
    public void Load_EmptyFile_DeserializationOk()
    {
        //given
        _sut = new SettingsReader(_fileSystem,_logger.Object);
        
        //when
        CopyFolders? settings = _sut.Load<CopyFolders>(_file2);

        //then
        settings.Should().NotBeNull();
        settings?.Folders.Should().NotBeNull();
        settings?.Folders?.Count.Should().Be(0);

    }
}