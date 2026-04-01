// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Build.Tasks;

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;
using Dnn.CakeUtils;

/// <summary>A cake task to generate the Microsoft.CodeDom.Providers.DotNetCompilerPlatform package.</summary>
public sealed class PackageMicrosoftCodeDomProvidersDotNetCompilerPlatform : AsyncFrostingTask<Context>
{
    private static readonly string[] AllFiles = ["*",];
    private static readonly string[] ManifestFiles = ["*.dnn",];

    /// <inheritdoc />
    public override async Task RunAsync(Context context)
    {
        var binDir = context.WebsiteDir.Path.Combine("bin");
        var mainAssemblyPath = binDir.CombineWithFilePath("Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll");
        var packageVersion = FileVersionInfo.GetVersionInfo(context.MakeAbsolute(mainAssemblyPath).FullPath).FileVersion;

        var packageZip = context.WebsiteDir.Path.CombineWithFilePath($"Install/Library/Microsoft.CodeDom.Providers.DotNetCompilerPlatform_{packageVersion}_Install.zip");
        var packageDir = context.Directory("DNN Platform/Components/Microsoft.CodeDom.Providers.DotNetCompilerPlatform");

        context.Information($"Creating {packageZip}");
        context.Zip(
            packageDir,
            packageZip,
            context.GetFilesByPatterns(packageDir, AllFiles, ManifestFiles));

        var manifestPath = context.GetFiles(packageDir.Path.CombineWithFilePath("*.dnn").ToString()).Single();
        context.Information($"Reading manifest from {manifestPath}");
        var manifest = new XmlDocument();
        using (var manifestReader = XmlReader.Create(new StringReader(context.ReadFile(manifestPath)), new XmlReaderSettings { XmlResolver = null, }))
        {
            manifest.Load(manifestReader);
        }

        var assemblies =
            from XmlNode assemblyNode in manifest.SelectNodes("//assembly")
            where assemblyNode.Attributes?["action"]?.Value != "UnRegister"
            from XmlNode childNode in assemblyNode.ChildNodes
            where childNode.LocalName.Equals("name", System.StringComparison.Ordinal)
            select childNode;

        foreach (var assemblyNameNode in assemblies)
        {
            var assemblyPath = binDir.CombineWithFilePath(assemblyNameNode.InnerText);
            context.Information($"Adding {assemblyPath} to {packageZip}");
            context.AddFilesToZip(
                packageZip,
                context.MakeAbsolute(context.WebsiteDir.Path),
                context.GetFiles(assemblyPath.ToString()),
                append: true);

            var versionNode = assemblyNameNode.ParentNode.ChildNodes.Cast<XmlNode>()
                .SingleOrDefault(childNode => childNode.LocalName.Equals("version", System.StringComparison.Ordinal));
            if (versionNode != null)
            {
                versionNode.InnerText = FileVersionInfo.GetVersionInfo(context.MakeAbsolute(assemblyPath).FullPath).FileVersion;
                context.Information($"Set {assemblyPath} version to {versionNode.InnerText}");
            }
        }

        manifest.SelectSingleNode("//package[@version]").Attributes["version"].Value = packageVersion;

        context.AddXmlFileToZip(packageZip, manifest, manifestPath.GetFilename().ToString(), append: true);

        using var zipStream = new MemoryStream();
        await ZipFile.CreateFromDirectoryAsync(binDir.Combine("roslyn").ToString(), zipStream);
        await zipStream.FlushAsync();
        zipStream.Position = 0;
        context.AddStreamToZip(packageZip, zipStream, "roslyn.zip", append: true);
    }
}
