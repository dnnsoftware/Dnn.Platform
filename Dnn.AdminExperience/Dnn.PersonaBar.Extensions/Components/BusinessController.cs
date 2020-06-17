// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;

    using Dnn.PersonaBar.Library.Repository;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

    public class BusinessController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.04.00":
                    this.UpdateMenuController();
                    break;

                case "01.05.00":
                    if (this.TelerikAssemblyExists())
                    {
                        UpdateTelerikEncryptionKey("Telerik.Web.UI.DialogParametersEncryptionKey");
                    }
                    break;
            }

            return String.Empty;
        }

        private static string UpdateTelerikEncryptionKey(string keyName)
        {
            var strError = "";
            var currentKey = Config.GetSetting(keyName);
            if (string.IsNullOrEmpty(currentKey) || currentKey.Length < 40)
            {
                try
                {
                    //open the web.config
                    var xmlConfig = Config.Load();

                    //save the current config file
                    Config.BackupConfig();

                    //create a random Telerik encryption key and add it under <appSettings>
                    var newKey = new PortalSecurity().CreateKey(32);
                    newKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(newKey));
                    Config.AddAppSetting(xmlConfig, keyName, newKey);

                    //save a copy of the exitsing web.config
                    var backupFolder = string.Concat(Globals.glbConfigFolder, "Backup_", DateTime.Now.ToString("yyyyMMddHHmm"), "\\");
                    strError += Config.Save(xmlConfig, backupFolder + "web_.config") + Environment.NewLine;

                    //save the web.config
                    strError += Config.Save(xmlConfig) + Environment.NewLine;
                }
                catch (Exception ex)
                {
                    strError += ex.Message;
                }
            }
            return strError;
        }

        private void UpdateMenuController()
        {
            PersonaBarRepository.Instance.UpdateMenuController(Dnn.PersonaBar.Vocabularies.Components.Constants.MenuIdentifier, string.Empty);
        }

        private bool TelerikAssemblyExists()
        {
            return File.Exists(Path.Combine(Globals.ApplicationMapPath, "bin\\Telerik.Web.UI.dll"));
        }
    }
}
