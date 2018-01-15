#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Common.Lists;
using DotNetNuke.Framework;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageWriterFactory is a factory class that is used to instantiate the
    /// appropriate Package Writer
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PackageWriterFactory
    {
		#region "Public Shared Methods"
		
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetWriter method instantiates the relevant PackageWriter Installer
        /// </summary>
        /// <param name="package">The associated PackageInfo instance</param>
        /// -----------------------------------------------------------------------------
        public static PackageWriterBase GetWriter(PackageInfo package)
        {
            PackageWriterBase writer = null;
            switch (package.PackageType)
            {
                case "Auth_System":
                    writer = new AuthenticationPackageWriter(package);
                    break;
                case "Module":
                    writer = new ModulePackageWriter(package);
                    break;
                case "Container":
                    writer = new ContainerPackageWriter(package);
                    break;
                case "Skin":
                    writer = new SkinPackageWriter(package);
                    break;
                case "CoreLanguagePack":
                case "ExtensionLanguagePack":
                    writer = new LanguagePackWriter(package);
                    break;
                case "SkinObject":
                    writer = new SkinControlPackageWriter(package);
                    break;
                case "Provider":
                    writer = new ProviderPackageWriter(package);
                    break;
                case "Library":
                    writer = new LibraryPackageWriter(package);
                    break;
                case "Widget":
                    writer = new WidgetPackageWriter(package);
                    break;
                default:
                    //PackageType is defined in the List
                    var listController = new ListController();
                    ListEntryInfo entry = listController.GetListEntryInfo("PackageWriter", package.PackageType);

                    if (entry != null && !string.IsNullOrEmpty(entry.Text))
                    {
						//The class for the Installer is specified in the Text property
                        writer = (PackageWriterBase) Reflection.CreateObject(entry.Text, "PackageWriter_" + entry.Value);
                    }
                    break;
            }
            return writer;
        }
		
		#endregion
    }
}
