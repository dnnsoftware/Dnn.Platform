using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;
using Microsoft.JScript;
using Globals = DotNetNuke.Common.Globals;

namespace DNNConnect.CKEditorProvider.Components
{
    public class UpgradeController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.00.02":
                    UpdateProviderConfig("01.00.02");
                    break;
            }

            return "Success";
        }

        private void UpdateProviderConfig(string version)
        {
            var targetConfig = Config.Load();
            AddProviderConfig(targetConfig, "Install", version);

            if (!IsUpgradeMode())
            {
                AddProviderConfig(targetConfig, "DefaultProvider", version);
            }

            Config.Save(targetConfig);
        }

        private bool IsUpgradeMode()
        {
            var defaultEditorName = "DNNConnect.CKE";
            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            if (string.IsNullOrEmpty(providerConfiguration.DefaultProvider) 
                    || providerConfiguration.DefaultProvider == defaultEditorName
                    || providerConfiguration.Providers.Count == 0)
            {
                return false;
            }

            try
            {
                var settingsControlPath = Path.Combine(Globals.ApplicationMapPath, "Providers/HtmlEditorProviders/DNNConnect.CKE/Module/EditorConfigManager.ascx");
                var webConfigPath = Path.Combine(Globals.ApplicationMapPath, "web.config");
                var defaultFileModificationTime = File.GetLastWriteTime(settingsControlPath);
                var webConfigFileModificationTime = File.GetLastWriteTime(webConfigPath);

                if (Math.Abs((defaultFileModificationTime - webConfigFileModificationTime).Seconds) < 120)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private void AddProviderConfig(XmlDocument targetConfig, string fileName, string version)
        {
            try
            {
                var configFile = string.Format("Providers/HtmlEditorProviders/DNNConnect.CKE/Config/{0}.config", fileName);
                var installConfigFile = Path.Combine(Globals.ApplicationMapPath, configFile);
                var installConfig = File.ReadAllText(installConfigFile);

                var merge = new XmlMerge(new StringReader(installConfig), version, "DNNConnect.CKE");

                merge.UpdateConfig(targetConfig);
            }
            catch (Exception)
            {
                //do nothing here.
            }

        }
    }
}