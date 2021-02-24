using System.Linq;

using Cake.Common.IO;
using Cake.Common.Tools.VSTest;
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

        var vsTestPath = context.GetFiles("tools/Microsoft.TestPlatform.16.8.0/tools/**/vstest.console.exe").First();
        context.VSTest(
            testAssemblies,
            new VSTestSettings
            {
                ToolPath = vsTestPath,
                Logger = "trx",
                Parallel = true,
                EnableCodeCoverage = true,
                TestAdapterPath = @"tools\NUnitTestAdapter.2.3.0\build"
            });
    }
}

