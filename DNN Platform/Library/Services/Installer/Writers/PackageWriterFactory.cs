// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
