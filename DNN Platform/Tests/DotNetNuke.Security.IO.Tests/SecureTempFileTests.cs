// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.IO.Tests;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DotNetNuke.Security.IO;

using FluentAssertions;

using Xunit;

public class SecureTempFileTests
{
    [Fact]
    public void Create_Produces_File_Inside_TempDirectory()
    {
        using var temp = new SecureTempFile();

        File.Exists(temp.Path).Should().BeTrue();
        Path.GetDirectoryName(temp.Path)!.TrimEnd(Path.DirectorySeparatorChar)
            .Should().Be(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar));
    }

    [Fact]
    public void Path_Format_Matches_GetRandomFileName_Pattern()
    {
        using var temp = new SecureTempFile();

        var name = Path.GetFileName(temp.Path);

        // Path.GetRandomFileName format: 8 chars + '.' + 3 chars (cryptographically random).
        name.Should().MatchRegex(@"^[a-z0-9]{8}\.[a-z0-9]{3}$");
    }

    [Fact]
    public void Create_Returns_Unique_Paths_For_Concurrent_Callers()
    {
        const int count = 64;
        var files = new SecureTempFile[count];
        try
        {
            Parallel.For(0, count, i => files[i] = new SecureTempFile());

            files.Select(f => f.Path).Distinct().Count().Should().Be(count);
        }
        finally
        {
            foreach (var f in files)
            {
                f?.Dispose();
            }
        }
    }

    [Fact]
    public void Dispose_Deletes_File_From_Disk()
    {
        string path;
        using (var temp = new SecureTempFile())
        {
            path = temp.Path;
            File.Exists(path).Should().BeTrue();
        }

        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void Dispose_Is_Idempotent()
    {
        var temp = new SecureTempFile();

        temp.Dispose();
        Action secondDispose = () => temp.Dispose();

        secondDispose.Should().NotThrow();
        temp.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Access_After_Dispose_Throws_ObjectDisposedException()
    {
        var temp = new SecureTempFile();
        temp.Dispose();

        Action getPath = () => _ = temp.Path;
        Action getStream = () => _ = temp.Stream;

        getPath.Should().Throw<ObjectDisposedException>();
        getStream.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Stream_Has_Exclusive_Lock_While_Open()
    {
        using var temp = new SecureTempFile();

        Action openSecondHandle = () =>
            new FileStream(temp.Path, FileMode.Open, FileAccess.Read, FileShare.None).Dispose();

        openSecondHandle.Should().Throw<IOException>();
    }

    [Fact]
    public void Stream_Is_ReadWritable()
    {
        using var temp = new SecureTempFile();

        var payload = new byte[] { 1, 2, 3, 4, 5 };
        temp.Stream.Write(payload, 0, payload.Length);
        temp.Stream.Position = 0;

        var read = new byte[payload.Length];
        temp.Stream.Read(read, 0, read.Length).Should().Be(payload.Length);
        read.Should().Equal(payload);
    }
}
