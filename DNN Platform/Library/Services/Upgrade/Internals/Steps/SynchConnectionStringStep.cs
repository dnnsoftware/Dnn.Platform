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
            if (connection.ToLower().Contains("user id") || connection.ToLower().Contains("uid") || connection.ToLower().Contains("user"))
            {
                string[] connectionParams = connection.Split(';');

                foreach (string connectionParam in connectionParams)
                {
                    int index = connectionParam.IndexOf("=");
                    if (index > 0)
                    {
                        string key = connectionParam.Substring(0, index);
                        string value = connectionParam.Substring(index + 1);
                        if ("user id|uuid|user".Contains(key.Trim().ToLower()))
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
