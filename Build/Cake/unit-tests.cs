using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.VSTest;
using Cake.Common.Tools.VSWhere;
using Cake.Common.Tools.VSWhere.Latest;
using Cake.Core.Diagnostics;
using Cake.Frosting;

/// <summary>
/// Runs units tests on solution. Make sure to build the solution before running this task.
/// </summary>
public sealed class UnitTests : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var testAssemblies = context.GetFiles($@"**\bin\{context.configuration}\DotNetNuke.Tests.*.dll");
        testAssemblies += context.GetFiles($@"**\bin\{context.configuration}\Dnn.PersonaBar.*.Tests.dll");
        testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Utilities.dll");
        
        // TODO: address issues to allow these tests to run
        testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Integration.dll");
        testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Urls.dll");

        context.VSTest(
            testAssemblies,
            FixToolPath(
                context, 
                new VSTestSettings
                {
                    Logger = "trx",
                    Parallel = true,
                    EnableCodeCoverage = true,
                    TestAdapterPath = @"tools\NUnitTestAdapter.2.3.0\build"
                }));
    }

// https://github.com/cake-build/cake/issues/1522
    VSTestSettings FixToolPath(Context context, VSTestSettings settings)
    {
// #tool vswhere
        settings.ToolPath =
            context.VSWhereLatest(new VSWhereLatestSettings {Requires = "Microsoft.VisualStudio.PackageGroup.TestTools.Core"})
                .CombineWithFilePath(context.File(@"Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"));
        return settings;
    }
}

