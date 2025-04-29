// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.Diagnostics;
using Cake.Common.Tools.GitVersion;
using Cake.Frosting;

using Dnn.CakeUtils;

using Newtonsoft.Json;

/// <summary>A cake task to calculate the version.</summary>
public sealed class SetVersion : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        if (context.Settings.Version == "auto")
        {
            context.Version = context.GitVersion();
            context.BuildNumber = context.Version.LegacySemVerPadded;
        }
        else
        {
            context.Version = new GitVersion();
            var assemblyInfo = new AssemblyInfo("SolutionInfo.cs");
            var requestedVersion = context.Settings.Version == "off"
                ? assemblyInfo.GetVersion()
                : new Version(context.Settings.Version);
            context.Version.Major = requestedVersion.Major;
            context.Version.Minor = requestedVersion.Minor;
            context.Version.Patch = requestedVersion.Build;
            context.Version.InformationalVersion = requestedVersion.ToString(3) + " Custom build";
            context.Version.MajorMinorPatch = requestedVersion.ToString(3);
            context.Version.FullSemVer = requestedVersion.ToString(3);
            if (requestedVersion.Revision != -1)
            {
                context.Version.CommitsSinceVersionSource = requestedVersion.Revision;
                context.Version.InformationalVersion = requestedVersion.ToString(4) + " Custom build";
            }

            context.BuildNumber = requestedVersion.ToString(3);
        }

        context.Information(JsonConvert.SerializeObject(context.Version));
        if (context.Settings.Version != "off")
        {
            context.UpdateAssemblyInfoVersion(
                new Version(
                    context.Version.Major,
                    context.Version.Minor,
                    context.Version.Patch,
                    context.Version.CommitsSinceVersionSource ?? 0),
                context.Version.InformationalVersion,
                "SolutionInfo.cs");
        }

        context.Information("Informational Version : " + context.Version.InformationalVersion);
        context.ProductVersion = context.Version.MajorMinorPatch;
        context.Information("Product Version : " + context.ProductVersion);
        context.Information("Build Number : " + context.BuildNumber);
        context.Information("The build Id is : " + context.BuildId);
    }
}
