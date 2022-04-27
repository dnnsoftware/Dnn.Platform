// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    using System;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;

    /// <inheritdoc/>
    internal class RemoveTelerikBindingRedirectsStep : XmlStepBase, IRemoveTelerikBindingRedirectsStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveTelerikBindingRedirectsStep"/> class.
        /// </summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public RemoveTelerikBindingRedirectsStep(ILoggerSource loggerSource, IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, applicationStatusInfo)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Remove Telerik assembly binding redirects";

        /// <inheritdoc/>
        [Required]
        public override string RelativeFilePath => "Web.config";

        /// <inheritdoc/>
        protected override void ConfigureNamespaceManager()
        {
            this.NamespaceManager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
        }

        /// <inheritdoc/>
        protected override void ProcessXml(XmlDocument doc)
        {
            const string AssemblyBindingPath = "/configuration/runtime/asm:assemblyBinding";

            this.Success = true;

            var assemblyBinding = doc.SelectSingleNode(AssemblyBindingPath, this.NamespaceManager);
            if (assemblyBinding is null)
            {
                this.Notes = $"{AssemblyBindingPath} section not found.";
                return;
            }

            var matchCount = 0;

            assemblyBinding.SelectNodes("asm:dependentAssembly", this.NamespaceManager)
                .Cast<XmlElement>()
                .Select(e => new
                {
                    dependentAssembly = e,
                    assemblyIdentity = (XmlElement)e.SelectSingleNode("asm:assemblyIdentity", this.NamespaceManager),
                })
                .Where(x => x.assemblyIdentity != null)
                .Select(x => new { x.dependentAssembly, Name = $"{x.assemblyIdentity.GetAttribute("name")}" })
                .Where(x => x.Name.IndexOf("telerik", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => x.dependentAssembly)
                .ToList()
                .ForEach(a =>
                {
                    assemblyBinding.RemoveChild(a);
                    matchCount++;
                });

            this.Notes = matchCount > 0 ? $"{matchCount} matches found." : "No matches found.";
        }
    }
}
