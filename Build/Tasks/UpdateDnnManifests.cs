// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.IO;
using System.Linq;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.FileHelpers;
using Cake.Frosting;
using Cake.XdtTransform;

using Dnn.CakeUtils;

/// <summary>A cake task to set the <c>version</c> attribute in all of the <c>.dnn</c> manifest files.</summary>
[IsDependentOn(typeof(SetVersion))]
[IsDependentOn(typeof(SetPackageVersions))]
public sealed class UpdateDnnManifests : FrostingTask<Context>
{
    /// <summary>Gets the version from the context and formats it as a string.</summary>
    /// <param name="context">The context.</param>
    /// <returns>A string in the format <c>"09.09.00"</c>.</returns>
    public static string GetVersionString(Context context)
    {
        return $"{context.Version.Major:00}.{context.Version.Minor:00}.{context.Version.Patch:00}";
    }

    /// <inheritdoc/>
    public override void Run(Context context)
    {
        var unversionedManifests = context.FileReadLines("./Build/Tasks/unversionedManifests.txt");
        foreach (var file in context.GetFilesByPatterns(".", new[] { "**/*.dnn" }, unversionedManifests))
        {
            if (context.Settings.Version == "off")
            {
                return;
            }

            context.Information("Transforming: " + file);
            var transformFile = context.File(Path.GetTempFileName());
            context.FileAppendText(transformFile, this.GetXdtTransformation(context));
            context.XdtTransformConfig(file, transformFile, file);
        }
    }

    private string GetXdtTransformation(Context context)
    {
        return $@"<?xml version=""1.0""?>
<dotnetnuke xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
  <packages>
    <package version=""{GetVersionString(context)}"" 
             xdt:Transform=""SetAttributes(version)"" />
  </packages>
</dotnetnuke>";
    }
}
