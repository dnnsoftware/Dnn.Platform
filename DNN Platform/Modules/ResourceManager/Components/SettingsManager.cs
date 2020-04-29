using System.Collections;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using Dnn.Modules.ResourceManager.Exceptions;
using static Dnn.Modules.ResourceManager.Components.Constants;

namespace Dnn.Modules.ResourceManager.Components
{
    public class SettingsManager
    {
        #region Private Properties

        private readonly Hashtable _moduleSettingsDictionary;
        private readonly int _groupId;

        #endregion

        #region Public Properties

        public int HomeFolderId { get; set; }
        public int Mode { get; set; }

        #endregion

        public SettingsManager(int moduleId, int groupId)
        {
            _groupId = groupId;
            var moduleController = new ModuleController();
            var module = moduleController.GetModule(moduleId);

            _moduleSettingsDictionary = module.ModuleSettings;

            LoadSettings();
        }

        #region Private methods

        private void LoadSettings()
        {
            Mode = GetSettingIntValueOrDefault(ModeSettingName, DefaultMode);
            ValidateMode(Mode);
            HomeFolderId = GetHomeFolderId(Mode);
        }

        private int GetSettingIntValueOrDefault(string settingName, int defaultValue = -1)
        {
            var settingValue = _moduleSettingsDictionary.ContainsKey(settingName) ? _moduleSettingsDictionary[settingName].ToString() : null;

            return string.IsNullOrEmpty(settingValue) ? defaultValue : int.Parse(settingValue);
        }

        private void ValidateMode(int moduleMode)
        {
            switch (moduleMode)
            {
                case (int)ModuleModes.Group:
                    if (_groupId <= 0)
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
                    folderId = GroupManager.Instance.FindOrCreateGroupFolder(portalId, _groupId).FolderID;
                    break;

                case (int)ModuleModes.User:
                    var user = UserController.Instance.GetCurrentUserInfo();
                    folderId = FolderManager.Instance.GetUserFolder(user).FolderID;
                    break;

                case (int)ModuleModes.Normal:
                    var defaultHomeFolderId = FolderManager.Instance.GetFolder(portalId, "").FolderID;
                    folderId = GetSettingIntValueOrDefault(HomeFolderSettingName, defaultHomeFolderId);
                    break;
            }

            return folderId;
        }

        #endregion
    }
}