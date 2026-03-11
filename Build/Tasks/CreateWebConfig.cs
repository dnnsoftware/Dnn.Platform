// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

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
            var configFile = context.WebsiteFolder + "release.config";
            var doc = System.Xml.Linq.XDocument.Load(configFile);

            context.PackagingPatterns = context.DeserializeJsonFromFile<PackagingPatterns>("./Build/Tasks/packaging.json");
            var files = context.GetFilesByPatterns(context.WebsiteFolder, BinFolderInclude, context.PackagingPatterns.InstallExclude);
            var redirects = files.ParseAssemblies()
                .Where(x => x.PublicKeyToken != "null" && !string.IsNullOrEmpty(x.PublicKeyToken))
                .Where(x => !x.Name.StartsWith("DotNetNuke.", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Name.StartsWith("Dnn.", StringComparison.OrdinalIgnoreCase))
                .Select(assembly => ToElement(assembly.AssemblyBindingRedirect()))
                .ToList();

            // add assembly binding redirects to configuration/runtime/assemblyBinding
            var assemblyBinding = doc.Descendants().FirstOrDefault(element => element.Name.LocalName == "assemblyBinding");
            if (assemblyBinding == null)
            {
                throw new InvalidOperationException("Could not find configuration/runtime/assemblyBinding in release.config.");
            }

            var assemblyBindingNamespace = assemblyBinding.Name.Namespace;
            foreach (var redirect in redirects)
            {
                assemblyBinding.Add(WithNamespace(redirect, assemblyBindingNamespace));
            }

            var targetFile = context.WebsiteFolder + "web.config";

            // save xml document to target file
            doc.Save(targetFile);
        }

        private static XElement ToElement(string xml)
        {
            return XElement.Parse(xml);
        }

        private static XElement WithNamespace(XElement element, XNamespace targetNamespace)
        {
            if (targetNamespace == XNamespace.None)
            {
                return new XElement(element);
            }

            return new XElement(
                targetNamespace + element.Name.LocalName,
                element.Attributes(),
                element.Nodes().Select(node =>
                    node is XElement childElement
                        ? WithNamespace(childElement, targetNamespace)
                        : node));
        }
    }
}
