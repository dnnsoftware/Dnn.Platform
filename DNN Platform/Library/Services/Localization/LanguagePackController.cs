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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Services.Localization
{
    public class LanguagePackController
    {
        public static void DeleteLanguagePack(LanguagePackInfo languagePack)
        {
            // fix DNN-26330	 Removing a language pack extension removes the language
            // we should not delete language when deleting language pack, as there is just a loose relationship

            //if (languagePack.PackageType == LanguagePackType.Core)
            //{
            //    Locale language = LocaleController.Instance.GetLocale(languagePack.LanguageID);
            //    if (language != null)
            //    {
            //        Localization.DeleteLanguage(language);
            //    }
            //}

            DataProvider.Instance().DeleteLanguagePack(languagePack.LanguagePackID);
            EventLogController.Instance.AddLog(languagePack, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LANGUAGEPACK_DELETED);
        }

        public static LanguagePackInfo GetLanguagePackByPackage(int packageID)
        {
            return CBO.FillObject<LanguagePackInfo>(DataProvider.Instance().GetLanguagePackByPackage(packageID));
        }

        public static void SaveLanguagePack(LanguagePackInfo languagePack)
        {
            if (languagePack.LanguagePackID == Null.NullInteger)
            {
				//Add Language Pack
                languagePack.LanguagePackID = DataProvider.Instance().AddLanguagePack(languagePack.PackageID,
                                                                                      languagePack.LanguageID,
                                                                                      languagePack.DependentPackageID,
                                                                                      UserController.Instance.GetCurrentUserInfo().UserID);
                EventLogController.Instance.AddLog(languagePack, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LANGUAGEPACK_CREATED);
            }
            else
            {
				//Update LanguagePack
                DataProvider.Instance().UpdateLanguagePack(languagePack.LanguagePackID,
                                                           languagePack.PackageID,
                                                           languagePack.LanguageID,
                                                           languagePack.DependentPackageID,
                                                           UserController.Instance.GetCurrentUserInfo().UserID);
                EventLogController.Instance.AddLog(languagePack, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LANGUAGEPACK_UPDATED);
            }
        }
    }
}