// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Common.IO;
    using Cake.Common.Tools.VSTest;
    using Cake.Frosting;

    /// <summary>Runs units tests on solution. Make sure to build the solution before running this task.</summary>
    public sealed class UnitTests : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var testAssemblies = context.GetFiles(@"**\bin\**\DotNetNuke.Tests.*.dll");
            testAssemblies += context.GetFiles(@"**\bin\**\Dnn.PersonaBar.*.Tests.dll");
            testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Utilities.dll");

            // TODO: address issues to allow these tests to run
            testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Integration.dll");
            testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Urls.dll");

            var vsTestPath = context.GetFiles($"tools/Microsoft.TestPlatform.{Program.MicrosoftTestPlatformVersion}/tools/**/vstest.console.exe")
                .First();
            context.VSTest(
                testAssemblies,
                new VSTestSettings
                {
                    ToolPath = vsTestPath,
                    Logger = "trx",
                    Parallel = true,
                    EnableCodeCoverage = true,
                    TestAdapterPath = $@"tools\NUnit3TestAdapter.{Program.NUnit3TestAdapterVersion}\build",
                });
        }
    }
}
