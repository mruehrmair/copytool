using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using Xunit;

namespace CopyTool.Tests;

public class CopyOperationTests : CopyToolTestsBase
{
    private readonly MockFileSystem _fileSystem;

    private readonly CopyOperation _sut;
    private readonly Mock<ISettingsReader> _settingsReader;
    private const string _file1 = @"c:\testfolder\testfile1.txt";
    private const string _file2 = @"c:\testfolder\testfileCopy.txt";
    private const string _file3 = @"c:\testfolder\testfileCopy3.txt";
    private const string _file4 = @"c:\folder1src\testfileCopy.txt";
    private const string _settingsFile = @"c:\settings.json";
    private const string _folder1 = @"c:\testfolder";
    private const string _text = "Testing is meh.";

    private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public CopyOperationTests()
    {
        CopyFolders? foldersToCopy = JsonSerializer.Deserialize<CopyFolders>(_jsonConfig, _jsonOptions);

        _settingsReader = new Mock<ISettingsReader>();
        _settingsReader.Setup(x => x.Load<CopyFolders>()).Returns(foldersToCopy);
        
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{_folder1}", new MockDirectoryData() },
            { @$"{_file1}", new MockFileData(_text) },
            { @$"{_file3}", new MockFileData(_text) },
            { @"c:\folder1src", new MockDirectoryData()},
            { @"c:\folder2src", new MockDirectoryData()},
            { @"c:\folder3src", new MockDirectoryData()},
            { @$"{_file4}", new MockFileData(_text) },
            { @$"{_settingsFile}", new MockFileData(_jsonConfig) },
            { @"c:\demo\jQuery.js", new MockFileData("some js") },
            { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
        });
        var logger = Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .CreateLogger();
        var debugLogger = new SerilogLoggerFactory(logger)
            .CreateLogger<CopyOperation>();
        _sut = new CopyOperation(_fileSystem, _settingsReader.Object, debugLogger);
    }

    [Fact]
    public async void FileCopy_TwoFiles_CopyOk()
    {
        //given
        var testFileSrc = _file1;
        var testFileDest = _file2;
        //when
        await _sut.FileCopy(testFileSrc, testFileDest);
        //then
        var expectedFile = testFileDest;
        _fileSystem.FileExists(expectedFile).Should().Be(true);

    }

    [Fact]
    public async void FileRead_Ok()
    {
        //given
        var testFileSrc = _file1;

        //when
        var text = await _fileSystem.File.ReadAllLinesAsync(testFileSrc);

        //then
        var expectedText = _text;
        string.Join("", text).Should().Be(expectedText);
    }

    [Fact]
    public async void FileCopy_DestinationReadOnly_ThrowsException()
    {
        //given
        var testFileSrc = _file1;
        var testFileDest = _file3;

        var expectedFile = testFileDest;
        _fileSystem.FileInfo.FromFileName(expectedFile).IsReadOnly = true;

        //when
        var action = async () => await _sut.FileCopy(testFileSrc, testFileDest);

        //then
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async void FolderCopy_TwoFolders_CopyOk()
    {
        //given
        const string testFolderSrc = @"c:\testfolder\";
        const string testFolderDest = @"c:\testfolder1\";

        var foldersToCopy = new CopyFolder(testFolderSrc, testFolderDest);

        //when
        await _sut.FolderCopy(foldersToCopy);

        //then
        string? expectedFolder = testFolderDest;
        _fileSystem.FileExists(expectedFolder).Should().Be(true);

    }

    [Fact]
    public async void FolderCopy_MultipleFoldersInJson_CopyOk()
    {
        //given
        var expectedFolders = new[] { @"c:\folder1dest\", @"c:\folder2dest\", @"c:\folder3dest\" };
        
        //when
        await _sut.FolderCopy(_settingsFile);

        //then
        foreach(var expectedFolder in expectedFolders)
        {
            _fileSystem.FileExists(expectedFolder).Should().Be(true);
        }
    }

    [Fact]
    public async void FolderCopy_JsonIsNull_ArgumentNullException()
    {
        //given
        CopyFolders? foldersToCopy = null;

        //when
        var action = async () => await _sut.FolderCopy(foldersToCopy);

        //then
        await action.Should().ThrowAsync<ArgumentNullException>();

    }
}