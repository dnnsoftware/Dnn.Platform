// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    [JsonObject]
    public class DnnModuleDto
    {
        public bool TranslatedVisible => !this.ErrorVisible && this.CultureCode != null
                                                            && this.DefaultLanguageGuid != Null.NullGuid && this.ModuleId != this.DefaultModuleId;

        public bool LocalizedVisible => !this.ErrorVisible && this.CultureCode != null && this.DefaultLanguageGuid != Null.NullGuid;

        public bool Exist => this.TabModuleId > 0;

        public string TranslatedTooltip
        {
            get
            {
                if (this.CultureCode == null)
                    return "";
                if (this.DefaultLanguageGuid == Null.NullGuid)
                {
                    return "";
                }
                var pageName = "";
                if (this.DefaultTabName != null)
                    pageName = " / " + this.DefaultTabName;

                if (this.ModuleId == this.DefaultModuleId)
                {
                    return string.Format(this.LocalizeString("Reference.Text"), pageName);
                }
                if (this.IsTranslated)
                {
                    return string.Format(this.LocalizeString("Translated.Text"), pageName);

                }
                return string.Format(this.LocalizeString("NotTranslated.Text"), pageName);
            }
        }

        public string LocalizedTooltip
        {
            get
            {
                if (this.CultureCode == null || this.DefaultLanguageGuid == Null.NullGuid)
                {
                    return "";
                }

                var pageName = "";
                if (this.DefaultTabName != null)
                    pageName = " / " + this.DefaultTabName;

                if (this.ModuleId == this.DefaultModuleId)
                {
                    return string.Format(this.LocalizeString("ReferenceDefault.Text"), pageName);
                }

                return string.Format(this.LocalizeString("Detached.Text"), pageName);
            }
        }

        public bool ErrorVisible => this.ErrorDefaultOnOtherTab || this.ErrorCultureOfModuleNotCultureOfTab || this.ErrorDuplicateModule;

        public string ErrorToolTip
        {
            get
            {
                if (this.ErrorDefaultOnOtherTab)
                {
                    return "Default module on other tab";
                }

                if (this.ErrorCultureOfModuleNotCultureOfTab)
                {
                    return "Culture of module # culture of tab";
                }

                if (this.ErrorDuplicateModule)
                {
                    return "Duplicate module";
                }

                return "";
            }
        }

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

        public void SetModuleInfoHelp()
        {
            var returnValue = "";
            var moduleInfo = ModuleController.Instance.GetModule(this.ModuleId, Null.NullInteger, true);
            if (moduleInfo != null)
            {
                if (moduleInfo.IsDeleted)
                {
                    returnValue = this.LocalizeString("ModuleDeleted.Text");
                }
                else
                {
                    returnValue = ModulePermissionController.CanAdminModule(moduleInfo)
                        ? string.Format(this.LocalizeString("ModuleInfo.Text"),
                                moduleInfo.ModuleDefinition.FriendlyName, moduleInfo.ModuleTitle, moduleInfo.PaneName)
                        : this.LocalizeString("ModuleInfoForNonAdmins.Text");
                }
            }

            this.ModuleInfoHelp = returnValue;
        }

        private string LocalizeString(string localizationKey)
        {
            return Localization.GetString(localizationKey, this.LocalResourceFile) ?? "";
        }
    }
}
