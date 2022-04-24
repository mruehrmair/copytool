using FluentAssertions;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CopyTool.Tests;

public class FolderCopyTests
{
    private readonly CopyOperation _sut;
    private readonly MockFileSystem _fileSystem;

    public FolderCopyTests()
    {
        const string file1 = @"c:\testfolder\testfile1.txt";

        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{file1}", new MockFileData("Testing is meh.") },
            { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
            { @"c:\demo\jQuery.js", new MockFileData("some js") },
            { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
        });
        _sut = new CopyOperation(_fileSystem);
    }

    [Fact]
    public async void FileCopy_TwoFolders_CopyOk()
    {
        //given
        const string testFileSrc = @"c:\testfolder\testfile1.txt";
        const string testFileDest = @"c:\testfolder\testfileCopy.txt";
        //when
        await _sut.FileCopy(testFileSrc, testFileDest);
        //then
        var expectedFile = @"c:\testfolder\testfileCopy.txt";       
        _fileSystem.FileExists(expectedFile).Should().Be(true);

    }
}