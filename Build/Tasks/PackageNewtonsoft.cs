// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

/// <summary>A cake task to generate the Newtonsoft.Json package.</summary>
public sealed class PackageNewtonsoft : PackageComponentTask
{
    /// <summary> Initializes a new instance of the <see cref="PackageNewtonsoft"/> class. </summary>
    public PackageNewtonsoft()
        : base("Newtonsoft.Json", "Newtonsoft.Json.dll", "Newtonsoft")
    {
    }
}
