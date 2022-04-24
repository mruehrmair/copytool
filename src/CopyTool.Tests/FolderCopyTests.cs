using System.Collections.Generic;
using Xunit;
using System.IO.Abstractions.TestingHelpers;

namespace CopyTool.Tests;

public class FolderCopyTests
{
    CopyOperation _sut;

    public FolderCopyTests()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
            { @"c:\demo\jQuery.js", new MockFileData("some js") },
            { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
        });
        _sut = new CopyOperation(fileSystem);
    }

    [Fact]
    public async void FolderCopy_TwoFolders_CopyOk()
    {

       await _sut.FolderCopy("test", "test");

    }
}