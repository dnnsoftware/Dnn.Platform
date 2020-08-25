// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Log.EventLog;

    public class LanguagePackController
    {
        public static void DeleteLanguagePack(LanguagePackInfo languagePack)
        {
            // fix DNN-26330     Removing a language pack extension removes the language
            // we should not delete language when deleting language pack, as there is just a loose relationship

            // if (languagePack.PackageType == LanguagePackType.Core)
            // {
            //    Locale language = LocaleController.Instance.GetLocale(languagePack.LanguageID);
            //    if (language != null)
            //    {
            //        Localization.DeleteLanguage(language);
            //    }
            // }
            DataProvider.Instance().DeleteLanguagePack(languagePack.LanguagePackID);
            EventLogController.Instance.AddLog(languagePack, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.LANGUAGEPACK_DELETED);
        }

        public static LanguagePackInfo GetLanguagePackByPackage(int packageID)
        {
            return CBO.FillObject<LanguagePackInfo>(DataProvider.Instance().GetLanguagePackByPackage(packageID));
        }

        public static void SaveLanguagePack(LanguagePackInfo languagePack)
        {
            if (languagePack.LanguagePackID == Null.NullInteger)
            {
                // Add Language Pack
                languagePack.LanguagePackID = DataProvider.Instance().AddLanguagePack(
                    languagePack.PackageID,
                    languagePack.LanguageID,
                    languagePack.DependentPackageID,
                    UserController.Instance.GetCurrentUserInfo().UserID);
                EventLogController.Instance.AddLog(languagePack, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.LANGUAGEPACK_CREATED);
            }
            else
            {
                // Update LanguagePack
                DataProvider.Instance().UpdateLanguagePack(
                    languagePack.LanguagePackID,
                    languagePack.PackageID,
                    languagePack.LanguageID,
                    languagePack.DependentPackageID,
                    UserController.Instance.GetCurrentUserInfo().UserID);
                EventLogController.Instance.AddLog(languagePack, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.LANGUAGEPACK_UPDATED);
            }
        }
    }
}
