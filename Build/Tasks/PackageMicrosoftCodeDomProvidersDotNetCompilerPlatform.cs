// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Build.Tasks;

using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Dnn.CakeUtils;

/// <summary>A cake task to generate the Microsoft.CodeDom.Providers.DotNetCompilerPlatform package.</summary>
public sealed class PackageMicrosoftCodeDomProvidersDotNetCompilerPlatform()
    : PackageComponentTaskBase("Microsoft.CodeDom.Providers.DotNetCompilerPlatform")
{
    /// <inheritdoc />
    public override async Task RunAsync(Context context)
    {
        await base.RunAsync(context);

        var binDir = GetBinDir(context);
        using var zipStream = new MemoryStream();
        await ZipFile.CreateFromDirectoryAsync(binDir.Combine("roslyn").ToString(), zipStream);
        await zipStream.FlushAsync();
        zipStream.Position = 0;
        context.AddStreamToZip(this.GetPackageZipPath(context, binDir), zipStream, "roslyn.zip", append: true);
    }
}
