using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Installer.Packages;

namespace DotNetNuke.Providers.FolderProviders.Components
{
    public class BusinessController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "09.02.00":
                    var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.Providers.FolderProviders");
                    if (package != null)
                    {
                        DataProvider.Instance().UnRegisterAssembly(package.PackageID, "Microsoft.WindowsAzure.StorageClient.dll");
                    }
                    break;
            }

            return "Success";
        }
    }
}
