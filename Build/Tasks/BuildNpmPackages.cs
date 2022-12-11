// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.IO;
    using Cake.Core;
    using Cake.Core.IO;
    using Cake.Core.Tooling;
    using Cake.Frosting;
    using Cake.Yarn;

    /// <summary>
    /// Builds the npm packages for the entire solution.
    /// </summary>
    public sealed class BuildNpmPackages : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var yarn = new YarnRunner(
                context.FileSystem,
                context.Environment,
                context.ProcessRunner,
                context.Tools);
            yarn.Install(c => c
                .WithArgument("--no-immutable")
                .WithWorkingDirectory(context.Directory("./")));
            yarn.RunScript("build");
        }
    }
}
