#region Usings

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SynchConnectionStringStep - Step that synchs connection string between DotNetNuke.Install.Config and Web.Config
    /// The connection information stored in DotNetNuke.Install.Config takes precendence (if present)
    /// Synchornization only happens when settings are different
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class SynchConnectionStringStep : BaseInstallationStep
    {
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            var installConfig = InstallController.Instance.GetInstallConfig();
            if(installConfig == null)
            {
                Status = StepStatus.Done;
                return;
            }

            var connectionConfig = installConfig.Connection;
            if (connectionConfig == null)
            {
                Status = StepStatus.Done;
                return;
            }

            if (string.IsNullOrEmpty(connectionConfig.File) && string.IsNullOrEmpty(connectionConfig.Database))
            {                    
                Errors.Add(Localization.Localization.GetString("RequiresFileOrDatabase", LocalInstallResourceFile));
                Status = StepStatus.Abort;
                return;
            }

            var builder = DataProvider.Instance().GetConnectionStringBuilder();
            
            if (!string.IsNullOrEmpty(connectionConfig.Server))
                builder["Data Source"] = connectionConfig.Server;

            if (!string.IsNullOrEmpty(connectionConfig.Database))
                builder["Initial Catalog"] = connectionConfig.Database;
            else if (!string.IsNullOrEmpty(connectionConfig.File))
            {
                builder["attachdbfilename"] = "|DataDirectory|" + connectionConfig.File;
                builder["user instance"] = "true";
            }

            if (connectionConfig.Integrated)
                builder["integrated security"] = "true";

            if (!string.IsNullOrEmpty(connectionConfig.User))
                builder["uid"] = connectionConfig.User;

            if (!string.IsNullOrEmpty(connectionConfig.Password))
                builder["pwd"] = connectionConfig.Password;

            string dbowner;
            if (connectionConfig.RunAsDbowner)
            {
                dbowner = "dbo.";
            }
            else
            {
                dbowner = (string.IsNullOrEmpty(GetUpgradeConnectionStringUserID()))
                                           ? connectionConfig.User + "."
                                           : GetUpgradeConnectionStringUserID();
            }
                
            var connectionString = builder.ToString();

            //load web.config connection string for comparison
            var appConnectionString = Config.GetConnectionString();

            var modified = false;
            //save to web.config if different
            if(appConnectionString.ToLower() != connectionString.ToLower())
            {
                Config.UpdateConnectionString(connectionString);
                modified = true;
            }

            //Compare (and overwrite) Owner and Qualifier in Data Provider
            if (Config.GetDataBaseOwner().ToLower() != dbowner.ToLower() ||
                (Config.GetObjectQualifer().ToLower() != connectionConfig.Qualifier.ToLower()))
            {
                Config.UpdateDataProvider("SqlDataProvider", dbowner, connectionConfig.Qualifier);
                modified = true;
            }

            //Compare (and overwrite) Owner and Qualifier in Data Provider
            if (!string.IsNullOrEmpty(connectionConfig.UpgradeConnectionString) && Config.GetUpgradeConnectionString().ToLower() != connectionConfig.UpgradeConnectionString.ToLower())
            {
                Config.UpdateUpgradeConnectionString("SqlDataProvider", connectionConfig.UpgradeConnectionString);
                modified = true;
            }

            Status = modified ? StepStatus.AppRestart : StepStatus.Done;
        }

        #endregion

        #region Private Methods

        private string GetUpgradeConnectionStringUserID()
        {
            string dbUser = "";
            string connection = Config.GetUpgradeConnectionString();

            //If connection string does not use integrated security, then get user id.
            if (connection.ToLowerInvariant().Contains("user id") || connection.ToLowerInvariant().Contains("uid") || connection.ToLowerInvariant().Contains("user"))
            {
                string[] connectionParams = connection.Split(';');

                foreach (string connectionParam in connectionParams)
                {
                    int index = connectionParam.IndexOf("=");
                    if (index > 0)
                    {
                        string key = connectionParam.Substring(0, index);
                        string value = connectionParam.Substring(index + 1);
                        if ("user id|uuid|user".Contains(key.Trim().ToLowerInvariant()))
                        {
                            dbUser = value.Trim();
                        }
                    }
                }
            }
            return dbUser;
        }

        #endregion
    }
}
