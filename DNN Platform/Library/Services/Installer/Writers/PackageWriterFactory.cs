// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Installer.Packages;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The PackageWriterFactory is a factory class that is used to instantiate the appropriate Package Writer.</summary>
    public partial class PackageWriterFactory
    {
        /// <summary>The GetWriter method instantiates the relevant PackageWriter Installer.</summary>
        /// <param name="package">The associated PackageInfo instance.</param>
        /// <returns>A <see cref="PackageWriterBase"/> instance or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IServiceProvider")]
        public static partial PackageWriterBase GetWriter(PackageInfo package)
            => GetWriter(Globals.GetCurrentServiceProvider(), package);

        /// <summary>The GetWriter method instantiates the relevant PackageWriter Installer.</summary>
        /// <param name="serviceProvider">The dependency injection container from which a custom writer should be created.</param>
        /// <param name="package">The associated PackageInfo instance.</param>
        /// <returns>A <see cref="PackageWriterBase"/> instance or <see langword="null"/>.</returns>
        public static PackageWriterBase GetWriter(IServiceProvider serviceProvider, PackageInfo package)
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
                    // PackageType is defined in the List
                    var listController = ActivatorUtilities.GetServiceOrCreateInstance<ListController>(serviceProvider);
                    ListEntryInfo entry = listController.GetListEntryInfo("PackageWriter", package.PackageType);

                    if (entry != null && !string.IsNullOrEmpty(entry.Text))
                    {
                        // The class for the Installer is specified in the Text property
                        writer = (PackageWriterBase)Reflection.CreateObject(serviceProvider, entry.Text, $"PackageWriter_{entry.Value}");
                    }

                    break;
            }

            return writer;
        }
    }
}
