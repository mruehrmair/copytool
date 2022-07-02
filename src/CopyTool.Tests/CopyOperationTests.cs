using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;
using System.Text.Json;
using Xunit;

namespace CopyTool.Tests;

public class CopyOperationTests : CopyToolTestsBase
{
    private readonly MockFileSystem _fileSystem;

    private readonly CopyOperation _sut;
    private readonly Mock<ISettingsReader> _settingsReader;
    private const string _file1 = @"c:\testfolder1\testfile1.txt";
    private const string _file1a = @"c:\testfolder2\testfile1.txt";
    private const string _file3 = @"c:\testfolder\testfileCopy3.txt";
    private const string _file4 = @"c:\folder1src\testfileCopy.txt";
    private const string _settingsFile = @"c:\settings.json";
    private const string _settingsFileDoesNotExist = @"c:\settingsN.json";
    private const string _folder1 = @"c:\testfolder1";
    private const string _folderDoesNotExist = @"c:\testfolderN";
    private const string _folder2 = @"c:\testfolder2";
    private const string _folder3 = @"c:\testfolder3";
    private const string _file2 = @"c:\testfolder3\testfile1.txt";
    private const string _text = "Testing is meh.";

    private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public CopyOperationTests()
    {
        CopyFolders? foldersToCopy = JsonSerializer.Deserialize<CopyFolders>(_jsonConfig, _jsonOptions);
        foldersToCopy = UpdatePathsToNonWindowsCompatibility(foldersToCopy);

        _settingsReader = new Mock<ISettingsReader>();
        _settingsReader.Setup(x => x.Load<CopyFolders>(It.IsIn(new[] { XFS.Path(_settingsFile) }))).Returns(foldersToCopy);

        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { XFS.Path(@$"{_folder1}"), new MockDirectoryData() },
            { XFS.Path(@$"{_folder3}"), new MockDirectoryData() },
            { XFS.Path(@$"{_file1}"), new MockFileData(_text) },
            { XFS.Path(@$"{_file2}"), new MockFileData(_text) },
            { XFS.Path(@$"{_file3}"), new MockFileData(_text) },
            { XFS.Path(@"c:\folder1src"), new MockDirectoryData()},
            { XFS.Path(@"c:\folder2src"), new MockDirectoryData()},
            { XFS.Path(@"c:\folder3src"), new MockDirectoryData()},
            { XFS.Path(@$"{_file4}"), new MockFileData(_text) },
            { XFS.Path(@$"{_settingsFile}"), new MockFileData(_jsonConfig) },
            { XFS.Path(@"c:\demo\jQuery.js"), new MockFileData("some js") },
            { XFS.Path(@"c:\demo\image.gif"), new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
        });
        var logger = Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .CreateLogger();
        var debugLogger = new SerilogLoggerFactory(logger)
            .CreateLogger<CopyOperation>();
        _sut = new CopyOperation(_fileSystem, _settingsReader.Object, debugLogger);
    }

    [Fact]
    public async void FolderCopy_OneFolder_CopyOk()
    {
        //given
        var testFolderSrc = XFS.Path(_folder1);
        var testFolderDest = XFS.Path(_folder2);
        //when
        await _sut.FolderCopy(testFolderSrc, testFolderDest);
        //then
        var expectedFile = XFS.Path(_file1a);
        _fileSystem.FileExists(expectedFile).Should().Be(true);

    }

    [Fact]
    public async void FolderCopy_SrcFolderDoesNotExist_CopyFails()
    {
        //given
        var testFolderSrc = XFS.Path(_folderDoesNotExist);
        var testFolderDest = XFS.Path(_folder2);

        //when
        //var action = async () => await _sut.FolderCopy(testFolderSrc, testFolderDest);
        var result = await _sut.FolderCopy(testFolderSrc, testFolderDest);

        //then
        result.Should().Be(false);
    }

    [Fact]
    public async void FileRead_Ok()
    {
        //given
        var testFileSrc = XFS.Path(_file1);

        //when
        var text = await _fileSystem.File.ReadAllLinesAsync(testFileSrc);

        //then
        var expectedText = _text;
        string.Join("", text).Should().Be(expectedText);
    }

    [Fact]
    public async void FolderCopy_DestinationReadOnly_CopyFails()
    {
        //given
        var testFolderSrc = _folder1;
        var testFolderDest = _folder3;

        _fileSystem.FileInfo.FromFileName(XFS.Path(_file2)).IsReadOnly = true;

        //when
        var result = await _sut.FolderCopy(testFolderSrc, testFolderDest);

        //then
        result.Should().Be(false);

    }

    [Fact]
    public async void FolderCopy_MultipleFoldersInJson_CopyOk()
    {
        //given
        var expectedFolders = new[] { XFS.Path(@"c:\folder1dest\"), XFS.Path(@"c:\folder2dest\"), XFS.Path(@"c:\folder3dest\") };

        //when
        await _sut.FolderCopy(XFS.Path(_settingsFile));

        //then
        foreach (var expectedFolder in expectedFolders)
        {
            _fileSystem.FileExists(expectedFolder).Should().Be(true);
        }
    }

    [Fact]
    public async void FolderCopy_SettingsFileDoesNotExist_CopyFails()
    {
        //given

        //when
        var result = await _sut.FolderCopy(_settingsFileDoesNotExist);

        //then
        result.Should().Be(false);

    }
}