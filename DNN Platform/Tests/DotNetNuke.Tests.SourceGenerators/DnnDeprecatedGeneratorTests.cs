// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.SourceGenerators;

[TestFixture]
public class DnnDeprecatedGeneratorTests
{
    [Test]
    public async Task NotDeprecatedClass_DoesNotGenerateAnything()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

public partial class PagesController
{
}

""");
    }

    [Test]
    public async Task DeprecatedNonPartialClass_ReportsAnErrorDiagnostic()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve IPagesController via dependency injection.")]
public class PagesController
{
}

""");
    }

    [Test]
    public async Task DeprecatedPartialClass_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve IPagesController via dependency injection.")]
public partial class PagesController
{
}

""");
    }

    [Test]
    public async Task DeprecatedGenericPartialClass_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve IPagesController via dependency injection.")]
public partial class PagesController<T,U>
{
}

""");
    }

    [Test]
    public async Task DeprecatedNestedGenericPartialClass_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

public partial class PagesController<T,U>
{
    [DnnDeprecated(10, 0, 0, "Please use outer class.")]
    public partial class Inner<T,X,Z>
    {
    }
}

""");
    }

    [Test]
    public async Task DeprecatedPartialInterface_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use the other IPagesController.")]
public partial interface IPagesController
{
}

""");
    }

    [Test]
    public async Task DeprecatedPartialGenericInterface_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use the other IPagesController.")]
public partial interface IPagesController<T>
{
}

""");
    }

    [Test]
    public async Task DeprecatedPartialStruct_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use PageInfo.")]
public partial struct Page
{
}

""");
    }

    [Test]
    public async Task DeprecatedPartialRecord_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use PageInfo.")]
public partial record Page
{
}

""");
    }

    [Test]
    public async Task DeprecatedNestedPartialRecord_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

public partial class Page
{
    [DnnDeprecated(9, 1, 2, "Please use InnerPageInfo.")]
    public partial record InnerPage
    {
    }
}

""");
    }

    [Test]
    public async Task DeprecatedMethods_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using System;
using System.Collections.Generic;
using DotNetNuke.Internal.SourceGenerators;

internal partial class Page
{
    internal partial static class StaticWrapper
    {
        [DnnDeprecated(8, 4, 4, "Use overload taking IServiceProvider.")]
        public static partial void DoAThing<T>(ref IEnumerable<T> i)
            where T : class, new()
        {
            return;
        }

        [DnnDeprecated(9, 4, 4, "Use overload taking IApplicationStatusInfo.")]
        public static partial int?[] GetTheseThings(int? a = null, int b = -1)
        {
            return new[] { a, b, };
        }
    }

    internal partial class Wrapper<T>
    {
        [DnnDeprecated(8, 4, 4, "Use overload taking IApplicationStatusInfo.")]
        internal partial (decimal, Int32) GetThemBoth(decimal x, bool addOne = true)
        {
            var theInt = addOne ? 1 : 2;
            return (x + theInt, theInt);
        }

        [DnnDeprecated(9, 4, 4, "Use overload taking IServiceProvider.")]
        public static partial System.Text.StringBuilder CombineThings(string y, out String z)
        {
            z = nameof(CombineThings);
            return new StringBuilder(y + z);
        }
    }
}

""");
    }

    [Test]
    public async Task DeprecatedMethodWithParamsParameter_AddsPartialWithObsoleteAttribute()
    {
        await Verify("""
namespace Example.Test;

using DotNetNuke.Internal.SourceGenerators;

partial class Page
{
    [DnnDeprecated(9, 9, 1, "Use overload taking IEnumerable.")]
    partial void WithParams(params int[] numbers)
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
                .Append(MetadataReference.CreateFromFile(typeof(DnnDeprecatedAttribute).Assembly.Location));
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
