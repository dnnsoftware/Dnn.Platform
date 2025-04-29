// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

/// <summary>A cake task to generate the Microsoft.Extensions.FileSystemGlobbing package.</summary>
public sealed class PackageMicrosoftGlobbing : PackageComponentTask
{
    /// <summary>Initializes a new instance of the <see cref="PackageMicrosoftGlobbing"/> class.</summary>
    public PackageMicrosoftGlobbing()
        : base("MicrosoftGlobbing", "Microsoft.Extensions.FileSystemGlobbing.dll", "Microsoft.Extensions.FileSystemGlobbing")
    {
    }
}
