﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallSiteStep - Step that installs Website
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class InstallSiteStep : BaseInstallationStep
    {
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;


            //Set Status to None
            Globals.SetStatus(Globals.UpgradeStatus.None);


            var installConfig = InstallController.Instance.GetInstallConfig();
            var percentForEachStep = 100 / installConfig.Portals.Count;
            var counter = 0;
            foreach (var portal in installConfig.Portals)
            {
                string description = Localization.Localization.GetString("CreatingSite", LocalInstallResourceFile);
                Details = string.Format(description, portal.PortalName);
                CreateSite(portal, installConfig);

                counter++;
                Percentage = percentForEachStep * counter++;
            }

            Globals.ResetAppStartElapseTime();

            Status = StepStatus.Done;
        }

        #endregion

        #region Private Methods

        private void CreateSite(PortalConfig portal, InstallConfig installConfig)
        {

            var domain = "";
            if ((HttpContext.Current != null))
            {
                domain = Globals.GetDomainName(HttpContext.Current.Request, true).ToLowerInvariant().Replace("/install/launchautoinstall", "").Replace("/install", "").Replace("/runinstall", "");
            }

            var serverPath = Globals.ApplicationMapPath + "\\";

            //Get the Portal Alias
            var portalAlias = domain;
            if (portal.PortAliases.Count > 0) portalAlias = portal.PortAliases[0];

            //Verify that portal alias is not present
            if (PortalAliasController.Instance.GetPortalAlias(portalAlias.ToLowerInvariant()) != null)
            {
                string description = Localization.Localization.GetString("SkipCreatingSite", LocalInstallResourceFile);
                Details = string.Format(description, portalAlias);
                return;
            }

            //Create default email
            var email = portal.AdminEmail;
            if (string.IsNullOrEmpty(email))
            {
                email = "admin@" + domain.Replace("www.", "");
                //Remove any domain subfolder information ( if it exists )
                if (email.IndexOf("/") != -1)
                {
                    email = email.Substring(0, email.IndexOf("/"));
                }
            }

            //install LP if installing in a different language
            string culture = installConfig.InstallCulture;
            if (!culture.Equals("en-us", StringComparison.InvariantCultureIgnoreCase))
            {
                string installFolder = HttpContext.Current.Server.MapPath("~/Install/language");
                string lpFilePath = installFolder + "\\installlanguage.resources";

                if (File.Exists(lpFilePath))
                {
                    if (!Upgrade.InstallPackage(lpFilePath, "Language", false))
                    {
                        culture = Localization.Localization.SystemLocale;
                    }
                }
                else
                {
                    culture = Localization.Localization.SystemLocale;
                }
            }

            var template = Upgrade.FindBestTemplate(portal.TemplateFileName, culture);
            UserInfo userInfo;

            if (!String.IsNullOrEmpty(portal.AdminUserName))
                userInfo = Upgrade.CreateUserInfo(portal.AdminFirstName, portal.AdminLastName, portal.AdminUserName, portal.AdminPassword, email);
            else
                userInfo = Upgrade.CreateUserInfo(installConfig.SuperUser.FirstName, installConfig.SuperUser.LastName, installConfig.SuperUser.UserName, installConfig.SuperUser.Password, installConfig.SuperUser.Email);

            var childPath = string.Empty;
            if (portal.IsChild)
                childPath = portalAlias.Substring(portalAlias.LastIndexOf("/") + 1);

            //Create Folder Mappings config
            if (!String.IsNullOrEmpty(installConfig.FolderMappingsSettings))
            {
                FolderMappingsConfigController.Instance.SaveConfig(installConfig.FolderMappingsSettings);
            }
            //add item to identity install from install wizard.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("InstallFromWizard", true);
            }

            //Create Portal
            var portalId = PortalController.Instance.CreatePortal(portal.PortalName,
                                                     userInfo,
                                                     portal.Description,
                                                     portal.Keywords,
                                                     template,
                                                     portal.HomeDirectory,
                                                     portalAlias,
                                                     serverPath,
                                                     serverPath + childPath,
                                                     portal.IsChild);

            if (portalId > -1)
            {
                foreach (var alias in portal.PortAliases)
                {
                    PortalController.Instance.AddPortalAlias(portalId, alias);
                }
            }

            //remove en-US from portal if installing in a different language
            if (!culture.Equals("en-us", StringComparison.InvariantCultureIgnoreCase))
            {
                var locale = LocaleController.Instance.GetLocale("en-US");
                Localization.Localization.RemoveLanguageFromPortal(portalId, locale.LanguageId, true);
            }

            //Log user in to site
            var loginStatus = UserLoginStatus.LOGIN_FAILURE;
            UserController.UserLogin(portalId, userInfo.Username, installConfig.SuperUser.Password, "", "", "", ref loginStatus, false);

            InstallController.Instance.RemoveFromInstallConfig("//dotnetnuke/superuser/password");
        }

        //private void CreateFolderMappingConfig(string folderMappinsSettings)
        //{
        //    string folderMappingsConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DotNetNuke.folderMappings.config");
        //    if (!File.Exists(folderMappingsConfigPath))
        //    {
        //        File.AppendAllText(folderMappingsConfigPath, "<folderMappingsSettings>" + folderMappinsSettings + "</folderMappingsSettings>");
        //    }
        //}

        #endregion

    }
}
