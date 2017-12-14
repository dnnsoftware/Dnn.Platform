#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
