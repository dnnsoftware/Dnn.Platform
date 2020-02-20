// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.Installer.Packages
{
    public interface IPackageEditor
    {
        int PackageID { get; set; }
        bool IsWizard { get; set; }

        void Initialize();

        void UpdatePackage();
    }
}
