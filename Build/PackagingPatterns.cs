// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build;

/// <summary>The patterns of files to include and exclude from packaging.</summary>
public class PackagingPatterns
{
    /// <summary>Gets or sets a set of files to exclude from the install package.</summary>
    public string[] InstallExclude { get; set; }

    /// <summary>Gets or sets a set of files to include from the install package.</summary>
    public string[] InstallInclude { get; set; }

    /// <summary>Gets or sets a set of files to exclude from the upgrade package.</summary>
    public string[] UpgradeExclude { get; set; }

    /// <summary>Gets or sets a set of files to include from the symbols package.</summary>
    public string[] SymbolsInclude { get; set; }

    /// <summary>Gets or sets a set of files to exclude from the symbols package.</summary>
    public string[] SymbolsExclude { get; set; }
}
