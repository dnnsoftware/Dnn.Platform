// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Frosting;
    using Cake.Json;
    using Dnn.CakeUtils;

    /// <summary>A cake task to copy the <c>release.config</c> to the <c>web.config</c>.</summary>
    public sealed class CreateWebConfig : FrostingTask<Context>
    {
        private static readonly string[] BinFolderInclude = ["bin/**/*.dll",];

        /// <inheritdoc />
        public override void Run(Context context)
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
            var files = context.GetFilesByPatterns(context.WebsiteFolder, BinFolderInclude, context.PackagingPatterns.InstallExclude);
            var parsedAssemblies = files.ParseAssemblies();
            parsedAssemblies.RemoveAll(a => a.PublicKeyToken is null);
            var redirects = parsedAssemblies.ConvertAll(a => a.AssemblyBindingRedirect());
            assemblyBinding.Add(redirects.ToArray<object>());

            // save XML document to target file
            var targetFile = context.WebsiteDir + context.File("web.config");
            doc.Save(targetFile);
        }
    }
}
