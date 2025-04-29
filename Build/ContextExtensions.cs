// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Build;

using System;
using System.Diagnostics;

using Cake.Common.IO;
using Cake.Core.IO;

/// <summary>Provides extensions to <see cref="Context"/>.</summary>
public static class ContextExtensions
{
    /// <summary>Gets the <see cref="FileVersionInfo.FileVersion"/> for an assembly.</summary>
    /// <param name="context">The cake context.</param>
    /// <param name="assemblyPath">The path to the assembly file.</param>
    /// <returns>The file version.</returns>
    public static string GetAssemblyFileVersion(this Context context, FilePath assemblyPath)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(context.MakeAbsolute(assemblyPath).FullPath);
        var fileVersion = versionInfo.FileVersion;
        return Version.TryParse(fileVersion, out _) ? fileVersion : $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}";
    }
}
