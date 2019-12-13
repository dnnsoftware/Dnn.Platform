﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.IO;
using System.Xml;

using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The WidgetPackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class WidgetPackageWriter : PackageWriterBase
    {
		#region "Constructors"
		
        public WidgetPackageWriter(PackageInfo package) : base(package)
        {
            string company = package.Name;
            if(company.Contains("."))
            {
                company = company.Substring(0, company.IndexOf("."));
            }

            BasePath = Path.Combine("Resources\\Widgets\\User", company);
        }
		
		#endregion

		#region "Public Properties"

        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }
		
		#endregion

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			//Call base class method with includeAppCode = false
            base.GetFiles(includeSource, false);
        }

        protected override void WriteFilesToManifest(XmlWriter writer)
        {
            string company = Package.Name.Substring(0, Package.Name.IndexOf("."));
            var widgetFileWriter = new WidgetComponentWriter(company, Files, Package);
            widgetFileWriter.WriteManifest(writer);
        }
    }
}
