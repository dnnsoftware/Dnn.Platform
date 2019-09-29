using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [JsonObject]
    public class DnnModuleDto
    {
        public string ModuleTitle { get; set; }
        public string CultureCode { get; set; }
        public Guid DefaultLanguageGuid { get; set; }
        public int TabId { get; set; }
        public int TabModuleId { get; set; }
        public int ModuleId { get; set; }
        public int DefaultModuleId { get; set; }
        public string DefaultTabName { get; set; }
        public string ModuleInfoHelp { get; set; }
        public bool IsTranslated { get; set; }
        public bool IsLocalized { get; set; }
        public bool IsShared { get; set; }
        public bool IsDeleted { get; set; }
        public bool CopyModule { get; set; }
        public bool ErrorDuplicateModule { get; set; }
        public bool ErrorDefaultOnOtherTab { get; set; }
        public bool ErrorCultureOfModuleNotCultureOfTab { get; set; }

        [JsonIgnore]
        public string LocalResourceFile { get; set; }

        [JsonIgnore]
        public bool CanViewModule { get; set; }

        [JsonIgnore]
        public bool CanAdminModule { get; set; }

        public bool TranslatedVisible => !ErrorVisible && CultureCode != null
                                         && DefaultLanguageGuid != Null.NullGuid && ModuleId != DefaultModuleId;

        public bool LocalizedVisible => !ErrorVisible && CultureCode != null && DefaultLanguageGuid != Null.NullGuid;

        public bool Exist => TabModuleId > 0;

        public void SetModuleInfoHelp()
        {
            var returnValue = "";
            var moduleInfo = ModuleController.Instance.GetModule(ModuleId, Null.NullInteger, true);
            if (moduleInfo != null)
            {
                if (moduleInfo.IsDeleted)
                {
                    returnValue = LocalizeString("ModuleDeleted.Text");
                }
                else
                {
                    returnValue = ModulePermissionController.CanAdminModule(moduleInfo)
                        ? string.Format(LocalizeString("ModuleInfo.Text"),
                                moduleInfo.ModuleDefinition.FriendlyName, moduleInfo.ModuleTitle, moduleInfo.PaneName)
                        : LocalizeString("ModuleInfoForNonAdmins.Text");
                }
            }

            ModuleInfoHelp = returnValue;
        }

        public string TranslatedTooltip
        {
            get
            {
                if (CultureCode == null)
                    return "";
                if (DefaultLanguageGuid == Null.NullGuid)
                {
                    return "";
                }
                var pageName = "";
                if (DefaultTabName != null)
                    pageName = " / " + DefaultTabName;

                if (ModuleId == DefaultModuleId)
                {
                    return string.Format(LocalizeString("Reference.Text"), pageName);
                }
                if (IsTranslated)
                {
                    return string.Format(LocalizeString("Translated.Text"), pageName);

                }
                return string.Format(LocalizeString("NotTranslated.Text"), pageName);
            }
        }

        public string LocalizedTooltip
        {
            get
            {
                if (CultureCode == null || DefaultLanguageGuid == Null.NullGuid)
                {
                    return "";
                }

                var pageName = "";
                if (DefaultTabName != null)
                    pageName = " / " + DefaultTabName;

                if (ModuleId == DefaultModuleId)
                {
                    return string.Format(LocalizeString("ReferenceDefault.Text"), pageName);
                }

                return string.Format(LocalizeString("Detached.Text"), pageName);
            }
        }

        public bool ErrorVisible => ErrorDefaultOnOtherTab || ErrorCultureOfModuleNotCultureOfTab || ErrorDuplicateModule;

        public string ErrorToolTip
        {
            get
            {
                if (ErrorDefaultOnOtherTab)
                {
                    return "Default module on other tab";
                }

                if (ErrorCultureOfModuleNotCultureOfTab)
                {
                    return "Culture of module # culture of tab";
                }

                if (ErrorDuplicateModule)
                {
                    return "Duplicate module";
                }

                return "";
            }
        }

        private string LocalizeString(string localizationKey)
        {
            return Localization.GetString(localizationKey, LocalResourceFile) ?? "";
        }
    }
}