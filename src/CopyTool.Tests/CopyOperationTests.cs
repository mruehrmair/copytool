using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using Xunit;

namespace CopyTool.Tests;

public class CopyOperationTests
{
    private readonly CopyOperation _sut;
    private readonly MockFileSystem _fileSystem;
    private const string _file1 = @"c:\testfolder\testfile1.txt";
    private const string _file2 = @"c:\testfolder\testfileCopy.txt";
    private const string _file3 = @"c:\testfolder\testfileCopy3.txt";
    private const string _file4 = @"c:\folder1src\testfileCopy.txt";
    private const string _folder1 = @"c:\testfolder";
    private const string _text = "Testing is meh.";

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

    private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public CopyOperationTests()
    {
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{_folder1}", new MockDirectoryData() },
            { @$"{_file1}", new MockFileData(_text) },
            { @$"{_file3}", new MockFileData(_text) },
            { @"c:\folder1src", new MockDirectoryData()},
            { @"c:\folder2src", new MockDirectoryData()},
            { @"c:\folder3src", new MockDirectoryData()},
            { @$"{_file4}", new MockFileData(_text) },
            { @"c:\myfile.txt", new MockFileData(_text) },
            { @"c:\demo\jQuery.js", new MockFileData("some js") },
            { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
        });
        _sut = new CopyOperation(_fileSystem);
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
        await action.Should().ThrowAsync<System.UnauthorizedAccessException>();
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

    [Theory]
    [InlineData(@"c:\folder1dest\")]
    [InlineData(@"c:\folder2dest\")]
    [InlineData(@"c:\folder3dest\")]
    public async void FolderCopy_MultipleFoldersInJson_CopyOk(string testFolderDest)
    {
        //given
        CopyFolders? foldersToCopy =
                JsonSerializer.Deserialize<CopyFolders>(_jsonConfig, _jsonOptions);

        //when
        await _sut.FolderCopy(foldersToCopy);

        //then
        string? expectedFolder = testFolderDest;
        _fileSystem.FileExists(expectedFolder).Should().Be(true);

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

    [Fact]
    public async void FolderCopy_JsonContainsNoFolders_NoException()
    {
        //given
        CopyFolders? foldersToCopy = JsonSerializer.Deserialize<CopyFolders>(_jsonConfigNoFoldersToCopy, _jsonOptions);

        //when
        var action = async () => await _sut.FolderCopy(foldersToCopy);

        //then
        await action.Should().NotThrowAsync<Exception>();

    }
}