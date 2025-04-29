// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.FileHelpers;
using Cake.Frosting;

/// <summary>A cake task to set the version of client-side packages.</summary>
[IsDependentOn(typeof(SetVersion))]
public sealed class SetPackageVersions : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        if (context.Settings.Version == "off")
        {
            return;
        }

        var packages = context.GetFiles("./Dnn.AdminExperience/ClientSide/*.Web/package.json");
        packages.Add(context.GetFiles("./Dnn.AdminExperience/ClientSide/Dnn.React.Common/package.json"));
        packages.Add(context.GetFiles("./Dnn.AdminExperience/ClientSide/*.Web/**/_exportables/package.json"));
        packages.Add(context.GetFiles("./DNN Platform/Modules/ResourceManager/ResourceManager.Web/package.json"));

        // Set all package.json in Admin Experience to the current version and to consume the current (local) version of dnn-react-common.
        foreach (var file in packages)
        {
            context.Information($"Updating {file} to version {context.Version.FullSemVer}");
            context.ReplaceRegexInFiles(
                file.ToString(),
                @"""version"": "".*""",
                $@"""version"": ""{context.Version.FullSemVer}""");
            context.ReplaceRegexInFiles(
                file.ToString(),
                @"""@dnnsoftware\/dnn-react-common"": "".*""",
                $@"""@dnnsoftware/dnn-react-common"": ""{context.Version.FullSemVer}""");
        }
    }
}
