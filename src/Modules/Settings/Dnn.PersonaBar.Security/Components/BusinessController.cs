using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

namespace Dnn.PersonaBar.Security.Components
{
    public class BusinessController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.05.00":
                    if (TelerikAssemblyExists())
                    {
                        UpdateTelerikEncryptionKey("Telerik.Web.UI.DialogParametersEncryptionKey");
                    }
                    break;
            }

            return String.Empty;
        }

        private bool TelerikAssemblyExists()
        {
            return File.Exists(Path.Combine(Globals.ApplicationMapPath, "bin\\Telerik.Web.UI.dll"));
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
    }
}