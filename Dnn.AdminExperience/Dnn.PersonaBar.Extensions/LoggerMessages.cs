// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar;

using System;
using System.Data.SqlClient;
using System.IO;
using System.Web.Security;

using Dnn.PersonaBar.Roles.Components.Prompt.Exceptions;

using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Connections;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The Dnn.PersonaBar.Extensions project has been assigned event IDs from 7,000,000 to 7,999,999.</remarks>
internal static partial class LoggerMessages
{
    /*
    # Event IDs
    - 7,000,000 to 7,999,999
      - Dnn.PersonaBar.Extensions project
        - 7,000,000 to 7,099,999
          - Dnn.PersonaBar.Extensions namespace
        - 7,200,000 to 7,204,999
          - Dnn.PersonaBar.AdminLogs.Components namespace
          - Dnn.PersonaBar.AdminLogs.MenuControllers namespace
        - 7,205,000 to 7,209,999
          - Dnn.PersonaBar.AdminLogs.Services namespace
          - Dnn.PersonaBar.AdminLogs.Services.Dto namespace
        - 7,210,000 to 7,214,999
          - Dnn.PersonaBar.ConfigConsole.Components namespace
          - Dnn.PersonaBar.ConfigConsole.MenuControllers namespace
        - 7,215,000 to 7,219,999
          - Dnn.PersonaBar.ConfigConsole.Services namespace
          - Dnn.PersonaBar.ConfigConsole.Services.Dto namespace
        - 7,220,000 to 7,224,999
          - Dnn.PersonaBar.Connectors.Components namespace
        - 7,225,000 to 7,229,999
          - Dnn.PersonaBar.Connectors.Services namespace
        - 7,230,000 to 7,234,999
          - Dnn.PersonaBar.CssEditor.MenuControllers namespace
        - 7,235,000 to 7,239,999
          - Dnn.PersonaBar.CssEditor.Services namespace
          - Dnn.PersonaBar.CssEditor.Services.Dto namespace
        - 7,240,000 to 7,244,999
          - Dnn.PersonaBar.CssEditor.Components namespace
          - Dnn.PersonaBar.CssEditor.MenuControllers namespace
        - 7,245,000 to 7,249,999
          - Dnn.PersonaBar.CssEditor.Services namespace
          - Dnn.PersonaBar.CssEditor.Services.Dto namespace
        - 7,250,000 to 7,254,999
          - Dnn.PersonaBar.Extensions.Components namespace
          - Dnn.PersonaBar.Extensions.Components.Dto namespace
          - Dnn.PersonaBar.Extensions.Components.Dto.Editors namespace
          - Dnn.PersonaBar.Extensions.Components.Editors namespace
          - Dnn.PersonaBar.Extensions.MenuControllers namespace
        - 7,255,000 to 7,259,999
          - Dnn.PersonaBar.Extensions.Services namespace
          - Dnn.PersonaBar.Extensions.Services.Dto namespace
        - 7,260,000 to 7,264,999
          - Dnn.PersonaBar.Licensing.MenuControllers namespace
        - 7,265,000 to 7,269,999
          - Dnn.PersonaBar.Licensing.Services namespace
        - 7,270,000 to 7,274,999
          - Dnn.PersonaBar.Pages.Components namespace
          - Dnn.PersonaBar.Pages.Components.Dto namespace
          - Dnn.PersonaBar.Pages.Components.Exceptions namespace
          - Dnn.PersonaBar.Pages.Components.Prompt.Commands namespace
          - Dnn.PersonaBar.Pages.Components.Prompt.Models namespace
          - Dnn.PersonaBar.Pages.Components.Security namespace
          - Dnn.PersonaBar.Pages.MenuControllers namespace
        - 7,275,000 to 7,279,999
          - Dnn.PersonaBar.Pages.Services namespace
          - Dnn.PersonaBar.Pages.Services.Dto namespace
        - 7,280,000 to 7,284,999
          - Dnn.PersonaBar.Prompt.Common namespace
          - Dnn.PersonaBar.Prompt.Components namespace
          - Dnn.PersonaBar.Prompt.Components.Commands.Application namespace
          - Dnn.PersonaBar.Prompt.Components.Commands.Client namespace
          - Dnn.PersonaBar.Prompt.Components.Commands.Commands namespace
          - Dnn.PersonaBar.Prompt.Components.Commands.Host namespace
          - Dnn.PersonaBar.Prompt.Components.Commands.Module namespace
          - Dnn.PersonaBar.Prompt.Components.Commands.Portal namespace
          - Dnn.PersonaBar.Prompt.Components.Commands.Utilities namespace
          - Dnn.PersonaBar.Prompt.Components.Models namespace
          - Dnn.PersonaBar.Prompt.Components.Repositories namespace
          - Dnn.PersonaBar.Prompt.MenuControllers namespace
        - 7,285,000 to 7,289,999
          - Dnn.PersonaBar.Prompt.Services namespace
        - 7,290,000 to 7,294,999
          - Dnn.PersonaBar.Recyclebin.Components namespace
          - Dnn.PersonaBar.Recyclebin.Components.Dto namespace
          - Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands namespace
        - 7,295,000 to 7,299,999
          - Dnn.PersonaBar.Recyclebin.Services namespace
        - 7,300,000 to 7,304,999
          - Dnn.PersonaBar.Roles.Components namespace
          - Dnn.PersonaBar.Roles.Components.Prompt.Commands namespace
          - Dnn.PersonaBar.Roles.Components.Prompt.Exceptions namespace
          - Dnn.PersonaBar.Roles.Components.Prompt.Models namespace
        - 7,305,000 to 7,309,999
          - Dnn.PersonaBar.Roles.Services namespace
          - Dnn.PersonaBar.Roles.Services.DTO namespace
        - 7,310,000 to 7,314,999
          - Dnn.PersonaBar.Extensions.Components.Security.Ssl namespace
          - Dnn.PersonaBar.Security.Attributes namespace
          - Dnn.PersonaBar.Security.Components namespace
          - Dnn.PersonaBar.Security.Components.Checks namespace
          - Dnn.PersonaBar.Security.Helper namespace
          - Dnn.PersonaBar.Security.MenuControllers namespace
        - 7,315,000 to 7,319,999
          - Dnn.PersonaBar.Security.Services namespace
          - Dnn.PersonaBar.Security.Services.Dto namespace
        - 7,320,000 to 7,324,999
          - Dnn.PersonaBar.Seo.Components namespace
        - 7,325,000 to 7,329,999
          - Dnn.PersonaBar.Seo.Services namespace
          - Dnn.PersonaBar.Seo.Services.Dto namespace
        - 7,330,000 to 7,334,999
          - Dnn.PersonaBar.Servers.Components namespace
          - Dnn.PersonaBar.Servers.Components.DatabaseServer namespace
          - Dnn.PersonaBar.Servers.Components.Log namespace
          - Dnn.PersonaBar.Servers.Components.PerformanceSettings namespace
          - Dnn.PersonaBar.Servers.Components.WebServer namespace
          - Dnn.PersonaBar.Servers.MenuControllers namespace
        - 7,335,000 to 7,339,999
          - Dnn.PersonaBar.Servers.Services namespace
          - Dnn.PersonaBar.Servers.Services.Dto namespace
        - 7,340,000 to 7,344,999
          - Dnn.PersonaBar.SiteGroups namespace
          - Dnn.PersonaBar.SiteGroups.Models namespace
        - 7,345,000 to 7,349,999
          - Dnn.PersonaBar.SiteGroups.Services namespace
        - 7,350,000 to 7,354,999
          - Dnn.PersonaBar.SiteImportExport.Components namespace
          - Dnn.PersonaBar.SiteImportExport.MenuControllers namespace
        - 7,355,000 to 7,354,999
          - Dnn.PersonaBar.SiteImportExport.Services namespace
        - 7,360,000 to 7,364,999
          - Dnn.PersonaBar.Sites.Components namespace
          - Dnn.PersonaBar.Sites.Components.Dto namespace
          - Dnn.PersonaBar.Sites.MenuControllers namespace
        - 7,365,000 to 7,369,999
          - Dnn.PersonaBar.Sites.Services namespace
          - Dnn.PersonaBar.Sites.Services.Dto namespace
        - 7,370,000 to 7,374,999
          - Dnn.PersonaBar.SiteSettings.Components namespace
          - Dnn.PersonaBar.SiteSettings.Components.Constants namespace
          - Dnn.PersonaBar.SiteSettings.MenuControllers namespace
        - 7,375,000 to 7,379,999
          - Dnn.PersonaBar.SiteSettings.Services namespace
          - Dnn.PersonaBar.SiteSettings.Services.Dto namespace
        - 7,380,000 to 7,384,999
          - Dnn.PersonaBar.SqlConsole.Components namespace
          - Dnn.PersonaBar.SqlConsole.MenuControllers namespace
        - 7,385,000 to 7,389,999
          - Dnn.PersonaBar.SqlConsole.Services namespace
        - 7,390,000 to 7,394,999
          - Dnn.PersonaBar.Styles.Components namespace
          - Dnn.PersonaBar.Styles.MenuControllers namespace
        - 7,395,000 to 7,399,999
          - Dnn.PersonaBar.Styles.Services namespace
        - 7,400,000 to 7,404,999
          - Dnn.PersonaBar.TaskScheduler.Components namespace
          - Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands namespace
          - Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models namespace
          - Dnn.PersonaBar.TaskScheduler.MenuControllers namespace
        - 7,405,000 to 7,409,999
          - Dnn.PersonaBar.TaskScheduler.Services namespace
          - Dnn.PersonaBar.TaskScheduler.Services.Dto namespace
        - 7,410,000 to 7,414,999
          - Dnn.PersonaBar.Themes.Components namespace
          - Dnn.PersonaBar.Themes.Components.DTO namespace
          - Dnn.PersonaBar.Themes.MenuControllers namespace
        - 7,415,000 to 7,419,999
          - Dnn.PersonaBar.Themes.Services namespace
        - 7,420,000 to 7,424,999
          - Dnn.PersonaBar.Users.Components namespace
          - Dnn.PersonaBar.Users.Components.Contracts namespace
          - Dnn.PersonaBar.Users.Components.Dto namespace
          - Dnn.PersonaBar.Users.Components.Helpers namespace
          - Dnn.PersonaBar.Users.Components.Prompt namespace
          - Dnn.PersonaBar.Users.Components.Prompt.Commands namespace
          - Dnn.PersonaBar.Users.Components.Prompt.Models namespace
          - Dnn.PersonaBar.Users.Data namespace
        - 7,425,000 to 7,429,999
          - Dnn.PersonaBar.Users.Services namespace
        - 7,430,000 to 7,434,999
          - Dnn.PersonaBar.Vocabularies.Components namespace
          - Dnn.PersonaBar.Vocabularies.Exceptions namespace
        - 7,435,000 to 7,439,999
          - Dnn.PersonaBar.Vocabularies.Services namespace
          - Dnn.PersonaBar.Vocabularies.Services.Dto namespace
          - Dnn.PersonaBar.Vocabularies.Validators namespace
     */

    // Dnn.PersonaBar.AdminLogs.Services.AdminLogsController (7,205,000 to 7,205,099)
    [LoggerMessage(EventId = 7_205_000, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerGetLogTypesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_001, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerGetLogItemsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_002, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerDeleteLogItemsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_003, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerEmailLogItemsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_004, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerClearLogException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_005, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerGetKeepMostRecentOptionsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_006, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerGetOccurrenceOptionsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_007, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerGetLogSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_008, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerGetLogSettingException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_008, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerAddLogSettingException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_009, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerUpdateLogSettingException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_010, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerDeleteLogSettingException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_205_011, Level = LogLevel.Error)]
    public static partial void AdminLogsControllerGetLatestLogSettingException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.ConfigConsole.Services.ConfigConsoleController (7,215,000 to 7,215,099)
    [LoggerMessage(EventId = 7_215_000, Level = LogLevel.Error)]
    public static partial void ConfigConsoleControllerGetConfigFilesListException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_215_001, Level = LogLevel.Error)]
    public static partial void ConfigConsoleControllerGetConfigFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_215_002, Level = LogLevel.Error)]
    public static partial void ConfigConsoleControllerValidateConfigFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_215_003, Level = LogLevel.Error)]
    public static partial void ConfigConsoleControllerUpdateConfigFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_215_004, Level = LogLevel.Error)]
    public static partial void ConfigConsoleControllerMergeConfigFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_215_005, Level = LogLevel.Error)]
    public static partial void ConfigConsoleControllerSaveNonConfigFileIOException(this ILogger logger, IOException exception);

    [LoggerMessage(EventId = 7_215_005, Level = LogLevel.Error)]
    public static partial void ConfigConsoleControllerSaveNonConfigFileGeneralException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Connectors.Services.ConnectorsController (7,225,000 to 7,225,099)
    [LoggerMessage(EventId = 7_225_100, Level = LogLevel.Warning)]
    public static partial void ConnectorsControllerSaveConnectionConnectorArgumentException(this ILogger logger, ConnectorArgumentException exception);

    [LoggerMessage(EventId = 7_225_101, Level = LogLevel.Error)]
    public static partial void ConnectorsControllerSaveConnectionGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_225_102, Level = LogLevel.Error)]
    public static partial void ConnectorsControllerDeleteConnectionException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_225_103, Level = LogLevel.Error)]
    public static partial void ConnectorsControllerGetConnectionLocalizedStringException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.CssEditor.Services.CssEditorController (7,245,000 to 7,245,099)
    [LoggerMessage(EventId = 7_245_000, Level = LogLevel.Error)]
    public static partial void CssEditorControllerGetStyleSheetException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_245_001, Level = LogLevel.Error)]
    public static partial void CssEditorControllerUpdateStyleSheetException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_245_002, Level = LogLevel.Error)]
    public static partial void CssEditorControllerRestoreStyleSheetException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.Editors.SkinPackageEditor (7,250,000 to 7,250,099)
    [LoggerMessage(EventId = 7_250_000, Level = LogLevel.Error)]
    public static partial void SkinPackageEditorSavePackageSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.InstallController (7,250,100 to 7,250,199)
    [LoggerMessage(EventId = 7_250_100, Level = LogLevel.Error)]
    public static partial void InstallControllerDeleteTempInstallFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_250_100, Level = LogLevel.Error)]
    public static partial void InstallControllerDeleteInstallFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_250_101, Level = LogLevel.Error)]
    public static partial void InstallControllerReadAzureCompatibleException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.Editors.SkinObjectPackageEditor (7,250,200 to 7,250,299)
    [LoggerMessage(EventId = 7_250_200, Level = LogLevel.Error)]
    public static partial void SkinObjectPackageEditorSavePackageSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.Editors.ModulePackageEditor (7,250,300 to 7,250,399)
    [LoggerMessage(EventId = 7_250_300, Level = LogLevel.Error)]
    public static partial void ModulePackageEditorSavePackageSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.Editors.JsLibraryPackageEditor (7,250,400 to 7,250,499)
    [LoggerMessage(EventId = 7_250_400, Level = LogLevel.Error)]
    public static partial void JsLibraryPackageEditorSavePackageSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.Editors.ExtensionLanguagePackageEditor (7,250,500 to 7,250,599)
    [LoggerMessage(EventId = 7_250_500, Level = LogLevel.Error)]
    public static partial void ExtensionLanguagePackageEditorSavePackageSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.Editors.AuthSystemPackageEditor (7,250,600 to 7,250,699)
    [LoggerMessage(EventId = 7_250_600, Level = LogLevel.Error)]
    public static partial void AuthSystemPackageEditorSavePackageSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Components.Editors.CoreLanguagePackageEditor (7,250,700 to 7,250,799)
    [LoggerMessage(EventId = 7_250_700, Level = LogLevel.Error)]
    public static partial void CoreLanguagePackageEditorSavePackageSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Services.UpgradesController (7,255,000 to 7,255,099)
    [LoggerMessage(EventId = 7_255_000, Level = LogLevel.Error)]
    public static partial void UpgradesControllerDeleteException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_001, Level = LogLevel.Error)]
    public static partial void UpgradesControllerUploadException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Extensions.Services.ExtensionsController (7,255,100 to 7,255,199)
    [LoggerMessage(EventId = 7_255_100, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetPackageTypesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_101, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetAllPackagesListExceptLangPacksException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_102, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetInstalledPackagesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_103, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetAvailablePackagesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_104, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetPackageSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_105, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerSavePackageSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_106, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetAvailableControlsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_107, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerDeletePackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_108, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerInstallPackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_109, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerParsePackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_110, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerParsePackageFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_111, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerParseLanguagePackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_112, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerInstallAvailablePackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_113, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerDownloadPackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_114, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerDownloadLanguagePackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_115, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetPackageUsageFilterException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_116, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetPackageUsageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_117, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerCreateExtensionException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_118, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetOwnerFoldersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_119, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetModuleFoldersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_120, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetModuleFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_121, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerCreateFolderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_122, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerCreateModuleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_123, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerGetPackageManifestException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_124, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerCreateManifestException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_125, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerCreateNewManifestException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_126, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerCreatePackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_255_127, Level = LogLevel.Error)]
    public static partial void ExtensionsControllerRefreshPackageFilesException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Licensing.Services.LicensingController (7,265,000 to 7,265,099)
    [LoggerMessage(EventId = 7_265_000, Level = LogLevel.Error)]
    public static partial void LicensingControllerGetProductException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Pages.Services.PagesController (7,275,000 to 7,275,099)
    [LoggerMessage(EventId = 7_275_000, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to make this page neutral, please consult the logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorMakingPageNeutral(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_001, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to make this page translatable.")]
    public static partial void PagesControllerUnexpectedErrorMakingPageTranslatable(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_002, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to add missing languages to this page, consult the logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorAddingMissingLanguagesToPage(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_003, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to notify the translators, please consult the logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorNotifyingTranslators(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_004, Level = LogLevel.Error, Message = "An unexpected error occurred trying to get this page localization, consult the logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorGettingPageLocalization(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_005, Level = LogLevel.Error, Message = "An unexpected error occurred trying to update the page localization, please consult the logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorUpdatingPageLocalization(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_006, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to restore the module onto that page.")]
    public static partial void PagesControllerUnexpectedErrorRestoringModuleOntoPage(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_007, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to delete the module, consult the logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorDeletingModule(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_008, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to find if content localization is enabled")]
    public static partial void PagesControllerUnexpectedErrorGettingContentLocalizationEnabled(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_009, Level = LogLevel.Error, Message = "An unexpected error occurred trying to get the cached items count, please consult the logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorGettingCachedItemsCount(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_275_010, Level = LogLevel.Error, Message = "An unexpected error occurred while trying to clear the cache for this page, see logs for more details.")]
    public static partial void PagesControllerUnexpectedErrorClearingCacheForPage(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Prompt.Components.Commands.Portal.ClearLog (7,280,000 to 7,280,099)
    [LoggerMessage(EventId = 7_280_000, Level = LogLevel.Error)]
    public static partial void ClearLogRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Prompt.Components.Commands.Host.ClearCache (7,280,100 to 7,280,199)
    [LoggerMessage(EventId = 7_280_100, Level = LogLevel.Error)]
    public static partial void ClearCacheRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Prompt.Components.Commands.Commands.ListCommands (7,280,200 to 7,280,299)
    [LoggerMessage(EventId = 7_280_200, Level = LogLevel.Error)]
    public static partial void ListCommandsRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Prompt.Components.Commands.Application.RestartApplication (7,280,300 to 7,280,399)
    [LoggerMessage(EventId = 7_280_300, Level = LogLevel.Error)]
    public static partial void RestartApplicationRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Roles.Components.Prompt.Commands.SetRole (7,280,400 to 7,280,499)
    [LoggerMessage(EventId = 7_280_400, Level = LogLevel.Error)]
    public static partial void SetRoleRunSetRoleException(this ILogger logger, SetRoleException exception);

    [LoggerMessage(EventId = 7_280_401, Level = LogLevel.Error)]
    public static partial void SetRoleRunGeneralException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Roles.Components.Prompt.Commands.NewRole (7,280,500 to 7,280,599)
    [LoggerMessage(EventId = 7_280_500, Level = LogLevel.Error)]
    public static partial void NewRoleRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Roles.Components.Prompt.Commands.ListRoles (7,280,600 to 7,280,699)
    [LoggerMessage(EventId = 7_280_600, Level = LogLevel.Error)]
    public static partial void ListRolesRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Roles.Components.Prompt.Commands.DeleteRole (7,280,700 to 7,280,799)
    [LoggerMessage(EventId = 7_280_700, Level = LogLevel.Error)]
    public static partial void DeleteRoleRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands.SetTask (7,280,800 to 7,280,899)
    [LoggerMessage(EventId = 7_280_800, Level = LogLevel.Error)]
    public static partial void SetTaskRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands.GetTask (7,280,900 to 7,280,999)
    [LoggerMessage(EventId = 7_280_900, Level = LogLevel.Error)]
    public static partial void GetTaskRunException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Prompt.Components.ModulesController (7,281,000 to 7,281,099)
    [LoggerMessage(EventId = 7_281_000, Level = LogLevel.Error)]
    public static partial void ModulesControllerCopyModuleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_281_001, Level = LogLevel.Error)]
    public static partial void ModulesControllerDeleteModuleException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Prompt.Services.CommandController (7,285,000 to 7,285,099)
    [LoggerMessage(EventId = 7_285_000, Level = LogLevel.Error)]
    public static partial void CommandControllerCmdException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_285_001, Level = LogLevel.Error)]
    public static partial void CommandControllerTryRunOldCommandException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_285_002, Level = LogLevel.Error)]
    public static partial void CommandControllerTryRunNewCommandException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_285_003, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void CommandControllerCmdPortalNotFound(this ILogger logger, string message);

    // Dnn.PersonaBar.Recyclebin.Components.RecyclebinController (7,290,000 to 7,290,099)
    [LoggerMessage(EventId = 7_290_000, Level = LogLevel.Error)]
    public static partial void RecyclebinControllerDeleteModuleForTabException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_290_001, Level = LogLevel.Error)]
    public static partial void RecyclebinControllerHardDeleteModuleException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Roles.Services.RolesController (7,305,000 to 7,305,099)
    [LoggerMessage(EventId = 7_305_000, Level = LogLevel.Error)]
    public static partial void RolesControllerGetRolesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_305_001, Level = LogLevel.Error)]
    public static partial void RolesControllerSaveRoleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_305_002, Level = LogLevel.Error)]
    public static partial void RolesControllerGetRoleGroupsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_305_003, Level = LogLevel.Error)]
    public static partial void RolesControllerSaveRoleGroupException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_305_004, Level = LogLevel.Error)]
    public static partial void RolesControllerDeleteRoleGroupException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_305_005, Level = LogLevel.Error)]
    public static partial void RolesControllerGetRoleUsersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_305_006, Level = LogLevel.Error)]
    public static partial void RolesControllerAddUserToRoleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_305_007, Level = LogLevel.Error)]
    public static partial void RolesControllerRemoveUserFromRoleException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Security.Components.Checks.BaseCheck (7,310,000 to 7,310,099)
    [LoggerMessage(EventId = 7_310_000, Level = LogLevel.Error, Message = "{ID} failed")]
    public static partial void BaseCheckFailed(this ILogger logger, Exception exception, string id);

    // Dnn.PersonaBar.Security.Services.SecurityController (7,315,000 to 7,315,099)
    [LoggerMessage(EventId = 7_315_000, Level = LogLevel.Information)]
    public static partial void SecurityControllerUpdateIpFilterArgumentException(this ILogger logger, ArgumentException exception);

    [LoggerMessage(EventId = 7_315_001, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetBasicLoginSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_002, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateBasicLoginSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_003, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetIpFiltersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_004, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetIpFilterException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_005, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateIpFilterException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_006, Level = LogLevel.Error)]
    public static partial void SecurityControllerDeleteIpFilterException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_007, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetMemberSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_008, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateMemberSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_009, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetRegistrationSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_010, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetSslSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_011, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateRegistrationSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_012, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateSslSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_012, Level = LogLevel.Error)]
    public static partial void SecurityControllerSetAllPagesSecureException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_013, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetSecurityBulletinsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_014, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetOtherSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_015, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateOtherSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_015, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetAuditCheckResultsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_016, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetAuditCheckResultException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_017, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetSuperuserActivitiesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_018, Level = LogLevel.Error)]
    public static partial void SecurityControllerSearchFileSystemAndDatabaseException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_019, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetLastModifiedFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_020, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetLastModifiedSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_021, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetApiTokenSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_022, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateApiTokenSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_023, Level = LogLevel.Error)]
    public static partial void SecurityControllerGetCspSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_315_024, Level = LogLevel.Error)]
    public static partial void SecurityControllerUpdateCspSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Seo.Services.SeoController (7,325,000 to 7,325,099)
    [LoggerMessage(EventId = 7_325_000, Level = LogLevel.Error)]
    public static partial void SeoControllerGetGeneralSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_001, Level = LogLevel.Error)]
    public static partial void SeoControllerUpdateGeneralSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_002, Level = LogLevel.Error)]
    public static partial void SeoControllerGetRegexSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_003, Level = LogLevel.Error)]
    public static partial void SeoControllerUpdateRegexSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_004, Level = LogLevel.Error)]
    public static partial void SeoControllerGetSitemapSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_005, Level = LogLevel.Error)]
    public static partial void SeoControllerCreateVerificationException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_006, Level = LogLevel.Error)]
    public static partial void SeoControllerUpdateSitemapSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_007, Level = LogLevel.Error)]
    public static partial void SeoControllerResetCacheException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_008, Level = LogLevel.Error)]
    public static partial void SeoControllerGetSitemapProvidersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_009, Level = LogLevel.Error)]
    public static partial void SeoControllerUpdateSitemapProviderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_010, Level = LogLevel.Error)]
    public static partial void SeoControllerGetExtensionUrlProvidersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_011, Level = LogLevel.Error)]
    public static partial void SeoControllerUpdateExtensionUrlProviderStatusException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_012, Level = LogLevel.Error)]
    public static partial void SeoControllerTestUrlException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_325_013, Level = LogLevel.Error)]
    public static partial void SeoControllerTestUrlRewriteException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.SystemInfoWebController (7,335,000 to 7,335,099)
    [LoggerMessage(EventId = 7_335_000, Level = LogLevel.Error)]
    public static partial void SystemInfoWebControllerGetWebServerInfoException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.SystemInfoServersController (7,335,100 to 7,335,199)
    [LoggerMessage(EventId = 7_335_100, Level = LogLevel.Error)]
    public static partial void SystemInfoServersControllerGetServersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_101, Level = LogLevel.Error)]
    public static partial void SystemInfoServersControllerDeleteServerException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_102, Level = LogLevel.Error)]
    public static partial void SystemInfoServersControllerEditServerUrlException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_103, Level = LogLevel.Error)]
    public static partial void SystemInfoServersControllerDeleteNonActiveServersException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.SystemInfoDatabaseController (7,335,200 to 7,335,299)
    [LoggerMessage(EventId = 7_335_200, Level = LogLevel.Error)]
    public static partial void SystemInfoDatabaseControllerGetDatabaseServerInfoException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.SystemInfoApplicationHostController (7,335,300 to 7,335,399)
    [LoggerMessage(EventId = 7_335_300, Level = LogLevel.Error)]
    public static partial void SystemInfoApplicationHostControllerGetApplicationInfoException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.SystemInfoApplicationAdminController (7,335,400 to 7,335,499)
    [LoggerMessage(EventId = 7_335_400, Level = LogLevel.Error)]
    public static partial void SystemInfoApplicationAdminControllerGetApplicationInfoException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.ServerController (7,335,500 to 7,335,599)
    [LoggerMessage(EventId = 7_335_500, Level = LogLevel.Error)]
    public static partial void ServerControllerRestartApplicationException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_501, Level = LogLevel.Error)]
    public static partial void ServerControllerClearCacheException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.ServerSettingsLogsController (7,335,600 to 7,335,699)
    [LoggerMessage(EventId = 7_335_600, Level = LogLevel.Error)]
    public static partial void ServerSettingsLogsControllerGetLogsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_601, Level = LogLevel.Error)]
    public static partial void ServerSettingsLogsControllerGetLogFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_602, Level = LogLevel.Error)]
    public static partial void ServerSettingsLogsControllerGetUpgradeLogFileException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.ServerSettingsPerformanceController (7,335,700 to 7,335,799)
    [LoggerMessage(EventId = 7_335_700, Level = LogLevel.Error)]
    public static partial void ServerSettingsPerformanceControllerGetPerformanceSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_701, Level = LogLevel.Error)]
    public static partial void ServerSettingsPerformanceControllerIncrementPortalVersionException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_702, Level = LogLevel.Error)]
    public static partial void ServerSettingsPerformanceControllerIncrementHostVersionException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_703, Level = LogLevel.Error)]
    public static partial void ServerSettingsPerformanceControllerUpdatePerformanceSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.ServerSettingsSmtpAdminController (7,335,800 to 7,335,899)
    [LoggerMessage(EventId = 7_335_800, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpAdminControllerGetSmtpSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_801, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpAdminControllerUpdateSmtpSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_802, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpAdminControllerSendTestEmailException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_803, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpAdminControllerGetSmtpOAuthProvidersException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Servers.Services.ServerSettingsSmtpHostController (7,335,900 to 7,335,999)
    [LoggerMessage(EventId = 7_335_900, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpHostControllerGetSmtpSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_901, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpHostControllerUpdateSmtpSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_902, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpHostControllerSendTestEmailException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_335_903, Level = LogLevel.Error)]
    public static partial void ServerSettingsSmtpHostControllerGetSmtpOAuthProvidersException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.SiteGroups.Services.SiteGroupsController (7,340,000 to 7,340,099)
    [LoggerMessage(EventId = 7_340_000, Level = LogLevel.Error)]
    public static partial void SiteGroupsControllerSaveException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_340_001, Level = LogLevel.Error)]
    public static partial void SiteGroupsControllerDeleteException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Sites.Components.SitesController (7,360,000 to 7,360,099)
    [LoggerMessage(EventId = 7_360_000, Level = LogLevel.Error)]
    public static partial void ComponentsSitesControllerCreatePortalException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_360_001, Level = LogLevel.Error)]
    public static partial void ComponentsSitesControllerSendMailException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_360_002, Level = LogLevel.Error)]
    public static partial void ComponentsSitesControllerTryDeleteCreatingPortalException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Sites.Services.SitesController (7,365,000 to 7,365,099)
    [LoggerMessage(EventId = 7_365_000, Level = LogLevel.Error)]
    public static partial void SitesControllerGetPortalsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_365_001, Level = LogLevel.Error)]
    public static partial void SitesControllerCreatePortalException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_365_002, Level = LogLevel.Error)]
    public static partial void SitesControllerDeletePortalException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_365_003, Level = LogLevel.Error)]
    public static partial void SitesControllerExportPortalTemplateException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_365_004, Level = LogLevel.Error)]
    public static partial void SitesControllerGetPortalLocalesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_365_005, Level = LogLevel.Error)]
    public static partial void SitesControllerDeleteExpiredPortalsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_365_006, Level = LogLevel.Error)]
    public static partial void SitesControllerGetPortalTemplatesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_365_007, Level = LogLevel.Error)]
    public static partial void SitesControllerRequiresQuestionAndAnswerException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.SiteSettings.Components.LanguagesControllerTasks (7,370,000 to 7,370,099)
    [LoggerMessage(EventId = 7_370_000, Level = LogLevel.Error)]
    public static partial void LanguageControllerTasksLocalizeSitePagesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_370_001, Level = LogLevel.Error)]
    public static partial void LanguageControllerTasksLocalizeLanguagePagesException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.SiteSettings.Services.LanguagesController (7,375,000 to 7,375,099)
    [LoggerMessage(EventId = 7_375_000, Level = LogLevel.Warning, Message = "{Message}")]
    public static partial void LanguagesControllerObsolete(this ILogger logger, string message);

    [LoggerMessage(EventId = 7_375_001, Level = LogLevel.Error)]
    public static partial void LanguagesControllerGetRootResourcesFoldersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_002, Level = LogLevel.Error)]
    public static partial void LanguagesControllerGetSubRootResourcesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_003, Level = LogLevel.Error)]
    public static partial void LanguagesControllerGetResxEntriesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_004, Level = LogLevel.Error)]
    public static partial void LanguagesControllerSaveResxEntriesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_005, Level = LogLevel.Error)]
    public static partial void LanguagesControllerEnableLocalizedContentException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_006, Level = LogLevel.Error)]
    public static partial void LanguagesControllerLocalizedContentException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_007, Level = LogLevel.Error)]
    public static partial void LanguagesControllerGetLocalizationProgressException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_008, Level = LogLevel.Error)]
    public static partial void LanguagesControllerDisableLocalizedContentException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_009, Level = LogLevel.Error)]
    public static partial void LanguagesControllerMarkAllPagesTranslatedException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_010, Level = LogLevel.Error)]
    public static partial void LanguagesControllerActivateLanguageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_011, Level = LogLevel.Error)]
    public static partial void LanguagesControllerPublishAllPagesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_012, Level = LogLevel.Error)]
    public static partial void LanguagesControllerDeleteLanguagePagesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_013, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void LanguagesControllerLoadResourceException(this ILogger logger, Exception exception, string message);

    [LoggerMessage(EventId = 7_375_014, Level = LogLevel.Error)]
    public static partial void LanguagesControllerGetTabsForTranslationException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.SiteSettings.Services.SiteSettingsController (7,375,100 to 7,375,199)
    [LoggerMessage(EventId = 7_375_100, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetPortalSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_101, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetCultureListException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_102, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdatePortalSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_103, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetDefaultPagesSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_104, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateDefaultPagesSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_105, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetMessagingSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_106, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateMessagingSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_107, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetProfileSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_108, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateProfileSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_109, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetProfilePropertiesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_110, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetProfilePropertyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_111, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetProfilePropertyLocalizationException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_112, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateProfilePropertyLocalizationException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_113, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerAddProfilePropertyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_114, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateProfilePropertyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_115, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateProfilePropertyOrdersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_116, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerDeleteProfilePropertyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_117, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetUrlMappingSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_118, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateUrlMappingSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_119, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetSiteAliasesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_120, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetSiteAliasException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_121, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerAddSiteAliasException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_122, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateSiteAliasException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_123, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerDeleteSiteAliasException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_124, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerSetPrimarySiteAliasException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_125, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetListInfoException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_126, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateListEntryException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_127, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerDeleteListEntryException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_128, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateListEntryOrdersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_129, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetPrivacySettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_130, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdatePrivacySettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_131, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerResetTermsAgreementException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_132, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetBasicSearchSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_133, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateBasicSearchSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_134, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerCompactSearchIndexException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_135, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerHostSearchReindexException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_136, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerPortalSearchReindexException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_137, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetPortalsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_138, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetSynonymsGroupsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_139, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerAddSynonymsGroupException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_140, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateSynonymsGroupException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_141, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerDeleteSynonymsGroupException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_142, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetIgnoreWordsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_143, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerAddIgnoreWordsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_144, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateIgnoreWordsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_145, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerDeleteIgnoreWordsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_146, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetLanguageSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_147, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateLanguageSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_148, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetLanguagesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_149, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetLanguageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_150, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetAllLanguagesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_151, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerAddLanguageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_152, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateLanguageRolesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_153, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateLanguageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_154, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerVerifyLanguageResourceFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_155, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetModuleListException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_156, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerCreateLanguagePackException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_157, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetTranslatorRolesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_158, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetTranslatorRoleGroupsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_159, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerGetOtherSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_375_160, Level = LogLevel.Error)]
    public static partial void SiteSettingsControllerUpdateOtherSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.TaskScheduler.Components.TaskSchedulerController (7,400,000 to 7,400,099)
    [LoggerMessage(EventId = 7_400_000, Level = LogLevel.Error)]
    public static partial void ComponentsTaskSchedulerControllerGetScheduleItemsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.TaskScheduler.Services.TaskSchedulerController (7,405,000 to 7,405,099)
    [LoggerMessage(EventId = 7_405_000, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerGetServersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_001, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerGetScheduleItemsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_002, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerGetSchedulerSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_003, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerUpdateSchedulerSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_004, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerGetScheduleItemHistoryException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_005, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerGetScheduleItemException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_006, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerCreateScheduleItemException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_007, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerUpdateScheduleItemException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_008, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerGetScheduleStatusException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_009, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerStartScheduleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_010, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerStopScheduleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_011, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerRunScheduleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_405_012, Level = LogLevel.Error)]
    public static partial void TaskSchedulerControllerDeleteScheduleException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Themes.Components.ThemesController (7,410,000 to 7,410,099)
    [LoggerMessage(EventId = 7_410_000, Level = LogLevel.Error)]
    public static partial void ComponentsThemesControllerUpdateManifestException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_410_001, Level = LogLevel.Error)]
    public static partial void ComponentsThemesControllerCreateThumbnailException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Themes.Services.ThemesController (7,415,000 to 7,415,099)
    [LoggerMessage(EventId = 7_415_000, Level = LogLevel.Error)]
    public static partial void ThemesControllerGetCurrentThemeException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_001, Level = LogLevel.Error)]
    public static partial void ThemesControllerGetThemesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_002, Level = LogLevel.Error)]
    public static partial void ThemesControllerGetThemeFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_003, Level = LogLevel.Error)]
    public static partial void ThemesControllerApplyThemeException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_004, Level = LogLevel.Error)]
    public static partial void ThemesControllerApplyDefaultThemeException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_005, Level = LogLevel.Error)]
    public static partial void ThemesControllerDeleteThemePackageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_006, Level = LogLevel.Error)]
    public static partial void ThemesControllerGetEditableTokensException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_007, Level = LogLevel.Error)]
    public static partial void ThemesControllerGetEditableSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_008, Level = LogLevel.Error)]
    public static partial void ThemesControllerGetEditableValuesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_009, Level = LogLevel.Error)]
    public static partial void ThemesControllerUpdateThemeException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_010, Level = LogLevel.Error)]
    public static partial void ThemesControllerParseThemeException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_415_011, Level = LogLevel.Error)]
    public static partial void ThemesControllerRestoreThemeException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Users.Components.UsersController (7,420,000 to 7,420,099)
    [LoggerMessage(EventId = 7_420_000, Level = LogLevel.Error)]
    public static partial void ComponentsUsersControllerChangePasswordMembershipPasswordException(this ILogger logger, MembershipPasswordException exception);

    [LoggerMessage(EventId = 7_420_001, Level = LogLevel.Error)]
    public static partial void ComponentsUsersControllerChangePasswordInvalidPasswordException(this ILogger logger, InvalidPasswordException exception);

    [LoggerMessage(EventId = 7_420_002, Level = LogLevel.Error)]
    public static partial void ComponentsUsersControllerChangePasswordGeneralException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Users.Services.UsersController (7,425,000 to 7,425,099)
    [LoggerMessage(EventId = 7_425_000, Level = LogLevel.Error)]
    public static partial void UsersControllerCreateUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_001, Level = LogLevel.Error)]
    public static partial void UsersControllerGetUsersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_002, Level = LogLevel.Error)]
    public static partial void UsersControllerGetUserFiltersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_003, Level = LogLevel.Error)]
    public static partial void UsersControllerGetUserDetailException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_004, Level = LogLevel.Error)]
    public static partial void UsersControllerChangePasswordInvalidPasswordException(this ILogger logger, InvalidPasswordException exception);

    [LoggerMessage(EventId = 7_425_005, Level = LogLevel.Error)]
    public static partial void UsersControllerChangePasswordGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_006, Level = LogLevel.Error)]
    public static partial void UsersControllerForceChangePasswordException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_007, Level = LogLevel.Error)]
    public static partial void UsersControllerCreateResetTokenArgumentException(this ILogger logger, ArgumentException exception);

    [LoggerMessage(EventId = 7_425_008, Level = LogLevel.Error)]
    public static partial void UsersControllerCreateResetTokenGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_009, Level = LogLevel.Error)]
    public static partial void UsersControllerSendPasswordResetLinkException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_010, Level = LogLevel.Error)]
    public static partial void UsersControllerUpdateAuthorizeStatusException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_011, Level = LogLevel.Error)]
    public static partial void UsersControllerSoftDeleteUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_012, Level = LogLevel.Error)]
    public static partial void UsersControllerHardDeleteUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_013, Level = LogLevel.Error)]
    public static partial void UsersControllerRestoreDeletedUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_014, Level = LogLevel.Error)]
    public static partial void UsersControllerUpdateSuperUserStatusException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_015, Level = LogLevel.Error)]
    public static partial void UsersControllerUpdateUserBasicInfoSqlException(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 7_425_016, Level = LogLevel.Error)]
    public static partial void UsersControllerUpdateUserBasicInfoGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_017, Level = LogLevel.Error)]
    public static partial void UsersControllerUnlockUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_018, Level = LogLevel.Error)]
    public static partial void UsersControllerGetSuggestRolesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_019, Level = LogLevel.Error)]
    public static partial void UsersControllerGetUserRolesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_020, Level = LogLevel.Error)]
    public static partial void UsersControllerSaveUserRoleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_425_021, Level = LogLevel.Error)]
    public static partial void UsersControllerRemoveUserRoleException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Vocabularies.Services.VocabulariesController (7,435,000 to 7,435,099)
    [LoggerMessage(EventId = 7_435_000, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerGetVocabulariesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_001, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerCreateVocabularyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_002, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerUpdateVocabularyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_003, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerDeleteVocabularyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_004, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerGetTermsByVocabularyIdException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_005, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerGetTermException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_006, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerCreateTermException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_007, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerUpdateTermException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7_435_008, Level = LogLevel.Error)]
    public static partial void VocabulariesControllerDeleteTermException(this ILogger logger, Exception exception);
}
