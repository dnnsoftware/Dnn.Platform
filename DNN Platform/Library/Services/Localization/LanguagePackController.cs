// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A controller for language packs.</summary>
    public partial class LanguagePackController
    {
        /// <summary>Delete the specified language pack.</summary>
        /// <param name="languagePack">The language pack.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteLanguagePack(LanguagePackInfo languagePack)
            => DeleteLanguagePack(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), languagePack);

        /// <summary>Delete the specified language pack.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="languagePack">The language pack.</param>
        public static void DeleteLanguagePack(IEventLogger eventLogger, LanguagePackInfo languagePack)
        {
            // fix DNN-26330     Removing a language pack extension removes the language
            // we should not delete language when deleting language pack, as there is just a loose relationship

            ////if (languagePack.PackageType == LanguagePackType.Core)
            ////{
            ////   Locale language = LocaleController.Instance.GetLocale(languagePack.LanguageID);
            ////   if (language != null)
            ////   {
            ////       Localization.DeleteLanguage(language);
            ////   }
            ////}
            DataProvider.Instance().DeleteLanguagePack(languagePack.LanguagePackID);
            eventLogger.AddLog(languagePack, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.LANGUAGEPACK_DELETED);
        }

        public static LanguagePackInfo GetLanguagePackByPackage(int packageID)
        {
            return CBO.FillObject<LanguagePackInfo>(DataProvider.Instance().GetLanguagePackByPackage(packageID));
        }

        /// <summary>Add or update a language pack.</summary>
        /// <param name="languagePack">The language pack.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void SaveLanguagePack(LanguagePackInfo languagePack)
            => SaveLanguagePack(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), languagePack);

        /// <summary>Add or update a language pack.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="languagePack">The language pack.</param>
        public static void SaveLanguagePack(IEventLogger eventLogger, LanguagePackInfo languagePack)
        {
            if (languagePack.LanguagePackID == Null.NullInteger)
            {
                // Add Language Pack
                languagePack.LanguagePackID = DataProvider.Instance().AddLanguagePack(
                    languagePack.PackageID,
                    languagePack.LanguageID,
                    languagePack.DependentPackageID,
                    UserController.Instance.GetCurrentUserInfo().UserID);
                eventLogger.AddLog(languagePack, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.LANGUAGEPACK_CREATED);
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
                eventLogger.AddLog(languagePack, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.LANGUAGEPACK_UPDATED);
            }
        }
    }
}
