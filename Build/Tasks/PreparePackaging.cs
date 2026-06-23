// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Core.IO;
    using Cake.Frosting;
    using Cake.Json;
    using Dnn.CakeUtils;

    /// <summary>A cake task to prepare for packaging (by building the platform and copying files).</summary>
    [IsDependentOn(typeof(CopyWebsite))]
    [IsDependentOn(typeof(Build))]
    [IsDependentOn(typeof(CopyWebsiteBinFolder))]
    public sealed class PreparePackaging : FrostingTask<Context>
    {
        private static readonly string[] SampleModuleArtifactsPattern = ["SampleModules/*.zip",];
        private static readonly string[] BinFolderInclude = ["bin/**/*.dll",];

        /// <inheritdoc />
        public override void Run(Context context)
        {
            context.PackagingPatterns = context.DeserializeJsonFromFile<PackagingPatterns>("./Build/Tasks/packaging.json");

            // Various fixes
            context.CopyFile(
                "./DNN Platform/Library/bin/PetaPoco.dll",
                context.WebsiteDir + context.File("bin/PetaPoco.dll"));

            if (context.Settings.CopySampleProjects)
            {
                context.Information("Copying Sample Projects to Temp Folder");
                var files = context.GetFilesByPatterns(context.ArtifactsDir, SampleModuleArtifactsPattern);
                foreach (var file in files)
                {
                    var destination = context.WebsiteDir + context.Directory("Install") + context.Directory("Module") + file.GetFilename();
                    context.CopyFile(file, destination);
                    context.Information($"  Copied {file.GetFilename()} to {destination}");
                }
            }

            CreateWebConfig(context);
        }

        private static void CreateWebConfig(Context context)
        {
            var configFile = context.WebsiteDir + context.File("release.config");
            var doc = XDocument.Load(configFile);
            XNamespace asm = "urn:schemas-microsoft-com:asm.v1";
            var assemblyBinding = doc.Element("configuration")?.Element("runtime")?.Element(asm + "assemblyBinding");
            if (assemblyBinding == null)
            {
                throw new InvalidOperationException("Could not find configuration/runtime/assemblyBinding in release.config.");
            }

            context.PackagingPatterns = context.DeserializeJsonFromFile<PackagingPatterns>("./Build/Tasks/packaging.json");
            var files = context.GetFilesByPatterns(context.WebsiteDir, BinFolderInclude, context.PackagingPatterns.InstallExclude);
            var redirects = BuildBindingRedirects(files);
            assemblyBinding.Add(redirects.ToArray<object>());

            // save XML document to target file
            var targetFile = context.WebsiteDir + context.File("web.config");
            doc.Save(targetFile);
        }

        private static List<XElement> BuildBindingRedirects(IEnumerable<FilePath> files)
        {
            XNamespace asm = "urn:schemas-microsoft-com:asm.v1";
            var redirects = new List<XElement>();

            foreach (var file in files)
            {
                AssemblyName assemblyName;
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName(file.FullPath);
                }
                catch
                {
                    continue;
                }

                var tokenBytes = assemblyName.GetPublicKeyToken();
                if (tokenBytes == null || tokenBytes.Length == 0 || string.IsNullOrEmpty(assemblyName.Name) || assemblyName.Version == null)
                {
                    continue;
                }

                var token = string.Concat(tokenBytes.Select(static b => b.ToString("x2", CultureInfo.InvariantCulture)));
                redirects.Add(
                    new XElement(
                        asm + "dependentAssembly",
                        new XElement(
                            asm + "assemblyIdentity",
                            new XAttribute("name", assemblyName.Name),
                            new XAttribute("publicKeyToken", token)),
                        new XElement(
                            asm + "bindingRedirect",
                            new XAttribute("oldVersion", "0.0.0.0-32767.32767.32767.32767"),
                            new XAttribute("newVersion", assemblyName.Version.ToString()))));
            }

            return redirects;
        }
    }
}
