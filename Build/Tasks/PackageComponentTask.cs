// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Build.Tasks;

using System.Linq;
using System.Xml;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core.IO;
using Cake.Frosting;
using Dnn.CakeUtils;

/// <summary>Provides the base functionality for packaging a folder inside Components.</summary>
public abstract class PackageComponentTask : FrostingTask<Context>
{
    /// <summary>Initializes a new instance of the <see cref="PackageComponentTask"/> class.</summary>
    /// <param name="componentName">The name of the component.</param>
    /// <param name="primaryAssemblyName">The name of the primary assembly.</param>
    /// <param name="componentFolderName">The name of the folder in <c>DNN Platform/Components/</c>.</param>
    protected PackageComponentTask(string componentName, FilePath primaryAssemblyName = null, DirectoryPath componentFolderName = null)
    {
        this.ComponentName = componentName;
        this.ComponentFolderName = componentFolderName ?? componentName;
        this.PrimaryAssemblyName = primaryAssemblyName ?? $"{componentName}.dll";
    }

    /// <summary>Gets the name of the component.</summary>
    public string ComponentName { get; }

    /// <summary>Gets the name of the folder in <c>DNN Platform/Components/</c> where the component files are.</summary>
    public DirectoryPath ComponentFolderName { get; }

    /// <summary>Gets the name of the primary assembly.</summary>
    public FilePath PrimaryAssemblyName { get; }

    /// <inheritdoc />
    public override void Run(Context context)
    {
        var binDir = context.WebsiteDir.Path.Combine("bin");
        var mainAssemblyPath = binDir.CombineWithFilePath(this.PrimaryAssemblyName);
        var packageVersion = context.GetAssemblyFileVersion(mainAssemblyPath);

        var packageZip = context.WebsiteDir.Path.CombineWithFilePath($"Install/Library/{this.ComponentName}_{packageVersion}_Install.zip");
        var packageDir = context.Directory($"DNN Platform/Components/{this.ComponentFolderName}");

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

            var versionNode = assemblyNameNode.ParentNode?.ChildNodes.Cast<XmlNode>()
                .SingleOrDefault(childNode => childNode.LocalName.Equals("version"));
            if (versionNode != null)
            {
                versionNode.InnerText = context.GetAssemblyFileVersion(assemblyPath);
                context.Information($"Set {assemblyPath} version to {versionNode.InnerText}");
            }
        }

        manifest.SelectSingleNode("//package[@version]").Attributes["version"].Value = packageVersion;

        context.AddXmlFileToZip(packageZip, manifest, manifestPath.GetFilename().ToString(), append: true);
    }
}
