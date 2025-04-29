// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using Cake.Common.IO;
using Cake.Common.Tools.Command;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

/// <summary>Builds the npm packages for the entire solution.</summary>
public sealed class BuildNpmPackages : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        var corepackCommand = new CommandSettings
        {
            ToolName = "Corepack",
            ToolExecutableNames = new[] { "corepack", "corepack.cmd", },
            WorkingDirectory = context.Directory("./"),
        };

        context.Command(
            corepackCommand,
            new ProcessArgumentBuilder()
                .Append("yarn")
                .Append("install")
                .Append("--no-immutable"));

        context.Command(
            corepackCommand,
            new ProcessArgumentBuilder()
                .Append("yarn")
                .Append("run")
                .Append("build"));
    }
}
