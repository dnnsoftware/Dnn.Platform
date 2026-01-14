// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Services.Installer.Packages;

    /// <summary>The WidgetPackageWriter class.</summary>
    public class WidgetPackageWriter : PackageWriterBase
    {
        /// <summary>Initializes a new instance of the <see cref="WidgetPackageWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public WidgetPackageWriter(PackageInfo package)
            : base(package)
        {
            string company = package.Name;
            if (company.Contains(".", StringComparison.Ordinal))
            {
                company = company.Substring(0, company.IndexOf(".", StringComparison.Ordinal));
            }

            this.BasePath = Path.Combine(@"Resources\Widgets\User", company);
        }

        /// <inheritdoc/>
        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
            // Call base class method with includeAppCode = false
            base.GetFiles(includeSource, false);
        }

        /// <inheritdoc/>
        protected override void WriteFilesToManifest(XmlWriter writer)
        {
            string company = this.Package.Name.Substring(0, this.Package.Name.IndexOf(".", StringComparison.Ordinal));
            var widgetFileWriter = new WidgetComponentWriter(company, this.Files, this.Package);
            widgetFileWriter.WriteManifest(writer);
        }
    }
}
