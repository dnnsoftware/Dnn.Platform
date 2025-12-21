// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc/>
    internal abstract class XmlStepBase : StepBase, IXmlStep
    {
        private readonly IApplicationStatusInfo applicationStatusInfo;

        /// <summary>Initializes a new instance of the <see cref="XmlStepBase"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public XmlStepBase(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, localizer)
        {
            this.applicationStatusInfo = applicationStatusInfo ??
                throw new ArgumentNullException(nameof(applicationStatusInfo));
        }

        /// <inheritdoc/>
        [Required]
        public virtual string RelativeFilePath { get; set; }

        /// <summary>Gets the <see cref="XmlNamespaceManager"/> instance associated with the XML document.</summary>
        protected XmlNamespaceManager NamespaceManager { get; private set; }

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            var root = this.applicationStatusInfo.ApplicationMapPath + @"\";
            var fullPath = Path.GetFullPath(Path.Combine(root, this.RelativeFilePath));
            doc.Load(fullPath);

            this.NamespaceManager = new XmlNamespaceManager(doc.NameTable);
            this.ConfigureNamespaceManager();

            this.ProcessXml(doc);

            if (this.Success.HasValue && this.Success.Value)
            {
                doc.Save(fullPath);
            }
        }

        /// <summary>Provides an opportunity for derived classes to configure the namespace manager.</summary>
        protected virtual void ConfigureNamespaceManager()
        {
        }

        /// <summary>Performs the actual processing of the XML document.</summary>
        /// <param name="doc">The XML document to process.</param>
        protected abstract void ProcessXml(XmlDocument doc);
    }
}
