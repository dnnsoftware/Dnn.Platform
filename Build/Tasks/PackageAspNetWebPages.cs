// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

/// <summary>A cake task to generate the ASP.NET Web Pages package.</summary>
public sealed class PackageAspNetWebPages : PackageComponentTask
{
    /// <summary>Initializes a new instance of the <see cref="PackageAspNetWebPages"/> class.</summary>
    public PackageAspNetWebPages()
        : base("AspNetWebPages", "System.Web.WebPages.dll", "Microsoft.AspNetWebPages")
    {
    }
}
