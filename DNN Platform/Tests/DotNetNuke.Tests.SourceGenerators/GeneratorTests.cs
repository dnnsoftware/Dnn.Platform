// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.SourceGenerators;

[TestFixture]
public class GeneratorTests
{
    [Test]
    public async Task NoAttribute_GenerateNothing()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

public partial class D
{
}

""");
    }

    [Test]
    public async Task NotPartial_ReportsError()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve B via dependency injection.")]
public class D
{
}

""");
    }

    [Test]
    public async Task Class()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve B via dependency injection.")]
public partial class D
{
}

""");
    }

    [Test]
    public async Task GenericClass()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve B via dependency injection.")]
public partial class D<T,U>
{
}

""");
    }

    [Test]
    public async Task NestedGeneric()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

public partial class D<T,U>
{
    [DnnDeprecated(10, 0, 0, "Please use outer class.")]
    public partial class C<T,X,Z>
    {
    }
}

""");
    }

    [Test]
    public async Task Interface()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use the other B.")]
public partial interface B
{
}

""");
    }

    [Test]
    public async Task GenericInterface()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use the other B.")]
public partial interface B<T>
{
}

""");
    }

    [Test]
    public async Task Struct()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use PageInfo.")]
public partial struct A
{
}

""");
    }

    [Test]
    public async Task Record()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use PageInfo.")]
public partial record A
{
}

""");
    }

    [Test]
    public async Task NestedRecord()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

public partial class A
{
    [DnnDeprecated(9, 1, 2, "Please use InnerPageInfo.")]
    public partial record H
    {
    }
}

""");
    }

    [Test]
    public async Task SpecialCharacters()
    {
        await Verify(""""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(9, 1, 2, @"Use PageMaker.MakePage(""PageType0"")")]
public partial class A
{
    [DnnDeprecated(9, 2, 2, "Use PageMaker.MakePage(\"\n\tPageType1\t\n\")")]
    public static partial void I()
    {
    }

    [DnnDeprecated(9, 2, 2, """ Use PageMaker.MakePage("PageType2") """)]
    public static partial void J()
    {
    }
}

"""");
    }

    [Test]
    public async Task Methods()
    {
        await Verify("""
namespace G;

using System;
using System.Collections.Generic;
using DotNetNuke.Internal.SourceGenerators;

internal partial class A
{
    internal partial static class K
    {
        [DnnDeprecated(8, 4, 4, "Use overload taking IServiceProvider.")]
        public static partial void L<T>(ref IEnumerable<T> i)
            where T : class, new()
        {
            return;
        }

        [DnnDeprecated(9, 4, 4, "Use overload taking IApplicationStatusInfo.")]
        public static partial int?[] M(int? a = null, int b = -1)
        {
            return new[] { a, b, };
        }
    }

    internal partial class Wrapper<T>
    {
        [DnnDeprecated(8, 4, 4, "Use overload taking IApplicationStatusInfo.")]
        internal partial (decimal, Int32) N(decimal x, bool addOne = true)
        {
            var theInt = addOne ? 1 : 2;
            return (x + theInt, theInt);
        }

        [DnnDeprecated(9, 4, 4, "Use overload taking IServiceProvider.")]
        public static partial System.Text.StringBuilder O(string y, out String z)
        {
            z = nameof(CombineThings);
            return new StringBuilder(y + z);
        }
    }
}

""");
    }

    [Test]
    public async Task ParamsParameter()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

partial class A
{
    [DnnDeprecated(9, 9, 1, "Use overload taking IEnumerable.")]
    partial void P(params int[] numbers)
    {
    }
}

""");
    }

    [Test]
    public async Task OptionalParameters()
    {
        await Verify("""
namespace G;

using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

using DotNetNuke.Internal.SourceGenerators;

partial class A
{
    [DnnDeprecated(7, 0, 0, "No replacement", RemovalVersion = 11)]
    public static partial ArrayList Q(
        DirectoryInfo currentDirectory,
        [Optional, DefaultParameterValue("")] // ERROR: Optional parameters aren't supported in C#
        string strExtensions,
        [Optional, DefaultParameterValue(true)] // ERROR: Optional parameters aren't supported in C#
        bool noneSpecified)
    {
        return null;
    }
}

""");
    }

    [Test]
    public async Task ExtensionMethod()
    {
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

partial class A
{
    [DnnDeprecated(9, 9, 1, "Use overload taking IEnumerable.")]
    partial void R(this int[] numbers, string another)
    {
    }
}

""");
    }

    private static async Task Verify(string source)
    {
        var references =
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Append(MetadataReference.CreateFromFile(typeof(DnnDeprecatedGenerator).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "AnAssemblyName",
            new[] { CSharpSyntaxTree.ParseText(source), },
            references);
        var driver =
            CSharpGeneratorDriver
                .Create(new DnnDeprecatedGenerator())
                .RunGenerators(compilation);
        await Verifier.Verify(driver).UseDirectory("Snapshots");
    }
}
