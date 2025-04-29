// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System;
using System.Collections;
using System.Collections.Generic;

using Dnn.Modules.ResourceManager.Exceptions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

using static Dnn.Modules.ResourceManager.Components.Constants;

/// <summary>Manages module settings.</summary>
public class SettingsManager
{
    private readonly Hashtable moduleSettingsDictionary;
    private readonly int groupId;

    /// <summary>Initializes a new instance of the <see cref="SettingsManager"/> class.</summary>
    /// <param name="moduleId">The id of the module.</param>
    /// <param name="groupId">The id of the group.</param>
    public SettingsManager(
        int moduleId,
        int groupId)
    {
        this.groupId = groupId;
        var moduleController = new ModuleController();
        var module = moduleController.GetModule(moduleId);

        this.moduleSettingsDictionary = module.ModuleSettings;
        this.LoadSettings();
    }

    /// <summary>Gets or sets the id of the home folder.</summary>
    public int HomeFolderId { get; set; }

    /// <summary>Gets or sets the name of the home folder.</summary>
    public string HomeFolderName { get; set; }

    /// <summary>Gets or sets the module mode, <see cref="Constants.ModuleModes"/>.</summary>
    public int Mode { get; set; }

    /// <summary>Gets or sets a value indicating whether the folder is in the host portal (i.e. <c>_default</c>, "Global Resources"), rather than the current portal.</summary>
    public bool IsHostPortal { get; set; }

    private void LoadSettings()
    {
        this.Mode = this.GetSettingIntValueOrDefault(ModeSettingName, DefaultMode);
        this.ValidateMode(this.Mode);
        this.HomeFolderId = this.GetHomeFolderId(this.Mode);

        var homeFolder = FolderManager.Instance.GetFolder(this.HomeFolderId);
        this.HomeFolderName = this.GetHomeFolderName(homeFolder);
        this.IsHostPortal = homeFolder.PortalID == Null.NullInteger;
    }

    private string GetHomeFolderName(IFolderInfo homeFolder)
    {
        if (homeFolder.PortalID == Null.NullInteger && homeFolder.ParentID == Null.NullInteger)
        {
            return Localization.GetString(
                "GlobalAssets",
                Constants.ViewResourceFileName);
        }

        if (homeFolder.ParentID == Null.NullInteger)
        {
            return Localization.GetString(
                "SiteAssets",
                Constants.ViewResourceFileName);
        }

        if (this.groupId > 0)
        {
            var socialRole = RoleController.Instance.GetRole(homeFolder.PortalID, r => r.RoleID == this.groupId);
            var template = Localization.GetString(
                "GroupAssets",
                Constants.ViewResourceFileName);
            return string.Format(template, socialRole.RoleName);
        }

        if (this.Mode == (int)ModuleModes.User)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            if (user.UserID < 0)
            {
                throw new ModeValidationException("UserModeError");
            }

            var template = Localization.GetString(
                "UserAssets",
                Constants.ViewResourceFileName);
            return string.Format(template, user.DisplayName);
        }

        return homeFolder.FolderName;
    }

    private int GetSettingIntValueOrDefault(string settingName, int defaultValue = -1)
    {
        var settingValue = this.moduleSettingsDictionary.ContainsKey(settingName) ? this.moduleSettingsDictionary[settingName].ToString() : null;

        return string.IsNullOrEmpty(settingValue) ? defaultValue : int.Parse(settingValue);
    }

    private void ValidateMode(int moduleMode)
    {
        switch (moduleMode)
        {
            case (int)ModuleModes.Group:
                if (this.groupId <= 0)
                {
                    throw new ModeValidationException("GroupModeError");
                }

                break;

            case (int)ModuleModes.User:
                var user = UserController.Instance.GetCurrentUserInfo();
                if (user.UserID < 0)
                {
                    throw new ModeValidationException("UserModeError");
                }

                break;
        }
    }

    private int GetHomeFolderId(int moduleMode)
    {
        var portalId = PortalSettings.Current.PortalId;
        var folderId = 0;

        switch (moduleMode)
        {
            case (int)ModuleModes.Group:
                folderId = GroupManager.Instance.FindOrCreateGroupFolder(portalId, this.groupId).FolderID;
                break;

            case (int)ModuleModes.User:
                var user = UserController.Instance.GetCurrentUserInfo();
                folderId = FolderManager.Instance.GetUserFolder(user).FolderID;
                break;

            case (int)ModuleModes.Normal:
                var defaultHomeFolderId = FolderManager.Instance.GetFolder(portalId, string.Empty).FolderID;
                folderId = this.GetSettingIntValueOrDefault(HomeFolderSettingName, defaultHomeFolderId);
                break;
        }

        return folderId;
    }
}
