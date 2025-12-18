// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System.Diagnostics;
using System.Linq;
using System.Xml;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;

using Dnn.CakeUtils;

/// <summary>A cake task to generate the Microsoft.Extensions.DependencyInjection  package.</summary>
public sealed class PackageMicrosoftExtensionsDependencyInjection : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        var binDir = context.WebsiteDir.Path.Combine("bin");
        var mainAssemblyPath = binDir.CombineWithFilePath("Microsoft.Extensions.DependencyInjection.dll");
        var packageVersion = FileVersionInfo.GetVersionInfo(context.MakeAbsolute(mainAssemblyPath).FullPath).FileVersion;

        var packageZip = context.WebsiteDir.Path.CombineWithFilePath($"Install/Library/Microsoft.Extensions.DependencyInjection_{packageVersion}_Install.zip");
        var packageDir = context.Directory("DNN Platform/Components/Microsoft.Extensions.DependencyInjection");

        context.Information($"Creating {packageZip}");
        context.Zip(
            packageDir.ToString(),
            packageZip,
            context.GetFilesByPatterns(packageDir, new[] { "*" }, new[] { "*.dnn" }));

        var manifestPath = context.GetFiles(packageDir.Path.CombineWithFilePath("*.dnn").ToString()).Single();
        context.Information($"Reading manifest from {manifestPath}");
        var manifest = new XmlDocument();
        manifest.LoadXml(context.ReadFile(manifestPath));
        var assemblies =
            from XmlNode assemblyNode in manifest.SelectNodes("//assembly")
            from XmlNode childNode in assemblyNode.ChildNodes
            where childNode.LocalName.Equals("name")
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
                .SingleOrDefault(childNode => childNode.LocalName.Equals("version"));
            if (versionNode != null)
            {
                versionNode.InnerText = FileVersionInfo.GetVersionInfo(context.MakeAbsolute(assemblyPath).FullPath).FileVersion;
                context.Information($"Set {assemblyPath} version to {versionNode.InnerText}");
            }
        }

        manifest.SelectSingleNode("//package[@version]").Attributes["version"].Value = packageVersion;

        context.AddXmlFileToZip(packageZip, manifest, manifestPath.GetFilename().ToString(), append: true);
    }
}
