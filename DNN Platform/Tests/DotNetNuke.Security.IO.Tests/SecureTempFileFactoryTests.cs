// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.IO.Tests;

using System.Linq;
using System.Threading.Tasks;

using DotNetNuke.Security.IO;

using FluentAssertions;

using Xunit;

public class SecureTempFileFactoryTests
{
    [Fact]
    public void Factory_Create_Returns_New_Instance_Each_Call()
    {
        ISecureTempFileFactory factory = new SecureTempFileFactory();

        using var a = factory.Create();
        using var b = factory.Create();

        a.Should().NotBeSameAs(b);
        a.Path.Should().NotBe(b.Path);
    }

    [Fact]
    public void Factory_Is_Thread_Safe_Under_Parallel_Create()
    {
        const int count = 64;
        ISecureTempFileFactory factory = new SecureTempFileFactory();
        var files = new SecureTempFile[count];

        try
        {
            Parallel.For(0, count, i => files[i] = factory.Create());

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
}
