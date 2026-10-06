// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke;

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.UI;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Entities.Tabs.TabVersions.Exceptions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Upgrade.Internals.Steps;

using Lucene.Net.Search;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.Library project has been assigned event IDs from 1 to 999,999.</remarks>
internal static partial class LoggerMessages
{
    /*
    # Event IDs
    - 1 to 999,999
      - DotNetNuke.Library project
        - 1 to 999
          - DotNetNuke namespace
        - 1,000 to 1,999
          - DotNetNuke.Application namespace
        - 2,000 to 2,999
          - DotNetNuke.Collections namespace
          - DotNetNuke.Collections.Internal namespace
        - 3,000 to 3,999
          - DotNetNuke.Common namespace
          - DotNetNuke.Common.Internal namespace
          - DotNetNuke.Common.Controls namespace
          - DotNetNuke.Common.Extensions namespace
          - DotNetNuke.Common.Lists namespace
        - 4,000 to 4,999
          - DotNetNuke.Common.Utilities namespace
          - DotNetNuke.Common.Utilities.Internal namespace
          - DotNetNuke.Common.Utils namespace
        - 5,000 to 5,999
          - DotNetNuke.ComponentModel namespace
          - DotNetNuke.ComponentModel.DataAnnotations namespace
        - 6,000 to 6,999
          - DotNetNuke.Data namespace
          - DotNetNuke.Data.PetaPoco namespace
        - 7,000 to 7,999
          - DotNetNuke.ExtensionPoints namespace
          - DotNetNuke.ExtensionPoints.Filters namespace
        - 8,000 to 8,999
          - DotNetNuke.Framework namespace
          - DotNetNuke.Framework.Internal.Reflection namespace
          - DotNetNuke.Framework.JavaScriptLibraries namespace
          - DotNetNuke.Framework.Providers namespace
          - DotNetNuke.Framework.Reflection namespace
        - 9,000 to 9,999
          - DotNetNuke.Prompt namespace
        - 10,000 to 10,999
          - DotNetNuke.Security namespace
          - DotNetNuke.Security.Cookies namespace
          - DotNetNuke.Security.Membership namespace
          - DotNetNuke.Security.Permissions namespace
          - DotNetNuke.Security.Permissions.Controls namespace
          - DotNetNuke.Security.Profile namespace
          - DotNetNuke.Security.Roles namespace
        - 100,000 to 109,999
          - DotNetNuke.Entities namespace
          - DotNetNuke.Entities.Controllers namespace
          - DotNetNuke.Entities.DataStructures namespace
          - DotNetNuke.Entities.Host namespace
          - DotNetNuke.Entities.Icons namespace
        - 110,000 to 119,999
          - DotNetNuke.Entities.Content namespace
          - DotNetNuke.Entities.Content.Common namespace
          - DotNetNuke.Entities.Content.Data namespace
          - DotNetNuke.Entities.Content.Taxonomy namespace
          - DotNetNuke.Entities.Content.Workflow namespace
          - DotNetNuke.Entities.Content.Workflow.Actions namespace
          - DotNetNuke.Entities.Content.Workflow.Dto namespace
          - DotNetNuke.Entities.Content.Workflow.Entities namespace
          - DotNetNuke.Entities.Content.Workflow.Exceptions namespace
        - 120,000 to 129,999
          - DotNetNuke.Entities.Friends namespace
          - DotNetNuke.Entities.Profile namespace
          - DotNetNuke.Entities.Users namespace
          - DotNetNuke.Entities.Users.Membership namespace
          - DotNetNuke.Entities.Users.Social namespace
          - DotNetNuke.Entities.Users.Social.Data namespace
        - 130,000 to 139,999
          - DotNetNuke.Entities.Modules namespace
          - DotNetNuke.Entities.Modules.Actions namespace
          - DotNetNuke.Entities.Modules.Communications namespace
          - DotNetNuke.Entities.Modules.Definitions namespace
          - DotNetNuke.Entities.Modules.Prompt namespace
          - DotNetNuke.Entities.Modules.Settings namespace
        - 140,000 to 149,999
          - DotNetNuke.Entities.Portals namespace
          - DotNetNuke.Entities.Portals.Data namespace
          - DotNetNuke.Entities.Portals.Extensions namespace
          - DotNetNuke.Entities.Portals.Internal namespace
          - DotNetNuke.Entities.Portals.Templates namespace
        - 150,000 to 159,999
          - DotNetNuke.Entities.Tabs namespace
          - DotNetNuke.Entities.Tabs.Actions namespace
          - DotNetNuke.Entities.Tabs.Dto namespace
          - DotNetNuke.Entities.Tabs.TabVersions namespace
          - DotNetNuke.Entities.Tabs.TabVersions.Exceptions namespace
        - 160,000 to 169,999
          - DotNetNuke.Entities.Urls namespace
          - DotNetNuke.Entities.Urls.Config namespace
        - 200,000 to 200,999
          - DotNetNuke.Services.Analytics namespace
          - DotNetNuke.Services.Analytics.Config namespace
        - 201,000 to 201,999
          - DotNetNuke.Services.Assets namespace
        - 202,000 to 202,999
          - DotNetNuke.Services.Authentication namespace
          - DotNetNuke.Services.Authentication.OAuth namespace
        - 204,000 to 204,999
          - DotNetNuke.Services.ClientCapability namespace
        - 205,000 to 205,999
          - DotNetNuke.Services.ClientDependency namespace
        - 206,000 to 206,999
          - DotNetNuke.Services.Connections namespace
        - 207,000 to 207,999
          - DotNetNuke.Services.Cryptography namespace
        - 208,000 to 208,999
          - DotNetNuke.Services.DependencyInjection namespace
        - 209,000 to 209,999
          - DotNetNuke.Services.EventQueue namespace
          - DotNetNuke.Services.EventQueue.Config namespace
        - 210,000 to 210,999
          - DotNetNuke.Services.GeneratedImage namespace
          - DotNetNuke.Services.GeneratedImage.FilterTransform namespace
          - DotNetNuke.Services.GeneratedImage.ImageQuantization namespace
          - DotNetNuke.Services.GeneratedImage.StartTransform namespace
        - 211,000 to 211,999
          - DotNetNuke.Services.Journal namespace
          - DotNetNuke.Services.Journal.Internal namespace
        - 212,000 to 212,999
          - DotNetNuke.Services.Mobile namespace
        - 213,000 to 213,999
          - DotNetNuke.Services.Pages namespace
        - 214,000 to 214,999
          - DotNetNuke.Services.Personalization namespace
        - 215,000 to 215,999
          - DotNetNuke.Services.Registration namespace
        - 216,000 to 216,999
          - DotNetNuke.Services.Scheduling namespace
        - 217,000 to 217,999
          - DotNetNuke.Services.Sitemap namespace
        - 218,000 to 218,999
          - DotNetNuke.Services.Syndication namespace
        - 219,000 to 219,999
          - DotNetNuke.Services.SystemHealth namespace
        - 220,000 to 220,999
          - DotNetNuke.Services.Tokens namespace
        - 221,000 to 221,999
          - DotNetNuke.Services.UserProfile namespace
        - 222,000 to 222,999
          - DotNetNuke.Services.UserRequest namespace
        - 223,000 to 223,999
          - DotNetNuke.Services.Users namespace
        - 224,000 to 224,999
          - DotNetNuke.Services.Zip namespace
        - 225,000 to 229,999
          - DotNetNuke.Services.Exceptions namespace
        - 230,000 to 239,999
          - DotNetNuke.Services.FileSystem namespace
          - DotNetNuke.Services.FileSystem.EventArgs namespace
          - DotNetNuke.Services.FileSystem.FolderMappings namespace
          - DotNetNuke.Services.FileSystem.Internal namespace
          - DotNetNuke.Services.FileSystem.Internal.SecurityCheckers namespace
        - 240,000 to 249,999
          - DotNetNuke.Services.Installer namespace
          - DotNetNuke.Services.Installer.Blocker namespace
          - DotNetNuke.Services.Installer.Dependencies namespace
          - DotNetNuke.Services.Installer.Installers namespace
          - DotNetNuke.Services.Installer.Log namespace
          - DotNetNuke.Services.Installer.Packages namespace
          - DotNetNuke.Services.Installer.Packages.WebControls namespace
          - DotNetNuke.Services.Installer.Writers namespace
        - 250,000 to 259,999
          - DotNetNuke.Services.Localization namespace
          - DotNetNuke.Services.Localization.Internal namespace
          - DotNetNuke.Services.Localization.Persian namespace
        - 260,000 to 269,999
          - DotNetNuke.Services.Log.EventLog namespace
        - 270,000 to 279,999
          - DotNetNuke.Services.Mail namespace
          - DotNetNuke.Services.Mail.OAuth namespace
        - 280,000 to 289,999
          - DotNetNuke.Services.Messaging namespace
          - DotNetNuke.Services.Messaging.Data namespace
        - 290,000 to 299,999
          - DotNetNuke.Services.Cache namespace
          - DotNetNuke.Services.ModuleCache namespace
          - DotNetNuke.Services.OutputCache namespace
          - DotNetNuke.Services.OutputCache.Providers namespace
        - 300,000 to 309,999
          - DotNetNuke.Services.Search namespace
          - DotNetNuke.Services.Search.Controllers namespace
          - DotNetNuke.Services.Search.Entities namespace
          - DotNetNuke.Services.Search.Internals namespace
        - 310,000 to 319,999
          - DotNetNuke.Services.Upgrade namespace
          - DotNetNuke.Services.Upgrade.InternalController.Steps namespace
          - DotNetNuke.Services.Upgrade.Internals namespace
          - DotNetNuke.Services.Upgrade.Internals.InstallConfiguration namespace
          - DotNetNuke.Services.Upgrade.Internals.Steps namespace
        - 400,000 to 449,999
          - DotNetNuke.Services.Social.Messaging namespace
          - DotNetNuke.Services.Social.Messaging.Data namespace
          - DotNetNuke.Services.Social.Messaging.Exceptions namespace
          - DotNetNuke.Services.Social.Messaging.Internal namespace
          - DotNetNuke.Services.Social.Messaging.Internal.Views namespace
          - DotNetNuke.Services.Social.Messaging.Scheduler namespace
        - 450,000 to 459,999
          - DotNetNuke.Services.Social.Notifications namespace
          - DotNetNuke.Services.Social.Notifications.Data namespace
        - 460,000 to 469,999
          - DotNetNuke.Services.Social.Subscriptions namespace
          - DotNetNuke.Services.Social.Subscriptions.Data namespace
          - DotNetNuke.Services.Social.Subscriptions.Entities namespace
        - 500,000 to 599,999
          - DotNetNuke.Services.Url.FriendlyUrl namespace
        - 600,000 to 600,999
          - DotNetNuke.UI namespace
          - DotNetNuke.UI.Internals namespace
        - 601,000 to 601,999
          - DotNetNuke.UI.Containers namespace
          - DotNetNuke.UI.Containers.EventListeners namespace
        - 602,000 to 602,999
          - DotNetNuke.UI.ControlPanels namespace
        - 603,000 to 603,999
          - DotNetNuke.UI.Modules namespace
          - DotNetNuke.UI.Modules.Html5 namespace
        - 604,000 to 604,999
          - DotNetNuke.UI.Skins namespace
          - DotNetNuke.UI.Skins.Controls namespace
          - DotNetNuke.UI.Skins.Controls.EventListeners namespace
        - 605,000 to 605,999
          - DotNetNuke.UI.UserControls namespace
        - 606,000 to 606,999
          - DotNetNuke.UI.Utilities namespace
        - 607,000 to 607,999
          - DotNetNuke.UI.WebControls namespace
          - DotNetNuke.UI.WebControls.Internal namespace
    - 1,000,000 to 1,199,999
      - DotNetNuke.Website project
    - 1,200,000 to 1,499,999
      - DotNetNuke.Web project
    - 1,500,000 to 1,599,999
      - DotNetNuke.Web.Mvc project
    - 1,600,000 to 1,649,999
      - DotNetNuke.Web.Client project
    - 1,650,000 to 1,699,999
      - DotNetNuke.Web.Client.ResourceManager project
    - 2,000,000 to 2,099,999
      - DotNetNuke.HttpModules project
    - 2,100,000 to 2,199,999
      - DotNetNuke.DependencyInjection project
    - 2,200,000 to 2,299,999
      - DotNetNuke.Maintenance project
    - 2,300,000 to 2,399,999
      - DotNetNuke.Syndication project
    - 2,400,000 to 2,499,999
      - DotNetNuke.Providers.AspNetCCP project
    - 2,500,000 to 2,999,999
      - DotNetNuke.Providers.FolderProviders project
    - 3,000,000 to 3,099,999
      - DotNetNuke.Modules.CoreMessaging project
    - 3,100,000 to 3,199,999
      - DotNetNuke.Modules.Groups project
    - 3,200,000 to 3,299,999
      - DotNetNuke.Modules.Journal project
    - 3,300,000 to 3,399,999
      - DotNetNuke.Modules.MemberDirectory project
    - 3,400,000 to 3,499,999
      - DotNetNuke.Modules.RazorHost project
    - 3,500,000 to 3,599,999
      - Dnn.Modules.Console project
    - 3,600,000 to 3,699,999
      - Dnn.Modules.ResourceManager project
    - 4,000,000 to 4,999,999
      - DnnExportImport project
    - 5,000,000 to 5,499,999
      - Dnn.PersonaBar.Library project
    - 5,500,000 to 5,999,999
      - Dnn.PersonaBar.UI
    - 6,000,000 to 6,099,999
      - Dnn.EditBar.UI project
    - 7,000,000 to 7,999,999
      - Dnn.PersonaBar.Extensions project
    */

    /*
     * 1,000 to 1,999
     * DotNetNuke.Application namespace
     */

    // DotNetNuke.Application.ApplicationStatusInfo (1,000 to 1,099)
    [LoggerMessage(EventId = 1_000, Level = LogLevel.Trace, Message = "Getting application status")]
    public static partial void ApplicationStatusInfoGettingStatus(this ILogger logger);

    [LoggerMessage(EventId = 1_001, Level = LogLevel.Trace, Message = "result of getting providerpath: {Message}")]
    public static partial void ApplicationStatusInfoResultOfGettingProviderPath(this ILogger logger, string message);

    [LoggerMessage(EventId = 1_002, Level = LogLevel.Trace, Message = "Application status is {Status}")]
    public static partial void ApplicationStatusInfoStatusIs(this ILogger logger, UpgradeStatus status);

    [LoggerMessage(EventId = 1_003, Level = LogLevel.Error)]
    public static partial void ApplicationStatusInfoDatabaseVersionException(this ILogger logger, Exception exception);

    /*
     * 2,000 to 2,999
     * DotNetNuke.Collections namespace
     * DotNetNuke.Collections.Internal namespace
     */

    // DotNetNuke.Collections.CollectionExtensions (2,000 to 2,099)
    [LoggerMessage(EventId = 2_000, Level = LogLevel.Error, Message = "Error loading portal setting: {Key}:{Value} Default value {DefaultValue} was used instead")]
    public static partial void CollectionExtensionsErrorLoadingPortalSettingDefaultUsedInstead(this ILogger logger, string key, object value, object defaultValue);

    /*
     * 3,000 to 3,999
     * DotNetNuke.Common namespace
     * DotNetNuke.Common.Internal namespace
     * DotNetNuke.Common.Controls namespace
     * DotNetNuke.Common.Extensions namespace
     * DotNetNuke.Common.Lists namespace
     */

    // DotNetNuke.Common.Initialize (3,000 to 3,099)
    [LoggerMessage(EventId = 3_000, Level = LogLevel.Information, Message = "Application shutting down. Reason: {Reason}")]
    public static partial void InitializeApplicationShuttingDown(this ILogger logger, string reason);

    [LoggerMessage(EventId = 3_001, Level = LogLevel.Information, Message = "Application shutting down. Reason: {Reason}\nASP.NET Shutdown Info: {ShutdownMessage}\n{ShutdownStack}")]
    public static partial void InitializeApplicationShuttingDownWithInfo(this ILogger logger, string reason, string shutdownMessage, string shutdownStack);

    [LoggerMessage(EventId = 3_002, Level = LogLevel.Information, Message = "UnderConstruction page was shown because application needs to be installed, and both the AutoUpgrade and UseWizard AppSettings in web.config are false. Use /install/install.aspx?mode=install to install application. ")]
    public static partial void InitializeUnderConstructionPageShownBecauseInstallationNeeded(this ILogger logger);

    [LoggerMessage(EventId = 3_003, Level = LogLevel.Information, Message = "UnderConstruction page was shown because application needs to be upgraded, and both the AutoUpgrade and UseInstallWizard AppSettings in web.config are false. Use /install/install.aspx?mode=upgrade to upgrade application. ")]
    public static partial void InitializeUnderConstructionPageShownBecauseUpgradeNeeded(this ILogger logger);

    [LoggerMessage(EventId = 3_004, Level = LogLevel.Information, Message = "Application Initializing")]
    public static partial void InitializeApplicationInitializing(this ILogger logger);

    [LoggerMessage(EventId = 3_005, Level = LogLevel.Information, Message = "Application Initialized")]
    public static partial void InitializeApplicationInitialized(this ILogger logger);

    [LoggerMessage(EventId = 3_006, Level = LogLevel.Trace, Message = "Running Schedule {SchedulerMode}")]
    public static partial void InitializeRunningSchedule(this ILogger logger, SchedulerMode schedulerMode);

    [LoggerMessage(EventId = 3_007, Level = LogLevel.Trace, Message = "Request {LocalPath}")]
    public static partial void InitializeRequest(this ILogger logger, string localPath);

    [LoggerMessage(EventId = 3_008, Level = LogLevel.Error, Message = "UnderConstruction page was shown because we cannot ascertain the application was ever installed, and there is no working database connection. Check database connectivity before continuing. ")]
    public static partial void InitializeUnderConstructionPageShownBecauseNoWorkingDatabaseConnection(this ILogger logger);

    [LoggerMessage(EventId = 3_009, Level = LogLevel.Error, Message = "The connection to the database has failed, the application is not installed yet, and both AutoUpgrade and UseInstallWizard are not set in web.config, a 500 error page will be shown to visitors")]
    public static partial void InitializeConnectionToTheDatabaseHasFailedTheApplicationIsNotInstalledYetA500ErrorPageWillBeShown(this ILogger logger);

    [LoggerMessage(EventId = 3_010, Level = LogLevel.Error, Message = "The connection to the database has failed, however, the application is already completely installed, a 500 error page will be shown to visitors")]
    public static partial void InitializeConnectionToTheDatabaseHasFailedHoweverTheApplicationIsAlreadyCompletelyInstalledA500ErrorPageWillBeShown(this ILogger logger);

    // DotNetNuke.Common.Globals (3,100 to 3,199)
    [LoggerMessage(EventId = 3_100, Level = LogLevel.Error)]
    public static partial void GlobalsRedirectException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3_101, Level = LogLevel.Error)]
    public static partial void GlobalsGetTotalRecordsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3_102, Level = LogLevel.Error)]
    public static partial void GlobalsDateToStringException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3_103, Level = LogLevel.Error)]
    public static partial void GlobalsDeserializeHashTableBase64Exception(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3_104, Level = LogLevel.Error)]
    public static partial void GlobalsSerializeHashTableBase64Exception(this ILogger logger, Exception exception);

    // DotNetNuke.Common.Lists.ListInfoCollection (3,200 to 3,299)
    [LoggerMessage(EventId = 3_200, Level = LogLevel.Error)]
    public static partial void ListInfoCollectionAddException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3_201, Level = LogLevel.Error)]
    public static partial void ListInfoCollectionItemIndexException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3_202, Level = LogLevel.Error)]
    public static partial void ListInfoCollectionItemKeyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3_203, Level = LogLevel.Error)]
    public static partial void ListInfoCollectionItemKeyCacheException(this ILogger logger, Exception exception);

    // DotNetNuke.Common.Internal.ServicesRoutingManager (3,300 to 3,399)
    [LoggerMessage(EventId = 3_300, Level = LogLevel.Error, Message = "Unable to register service routes")]
    public static partial void ServicesRoutingManagerUnableToRegisterServiceRoutes(this ILogger logger, Exception exception);

    // DotNetNuke.Common.Internal.EventHandlersContainer (3,400 to 3,499)
    [LoggerMessage(EventId = 3_400, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void EventHandlersContainerConstructorException(this ILogger logger, Exception exception, string message);

    /*
     * 4,000 to 4,999
     * DotNetNuke.Common.Utilities namespace
     * DotNetNuke.Common.Utilities.Internal namespace
     * DotNetNuke.Common.Utils namespace
     */

    // DotNetNuke.Common.Utilities.FileSystemUtils (4,000 to 4,099)
    [LoggerMessage(EventId = 4_000, Level = LogLevel.Information, Message = "{RootPath} does not exist. ")]
    public static partial void FileSystemUtilsFolderDoesNotExist(this ILogger logger, string rootPath);

    [LoggerMessage(EventId = 4_001, Level = LogLevel.Error, Message = "Reading from {FilePath} didn't read all data in buffer. Requested to read {BufferLength} bytes, but was read {ReadCount} bytes")]
    public static partial void FileSystemUtilsAddToZipDidNotReadAllDataInBuffer(this ILogger logger, string filePath, long bufferLength, int readCount);

    [LoggerMessage(EventId = 4_002, Level = LogLevel.Error)]
    public static partial void FileSystemUtilsDeleteFileWithWaitException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_003, Level = LogLevel.Error)]
    public static partial void FileSystemUtilsUnzipResourcesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_004, Level = LogLevel.Error)]
    public static partial void FileSystemUtilsDeleteFilesFolderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_005, Level = LogLevel.Error)]
    public static partial void FileSystemUtilsDeleteFilesFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_005, Level = LogLevel.Error)]
    public static partial void FileSystemUtilsDeleteFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_005, Level = LogLevel.Error)]
    public static partial void FileSystemUtilsDeleteFolderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_005, Level = LogLevel.Error)]
    public static partial void FileSystemUtilsUnzipException(this ILogger logger, Exception exception);

    // DotNetNuke.Common.Utilities.Internal.RetryableAction (4,100 to 4,199)
    [LoggerMessage(EventId = 4_100, Level = LogLevel.Trace, Message = "Action succeeded - {Description}")]
    public static partial void RetryableActionSucceeded(this ILogger logger, string description);

    [LoggerMessage(EventId = 4_101, Level = LogLevel.Trace, Message = "Retrying action {RetriesRemaining} - {Description}")]
    public static partial void RetryableActionRetrying(this ILogger logger, int retriesRemaining, string description);

    [LoggerMessage(EventId = 4_102, Level = LogLevel.Warning, Message = "All retries of action failed - {Description}")]
    public static partial void RetryableActionAllRetriesFailed(this ILogger logger, string description);

    // DotNetNuke.Common.Utilities.Config (4,200 to 4,299)
    [LoggerMessage(EventId = 4_200, Level = LogLevel.Error)]
    public static partial void ConfigSaveFileIOException(this ILogger logger, IOException exception);

    [LoggerMessage(EventId = 4_201, Level = LogLevel.Error)]
    public static partial void ConfigSaveFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_202, Level = LogLevel.Error)]
    public static partial void ConfigTouchException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_203, Level = LogLevel.Error)]
    public static partial void ConfigUpdateMachineKeyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_204, Level = LogLevel.Error)]
    public static partial void ConfigUpdateValidationKeyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_205, Level = LogLevel.Error)]
    public static partial void ConfigUpdateInstallVersionException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_206, Level = LogLevel.Error)]
    public static partial void ConfigAddFcnModeException(this ILogger logger, Exception exception);

    // DotNetNuke.Common.Utilities.DataCache (4,300 to 4,399)
    [LoggerMessage(EventId = 4_300, Level = LogLevel.Error)]
    public static partial void DataCacheItemRemovedCallbackException(this ILogger logger, Exception exception);

    // DotNetNuke.Common.Utilities.FileSystemPermissionVerifier (4,400 to 4,499)
    [LoggerMessage(EventId = 4_400, Level = LogLevel.Error)]
    public static partial void FileSystemPermissionVerifierFileCreateException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_401, Level = LogLevel.Error)]
    public static partial void FileSystemPermissionVerifierFileDeleteException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_402, Level = LogLevel.Error)]
    public static partial void FileSystemPermissionVerifierFolderCreateException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4_403, Level = LogLevel.Error)]
    public static partial void FileSystemPermissionVerifierFolderDeleteException(this ILogger logger, Exception exception);

    /*
     * 5,000 to 5,999
     * DotNetNuke.ComponentModel namespace
     * DotNetNuke.ComponentModel.DataAnnotations namespace
     */

    // DotNetNuke.ComponentModel.ContainerWithServiceProviderFallback (5,000 to 5,099)
    [LoggerMessage(EventId = 5_000, Level = LogLevel.Trace, Message = "Getting component for {FullName}")]
    public static partial void ContainerWithServiceProviderFallbackGettingComponent(this ILogger logger, string fullName);

    [LoggerMessage(EventId = 5_001, Level = LogLevel.Trace, Message = "Got component for {FullName} from container")]
    public static partial void ContainerWithServiceProviderFallbackGotComponentFromContainer(this ILogger logger, string fullName);

    [LoggerMessage(EventId = 5_002, Level = LogLevel.Trace, Message = "Getting component for {FullName} from service provider")]
    public static partial void ContainerWithServiceProviderFallbackGettingComponentFromServiceProvider(this ILogger logger, string fullName);

    // DotNetNuke.ComponentModel.ComponentFactory (5,100 to 5,199)
    [LoggerMessage(EventId = 5_100, Level = LogLevel.Warning, Message = "Container was null, instantiating SimpleContainer")]
    public static partial void ComponentFactoryInstantiatingSimpleContainer(this ILogger logger);

    // DotNetNuke.ComponentModel.ProviderInstaller (5,200 to 5,299)
    [LoggerMessage(EventId = 5_200, Level = LogLevel.Error)]
    public static partial void ProviderInstallerCouldNotLoadProvider(this ILogger logger, ConfigurationErrorsException exception);

    /*
     * 6,000 to 6,999
     * DotNetNuke.Data namespace
     * DotNetNuke.Data.PetaPoco namespace
     */

    // DotNetNuke.Data.DataProvider (6,000 to 6,099)
    [LoggerMessage(EventId = 6_000, Level = LogLevel.Debug)]
    public static partial void DataProviderSqlExceptionFromAddPropertyDefinition(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 6_001, Level = LogLevel.Error)]
    public static partial void DataProviderSqlExceptionFromAddSearchDeletedItems(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 6_002, Level = LogLevel.Error)]
    public static partial void DataProviderSqlExceptionFromDeleteProcessedSearchDeletedItems(this ILogger logger, SqlException exception);

    // DotNetNuke.Data.SqlDataProvider (6,100 to 6,199)
    [LoggerMessage(EventId = 6_100, Level = LogLevel.Trace, Message = "Executing SQL Script {SQL}")]
    public static partial void SqlDataProviderExecutingSqlScript(this ILogger logger, string sql);

    [LoggerMessage(EventId = 6_101, Level = LogLevel.Error)]
    public static partial void SqlDataProviderGrantProcedureExecutePermissionException(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 6_102, Level = LogLevel.Error)]
    public static partial void SqlDataProviderGrantFunctionExecutePermissionException(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 6_103, Level = LogLevel.Error)]
    public static partial void SqlDataProviderExecuteScriptException(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 6_104, Level = LogLevel.Error)]
    public static partial void SqlDataProviderExecuteSqlException(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 6_105, Level = LogLevel.Error)]
    public static partial void SqlDataProviderExecuteSqlGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 6_106, Level = LogLevel.Error)]
    public static partial void SqlDataProviderExecuteUpgradedConnectionQueryException(this ILogger logger, SqlException exception);

    // DotNetNuke.Data.PetaPoco.PetaPocoHelper (6,200 to 6,299)
    [LoggerMessage(EventId = 6_200, Level = LogLevel.Error, Message = "[1] Error executing SQL: {SQL}")]
    public static partial void PetaPocoHelper1ErrorExecutingSql(this ILogger logger, Exception exception, string sql);

    [LoggerMessage(EventId = 6_201, Level = LogLevel.Error, Message = "[2] Error executing SQL: {CommandText}")]
    public static partial void PetaPocoHelper2ErrorExecutingSql(this ILogger logger, Exception exception, string commandText);

    [LoggerMessage(EventId = 6_202, Level = LogLevel.Error, Message = "[3] Error executing SQL: {SQL}")]
    public static partial void PetaPocoHelper3ErrorExecutingSql(this ILogger logger, Exception exception, string sql);

    [LoggerMessage(EventId = 6_203, Level = LogLevel.Error, Message = "[4] Error executing SQL: {SQL}")]
    public static partial void PetaPocoHelper4ErrorExecutingSql(this ILogger logger, Exception exception, string sql);

    [LoggerMessage(EventId = 6_204, Level = LogLevel.Error, Message = "[5] Error executing SQL: {SQL}")]
    public static partial void PetaPocoHelper5ErrorExecutingSql(this ILogger logger, Exception exception, string sql);

    /*
     * 8,000 to 8,999
     * DotNetNuke.Framework namespace
     * DotNetNuke.Framework.Internal.Reflection namespace
     * DotNetNuke.Framework.JavaScriptLibraries namespace
     * DotNetNuke.Framework.Providers namespace
     * DotNetNuke.Framework.Reflection namespace
     */

    // DotNetNuke.Framework.PageBase (8,000 to 8,099)
    [LoggerMessage(EventId = 8_000, Level = LogLevel.Debug, Message = "{Origin} {Action} (TabId:{TabId},{Message})")]
    public static partial void PageBaseTrace(this ILogger logger, string origin, string action, int tabId, string message);

    [LoggerMessage(EventId = 8_001, Level = LogLevel.Critical, Message = "An error has occurred while loading page.")]
    public static partial void PageBaseAnErrorHasOccurredWhileLoadingPage(this ILogger logger, Exception exception);

    // DotNetNuke.Framework.Reflection (8,100 to 8,199)
    [LoggerMessage(EventId = 8_100, Level = LogLevel.Warning, Message = "Unable to create type via service provider: {Type}")]
    public static partial void ReflectionUnableToCreateTypeViaServiceProvider(this ILogger logger, InvalidOperationException exception, Type type);

    [LoggerMessage(EventId = 8_101, Level = LogLevel.Error, Message = "{TypeName}")]
    public static partial void ReflectionCreateTypeException(this ILogger logger, Exception exception, string typeName);

    /*
     * 10,000 to 10,999
     * DotNetNuke.Security namespace
     * DotNetNuke.Security.Cookies namespace
     * DotNetNuke.Security.Membership namespace
     * DotNetNuke.Security.Permissions namespace
     * DotNetNuke.Security.Permissions.Controls namespace
     * DotNetNuke.Security.Profile namespace
     * DotNetNuke.Security.Roles namespace
     */

    // DotNetNuke.Security.Roles.RoleController (10,000 to 10,099)
    [LoggerMessage(EventId = 10_000, Level = LogLevel.Error)]
    public static partial void RoleControllerUserAlreadyBelongsToRoleException(this ILogger logger, Exception exception);

    // DotNetNuke.Security.Roles.DNNRoleProvider (10,100 to 10,199)
    [LoggerMessage(EventId = 10_100, Level = LogLevel.Error)]
    public static partial void DnnRoleProviderAddUserToRoleException(this ILogger logger, Exception exception);

    // DotNetNuke.Security.Membership.AspNetMembershipProvider (10,200 to 10,299)
    [LoggerMessage(EventId = 10_200, Level = LogLevel.Error)]
    public static partial void AspNetMembershipProviderDeleteUserException(this ILogger logger, Exception exception);

    /*
     * 100,000 to 109,999
     * DotNetNuke.Entities namespace
     * DotNetNuke.Entities.Controllers namespace
     * DotNetNuke.Entities.DataStructures namespace
     * DotNetNuke.Entities.Host namespace
     * DotNetNuke.Entities.Icons namespace
     */

    // DotNetNuke.Entities.Host.ServerController (100,000 to 100,099)
    [LoggerMessage(EventId = 100_000, Level = LogLevel.Debug, Message = "GetExecutingServerName: {ExecutingServerName}")]
    public static partial void ServerControllerGetExecutingServerName(this ILogger logger, string executingServerName);

    [LoggerMessage(EventId = 100_001, Level = LogLevel.Debug, Message = "GetServerName: {ServerName}")]
    public static partial void ServerControllerGetServerName(this ILogger logger, string serverName);

    [LoggerMessage(EventId = 100_002, Level = LogLevel.Error)]
    public static partial void ServerControllerGetServerUrlException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 100_003, Level = LogLevel.Error)]
    public static partial void ServerControllerGetServerUniqueIdException(this ILogger logger, Exception exception);

    // DotNetNuke.Entities.Icons.IconController (100,100 to 100,199)
    [LoggerMessage(EventId = 100_100, Level = LogLevel.Warning, Message = "Icon Not Present on Disk {PhysicalPath}")]
    public static partial void IconControllerIconNotPresentOnDisk(this ILogger logger, string physicalPath);

    // DotNetNuke.Entities.Controllers.HostController (100,200 to 100,299)
    [LoggerMessage(EventId = 100_200, Level = LogLevel.Error)]
    public static partial void HostControllerGetBooleanException(this ILogger logger, Exception exception);

    /*
     * 110,000 to 119,999
     * DotNetNuke.Entities.Content namespace
     * DotNetNuke.Entities.Content.Common namespace
     * DotNetNuke.Entities.Content.Data namespace
     * DotNetNuke.Entities.Content.Taxonomy namespace
     * DotNetNuke.Entities.Content.Workflow namespace
     * DotNetNuke.Entities.Content.Workflow.Actions namespace
     * DotNetNuke.Entities.Content.Workflow.Dto namespace
     * DotNetNuke.Entities.Content.Workflow.Entities namespace
     * DotNetNuke.Entities.Content.Workflow.Exceptions namespace
     */

    // DotNetNuke.Entities.Content.AttachmentController (110,000 to 110,099)
    [LoggerMessage(EventId = 110_000, Level = LogLevel.Warning, Message = "Unable to load file properties for File ID {FileId}")]
    public static partial void AttachmentControllerUnableToLoadFileProperties(this ILogger logger, int fileId);

    /*
     * 120,000 to 129,999
     * DotNetNuke.Entities.Friends namespace
     * DotNetNuke.Entities.Profile namespace
     * DotNetNuke.Entities.Users namespace
     * DotNetNuke.Entities.Users.Membership namespace
     * DotNetNuke.Entities.Users.Social namespace
     * DotNetNuke.Entities.Users.Social.Data namespace
     */

    // DotNetNuke.Entities.Users.UserProfile (120,000 to 120,099)
    [LoggerMessage(EventId = 120_000, Level = LogLevel.Error, Message = "Invalid data type {DataTypeId} for profile property {PropertyName}")]
    public static partial void UserProfileInvalidDataType(this ILogger logger, int dataTypeId, string propertyName);

    // DotNetNuke.Entities.Users.UserOnlineController (120,100 to 120,199)
    [LoggerMessage(EventId = 120_100, Level = LogLevel.Error)]
    public static partial void UserOnlineControllerUpdateUsersOnlineException(this ILogger logger, Exception exception);

    // DotNetNuke.Entities.Profile.ProfileController (120,200 to 120,299)
    [LoggerMessage(EventId = 120_200, Level = LogLevel.Error)]
    public static partial void ProfileControllerCreateThumbnailsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 120_201, Level = LogLevel.Error)]
    public static partial void ProfileControllerFillPropertyDefinitionInfoException(this ILogger logger, Exception exception);

    /*
     * 130,000 to 139,999
     * DotNetNuke.Entities.Modules namespace
     * DotNetNuke.Entities.Modules.Actions namespace
     * DotNetNuke.Entities.Modules.Communications namespace
     * DotNetNuke.Entities.Modules.Definitions namespace
     * DotNetNuke.Entities.Modules.Prompt namespace
     * DotNetNuke.Entities.Modules.Settings namespace
     */

    // DotNetNuke.Entities.Modules.PortalModuleBase (130,000 to 130,099)
    [LoggerMessage(EventId = 130_000, Level = LogLevel.Debug, Message = "PortalModuleBase.OnInit Start (TabId:{TabId},ModuleId:{ModuleId}): {Type}")]
    public static partial void PortalModuleBaseOnInitStart(this ILogger logger, int tabId, int moduleId, Type type);

    [LoggerMessage(EventId = 130_001, Level = LogLevel.Debug, Message = "PortalModuleBase.OnInit End (TabId:{TabId},ModuleId:{ModuleId}): {Type}")]
    public static partial void PortalModuleBaseOnInitEnd(this ILogger logger, int tabId, int moduleId, Type type);

    [LoggerMessage(EventId = 130_002, Level = LogLevel.Debug, Message = "PortalModuleBase.OnLoad Start (TabId:{TabId},ModuleId:{ModuleId}): {Type}")]
    public static partial void PortalModuleBaseOnLoadStart(this ILogger logger, int tabId, int moduleId, Type type);

    [LoggerMessage(EventId = 130_003, Level = LogLevel.Debug, Message = "PortalModuleBase.OnLoad End (TabId:{TabId},ModuleId:{ModuleId}): {Type}")]
    public static partial void PortalModuleBaseOnLoadEnd(this ILogger logger, int tabId, int moduleId, Type type);

    // DotNetNuke.Entities.Modules.DesktopModuleController (130,100 to 130,199)
    [LoggerMessage(EventId = 130_100, Level = LogLevel.Warning, Message = "Unable to find module by module ID. ID:{DesktopModuleId} PortalID:{PortalId}")]
    public static partial void DesktopModuleControllerUnableToFindModuleByModuleId(this ILogger logger, int desktopModuleId, int portalId);

    [LoggerMessage(EventId = 130_101, Level = LogLevel.Warning, Message = "Unable to find module by package ID. ID:{PackageId}")]
    public static partial void DesktopModuleControllerUnableToFindModuleByPackageId(this ILogger logger, int packageId);

    [LoggerMessage(EventId = 130_102, Level = LogLevel.Warning, Message = "Unable to find module by name. Name:{DesktopModuleName} portalId:{PortalId}")]
    public static partial void DesktopModuleControllerUnableToFindModuleByName(this ILogger logger, string desktopModuleName, int portalId);

    [LoggerMessage(EventId = 130_103, Level = LogLevel.Warning, Message = "Unable to find module by friendly name. Name:{FriendlyName}")]
    public static partial void DesktopModuleControllerUnableToFindModuleByFriendlyName(this ILogger logger, string friendlyName);

    // DotNetNuke.Entities.Modules.ModuleController (130,200 to 130,299)
    [LoggerMessage(EventId = 130_200, Level = LogLevel.Error, Message = "Error localizing module, moduleId: {ModuleId}")]
    public static partial void ModuleControllerErrorLocalizingModule(this ILogger logger, Exception exception, int moduleId);

    [LoggerMessage(EventId = 130_201, Level = LogLevel.Error)]
    public static partial void ModuleControllerModuleAlreadyOnThePageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 130_202, Level = LogLevel.Error)]
    public static partial void ModuleControllerAddContentException(this ILogger logger, Exception exception);

    // DotNetNuke.Entities.Modules.Prompt.AddModule (130,300 to 130,399)
    [LoggerMessage(EventId = 130_300, Level = LogLevel.Error)]
    public static partial void AddModuleRunException(this ILogger logger, Exception exception);

    // DotNetNuke.Entities.Modules.ModuleInfo (130,400 to 130,499)
    [LoggerMessage(EventId = 130_400, Level = LogLevel.Error)]
    public static partial void ModuleInfoFillException(this ILogger logger, Exception exception);

    // DotNetNuke.Entities.Modules.EventMessageProcessor (130,500 to 130,599)
    [LoggerMessage(EventId = 130_500, Level = LogLevel.Error)]
    public static partial void EventMessageProcessorProcessMessageException(this ILogger logger, Exception exception);

    /*
     * 140,000 to 149,999
     * DotNetNuke.Entities.Portals namespace
     * DotNetNuke.Entities.Portals.Data namespace
     * DotNetNuke.Entities.Portals.Extensions namespace
     * DotNetNuke.Entities.Portals.Internal namespace
     * DotNetNuke.Entities.Portals.Templates namespace
     */

    // DotNetNuke.Entities.Portals.Templates.PortalTemplateImporter (140,000 to 140,099)
    [LoggerMessage(EventId = 140_000, Level = LogLevel.Error)]
    public static partial void PortalTemplateImporterParseTemplateException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_001, Level = LogLevel.Error)]
    public static partial void PortalTemplateImporterGetFolderMappingException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_002, Level = LogLevel.Error)]
    public static partial void PortalTemplateImporterAddFolderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_003, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void PortalTemplateImporterParseFilesInvalidFileExtensionException(this ILogger logger, InvalidFileExtensionException exception, string message);

    // DotNetNuke.Entities.Portals.PortalGroupController (140,100 to 140,199)
    [LoggerMessage(EventId = 140_100, Level = LogLevel.Error)]
    public static partial void PortalGroupControllerLogEventException(this ILogger logger, Exception exception);

    // DotNetNuke.Entities.Portals.PortalController (140,200 to 140,299)
    [LoggerMessage(EventId = 140_200, Level = LogLevel.Error)]
    public static partial void PortalControllerCreateChildPortalFolderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_201, Level = LogLevel.Error)]
    public static partial void PortalControllerGetPortalSettingException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_202, Level = LogLevel.Error)]
    public static partial void PortalControllerGetPortalSettingAsBooleanException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_203, Level = LogLevel.Error)]
    public static partial void PortalControllerGetPortalSettingAsIntegerException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_204, Level = LogLevel.Error)]
    public static partial void PortalControllerGetPortalSettingAsDoubleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_205, Level = LogLevel.Error)]
    public static partial void PortalControllerGetAdminUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_206, Level = LogLevel.Error)]
    public static partial void PortalControllerCreateAdminUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_207, Level = LogLevel.Error)]
    public static partial void PortalControllerProcessResourceFileExplicitException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_208, Level = LogLevel.Error)]
    public static partial void PortalControllerLogDeletePortalException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_209, Level = LogLevel.Error)]
    public static partial void PortalControllerEnableBrowserLanguageInDefaultException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_210, Level = LogLevel.Error)]
    public static partial void PortalControllerDeleteHomeDirectoryException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_211, Level = LogLevel.Error)]
    public static partial void PortalControllerCreateChildPortalFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_212, Level = LogLevel.Error)]
    public static partial void PortalControllerAddDefaultFolderTypesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_213, Level = LogLevel.Error)]
    public static partial void PortalControllerApplyPortalTemplateException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_214, Level = LogLevel.Error)]
    public static partial void PortalControllerCreateDefaultRelationshipsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_215, Level = LogLevel.Error)]
    public static partial void PortalControllerCreateProfanityListException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_216, Level = LogLevel.Error)]
    public static partial void PortalControllerCreateBannedPasswordsListException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_217, Level = LogLevel.Error)]
    public static partial void PortalControllerLogCreatePortalException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 140_218, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void PortalControllerEnsureRequiredProvidersForFolderTypesException(this ILogger logger, Exception exception, string message);

    [LoggerMessage(EventId = 140_219, Level = LogLevel.Error, Message = "{Message}: {FolderTypeName}")]
    public static partial void PortalControllerAddFolderMappingException(this ILogger logger, Exception exception, string message, string folderTypeName);

    [LoggerMessage(EventId = 140_220, Level = LogLevel.Error, Message = "Error while parsing: {TemplateFilePath}")]
    public static partial void PortalControllerErrorWhileParsing(this ILogger logger, Exception exception, string templateFilePath);

    // DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo (140,300 to 140,399)
    [LoggerMessage(EventId = 140_300, Level = LogLevel.Error, Message = "Error while parsing: {TemplateFilePath}")]
    public static partial void PortalTemplateInfoErrorWhileParsing(this ILogger logger, Exception exception, string templateFilePath);

    /*
     * 150,000 to 159,999
     * DotNetNuke.Entities.Tabs namespace
     * DotNetNuke.Entities.Tabs.Actions namespace
     * DotNetNuke.Entities.Tabs.Dto namespace
     * DotNetNuke.Entities.Tabs.TabVersions namespace
     * DotNetNuke.Entities.Tabs.TabVersions.Exceptions namespace
     */

    // DotNetNuke.Entities.Tabs.TabController (150,000 to 150,099)
    [LoggerMessage(EventId = 150_000, Level = LogLevel.Trace, Message = "Localizing TabId: {TabId}, TabPath: {TabPath}, Locale: {Locale}")]
    public static partial void TabControllerLocalizingTab(this ILogger logger, int tabId, string tabPath, string locale);

    [LoggerMessage(EventId = 150_001, Level = LogLevel.Warning, Message = "Invalid tabId {TabId} of portal {PortalId}")]
    public static partial void TabControllerInvalidTabId(this ILogger logger, int tabId, int portalId);

    [LoggerMessage(EventId = 150_002, Level = LogLevel.Warning, Message = "Unable to find tabId {TabId} of portal {PortalId}")]
    public static partial void TabControllerUnableToFindTabId(this ILogger logger, int tabId, int portalId);

    // DotNetNuke.Entities.Tabs.TabWorkflowTracker (150,100 to 150,199)
    [LoggerMessage(EventId = 150_100, Level = LogLevel.Warning, Message = "Current Workflow and Default workflow are not found on NotifyWorkflowAboutChanges")]
    public static partial void TabWorkflowTrackerCurrentWorkflowAndDefaultWorkflowAreNotFoundOnNotifyWorkflowAboutChanges(this ILogger logger);

    // DotNetNuke.Entities.Tabs.TabVersions.TabVersionBuilder (150,200 to 150,299)
    [LoggerMessage(EventId = 150_200, Level = LogLevel.Error)]
    public static partial void TabVersionBuilderConvertToModuleInfoException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 150_201, Level = LogLevel.Error, Message = "There was a problem making rollback of the module {ModuleId}.")]
    public static partial void TabVersionBuilderProblemMakingRollbackOfTheModule(this ILogger logger, DnnTabVersionException exception, int moduleId);

    // DotNetNuke.Entities.Tabs.TabPublishingController (150,300 to 150,399)
    [LoggerMessage(EventId = 150_300, Level = LogLevel.Error, Message = "{ErrorMessage}")]
    public static partial void TabPublishingControllerPermissionsAreNotMetThePageHasNotBeenPublished(this ILogger logger, Entities.Tabs.PermissionsNotMetException exception, string errorMessage);

    /*
     * 160,000 to 169,999
     * DotNetNuke.Entities.Urls namespace
     * DotNetNuke.Entities.Urls.Config namespace
     */

    // DotNetNuke.Entities.Urls.UrlRewriterUtils (160,000 to 160,099)
    [LoggerMessage(EventId = 160_000, Level = LogLevel.Error)]
    public static partial void UrlRewriterUtilsLogExceptionInRequest(this ILogger logger, Exception exception);

    // DotNetNuke.Entities.Urls.Config.RewriterConfiguration (160,100 to 160,199)
    [LoggerMessage(EventId = 160_100, Level = LogLevel.Error, Message = "{LogInfo}")]
    public static partial void RewriterConfigurationGetConfigFailed(this ILogger logger, LogInfo logInfo);

    /*
     * 200,000 to 200,999
     * DotNetNuke.Services.Analytics namespace
     * DotNetNuke.Services.Analytics.Config namespace
     */

    // DotNetNuke.Services.Analytics.GoogleAnalyticsController (200,000 to 200,099)
    [LoggerMessage(EventId = 200_000, Level = LogLevel.Error)]
    public static partial void GoogleAnalyticsControllerGetConfigFileException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Analytics.Config.AnalyticsConfiguration (200,100 to 200,199)
    [LoggerMessage(EventId = 200_100, Level = LogLevel.Error)]
    public static partial void AnalyticsConfigGetConfigException(this ILogger logger, Exception exception);

    /*
     * 202,000 to 202,999
     * DotNetNuke.Services.Authentication namespace
     * DotNetNuke.Services.Authentication.OAuth namespace
     */

    // DotNetNuke.Services.Authentication.OAuth.OAuthClientBase (202,000 to 202,099)
    [LoggerMessage(EventId = 202_000, Level = LogLevel.Error, Message = "WebResponse exception: {ResponseContent}")]
    public static partial void OAuthClientBaseWebResponseException(this ILogger logger, WebException exception, string responseContent);

    // DotNetNuke.Services.Authentication.AuthenticationConfig (202,100 to 202,199)
    [LoggerMessage(EventId = 202_100, Level = LogLevel.Error)]
    public static partial void AuthenticationConfigConstructorException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Authentication.AuthenticationController (202,200 to 202,299)
    [LoggerMessage(EventId = 202_200, Level = LogLevel.Error)]
    public static partial void AuthenticationControllerGetAuthenticationTypeException(this ILogger logger, Exception exception);

    /*
     * 216,000 to 216,999
     * DotNetNuke.Services.Scheduling namespace
     */

    // DotNetNuke.Services.Scheduling.ScheduleHistoryItem (216,000 to 216,099)
    [LoggerMessage(EventId = 216_000, Level = LogLevel.Debug, Message = "ScheduleHistoryItem.Succeeded Info (ScheduledTask Start): {FriendlyName}")]
    public static partial void ScheduleHistoryItemSucceededStart(this ILogger logger, string friendlyName);

    [LoggerMessage(EventId = 216_001, Level = LogLevel.Debug, Message = "ScheduleHistoryItem.Succeeded Info (ScheduledTask End): {FriendlyName}")]
    public static partial void ScheduleHistoryItemSucceededEnd(this ILogger logger, string friendlyName);

    [LoggerMessage(EventId = 216_002, Level = LogLevel.Trace, Message = "{Notes}")]
    public static partial void ScheduleHistoryItemLogNote(this ILogger logger, string notes);

    // DotNetNuke.Services.Scheduling.Scheduler (216,100 to 216,199)
    [LoggerMessage(EventId = 216_100, Level = LogLevel.Debug)]
    public static partial void SchedulerReaderLockRequestTimeout(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 216_101, Level = LogLevel.Debug, Message = "loadqueue executingServer: {ExecutingServer}")]
    public static partial void SchedulerLoadQueue(this ILogger logger, string executingServer);

    [LoggerMessage(EventId = 216_102, Level = LogLevel.Debug, Message = "LoadQueueFromTimer executingServer: {ExecutingServer}")]
    public static partial void SchedulerLoadQueueFromTimer(this ILogger logger, string executingServer);

    // DotNetNuke.Services.Scheduling.ProcessGroup (216,200 to 216,299)
    [LoggerMessage(EventId = 216_200, Level = LogLevel.Error)]
    public static partial void ProcessGroupDoWorkException(this ILogger logger, Exception exception);

    /*
     * 217,000 to 217,999
     * DotNetNuke.Services.Sitemap namespace
     */

    // DotNetNuke.Services.Sitemap.CoreSitemapProvider (217,000 to 217,099)
    [LoggerMessage(EventId = 217_000, Level = LogLevel.Error, Message = "Error has occurred getting PageUrl for {TabName}")]
    public static partial void CoreSitemapProviderErrorGettingPageUrl(this ILogger logger, Exception exception, string tabName);

    /*
     * 219,000 to 219,999
     * DotNetNuke.Services.SystemHealth namespace
     */

    // DotNetNuke.Services.SystemHealth.WebServerMonitor (219,000 to 219,099)
    [LoggerMessage(EventId = 219_000, Level = LogLevel.Information, Message = "Starting WebServerMonitor")]
    public static partial void WebServerMonitorStartingWebServerMonitor(this ILogger logger);

    [LoggerMessage(EventId = 219_001, Level = LogLevel.Information, Message = "Starting UpdateCurrentServerActivity")]
    public static partial void WebServerMonitorStartingUpdateCurrentServerActivity(this ILogger logger);

    [LoggerMessage(EventId = 219_002, Level = LogLevel.Information, Message = "Starting RemoveInActiveServers")]
    public static partial void WebServerMonitorStartingRemoveInActiveServers(this ILogger logger);

    [LoggerMessage(EventId = 219_003, Level = LogLevel.Information, Message = "Finished RemoveInActiveServers")]
    public static partial void WebServerMonitorFinishedRemoveInActiveServers(this ILogger logger);

    [LoggerMessage(EventId = 219_004, Level = LogLevel.Information, Message = "Finished UpdateCurrentServerActivity")]
    public static partial void WebServerMonitorFinishedUpdateCurrentServerActivity(this ILogger logger);

    [LoggerMessage(EventId = 219_005, Level = LogLevel.Information, Message = "Finished WebServerMonitor")]
    public static partial void WebServerMonitorFinishedWebServerMonitor(this ILogger logger);

    [LoggerMessage(EventId = 219_006, Level = LogLevel.Error, Message = "Error in WebServerMonitor: {Message}. {StackTrace}")]
    public static partial void WebServerMonitorErrorInWebServerMonitor(this ILogger logger, Exception exception, string message, string stackTrace);

    /*
     * 221,000 to 221,999
     * DotNetNuke.Services.UserProfile namespace
     */

    // DotNetNuke.Services.UserProfile.UserProfilePageHandler (221,000 to 221,099)
    [LoggerMessage(EventId = 221_000, Level = LogLevel.Debug)]
    public static partial void UserProfilePageHandlerException(this ILogger logger, Exception exception);

    /*
     * 225,000 to 229,999
     * DotNetNuke.Services.Exceptions namespace
     */

    // DotNetNuke.Services.Exceptions.Exceptions (225,000 to 225,099)
    [LoggerMessage(EventId = 225_000, Level = LogLevel.Error, Message = "FriendlyMessage=\"{FriendlyMessage}\" ctrl=\"{Control}\"")]
    public static partial void ExceptionsProcessModuleLoadException(this ILogger logger, Exception exception, string friendlyMessage, Control control);

    [LoggerMessage(EventId = 225_001, Level = LogLevel.Error)]
    public static partial void ExceptionsGetExceptionInfoReflectionPermissionException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 225_002, Level = LogLevel.Error)]
    public static partial void ExceptionsLogModuleLoadException(this ILogger logger, ModuleLoadException exception);

    [LoggerMessage(EventId = 225_003, Level = LogLevel.Error)]
    public static partial void ExceptionsLogPageLoadException(this ILogger logger, PageLoadException exception);

    [LoggerMessage(EventId = 225_004, Level = LogLevel.Error)]
    public static partial void ExceptionsLogSchedulerException(this ILogger logger, SchedulerException exception);

    [LoggerMessage(EventId = 225_005, Level = LogLevel.Error)]
    public static partial void ExceptionsLogSecurityException(this ILogger logger, SecurityException exception);

    [LoggerMessage(EventId = 225_006, Level = LogLevel.Error)]
    public static partial void ExceptionsLogGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 225_007, Level = LogLevel.Error)]
    public static partial void ExceptionsProcessSchedulerException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 225_008, Level = LogLevel.Error)]
    public static partial void ExceptionsLogSearchException(this ILogger logger, SearchException exception);

    [LoggerMessage(EventId = 225_009, Level = LogLevel.Error, Message = "{URL}")]
    public static partial void ExceptionsProcessPageLoadExceptionWithUrl(this ILogger logger, Exception exception, string url);

    [LoggerMessage(EventId = 225_010, Level = LogLevel.Error, Message = "{ResourceNotFound}: - {URL}")]
    public static partial void ExceptionsProcessHttpException(this ILogger logger, Exception exception, string resourceNotFound, string url);

    [LoggerMessage(EventId = 225_011, Level = LogLevel.Critical)]
    public static partial void ExceptionsProcessModuleLoadExceptionUnexpectedException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Exceptions.SecurityException (225,100 to 225,199)
    [LoggerMessage(EventId = 225_100, Level = LogLevel.Error)]
    public static partial void SecurityExceptionInitializeProviderVariablesException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Exceptions.BasePortalException (225,200 to 225,299)
    [LoggerMessage(EventId = 225_200, Level = LogLevel.Error)]
    public static partial void BasePortalExceptionExceptionGettingDataProviderType(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 225_201, Level = LogLevel.Error)]
    public static partial void BasePortalExceptionExceptionGettingStackTrace(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 225_202, Level = LogLevel.Error)]
    public static partial void BasePortalExceptionExceptionGettingMessage(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 225_203, Level = LogLevel.Error)]
    public static partial void BasePortalExceptionExceptionGettingSource(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 225_204, Level = LogLevel.Error)]
    public static partial void BasePortalExceptionInitializePrivateVariablesException(this ILogger logger, Exception exception);

    /*
     * 230,000 to 239,999
     * DotNetNuke.Services.FileSystem namespace
     * DotNetNuke.Services.FileSystem.EventArgs namespace
     * DotNetNuke.Services.FileSystem.FolderMappings namespace
     * DotNetNuke.Services.FileSystem.Internal namespace
     * DotNetNuke.Services.FileSystem.Internal.SecurityCheckers namespace
     */

    // DotNetNuke.Services.FileSystem.FolderManager (230,000 to 230,099)
    [LoggerMessage(EventId = 230_000, Level = LogLevel.Information, Message = "{Message}")]
    public static partial void FolderManagerInvalidFileExtensionException(this ILogger logger, string message);

    [LoggerMessage(EventId = 230_001, Level = LogLevel.Error)]
    public static partial void FolderManagerAddFolderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_002, Level = LogLevel.Error)]
    public static partial void FolderManagerDeleteFolderException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_003, Level = LogLevel.Error)]
    public static partial void FolderManagerGetFileSystemFoldersRecursiveException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_004, Level = LogLevel.Error)]
    public static partial void FolderManagerRemoveOrphanedFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_005, Level = LogLevel.Error)]
    public static partial void FolderManagerAddOrUpdateFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_006, Level = LogLevel.Error)]
    public static partial void FolderManagerSynchronizeFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_007, Level = LogLevel.Error)]
    public static partial void FolderManagerDeleteFolderInternalException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_008, Level = LogLevel.Error, Message = "Could not create folder {FolderPath}. EXCEPTION: {Message}")]
    public static partial void FolderManagerCouldNotCreateFolder(this ILogger logger, Exception exception, string folderPath, string message);

    // DotNetNuke.Services.FileSystem.FileManager (230,100 to 230,199)
    [LoggerMessage(EventId = 230_100, Level = LogLevel.Warning)]
    public static partial void FileManagerExtractFilesPermissionsNotMet(this ILogger logger, PermissionsNotMetException exception);

    [LoggerMessage(EventId = 230_101, Level = LogLevel.Warning)]
    public static partial void FileManagerExtractFilesNoSpaceAvailable(this ILogger logger, NoSpaceAvailableException exception);

    [LoggerMessage(EventId = 230_102, Level = LogLevel.Warning)]
    public static partial void FileManagerExtractFilesInvalidFileExtension(this ILogger logger, InvalidFileExtensionException exception);

    [LoggerMessage(EventId = 230_103, Level = LogLevel.Error)]
    public static partial void FileManagerAddFileLockedException(this ILogger logger, FileLockedException exception);

    [LoggerMessage(EventId = 230_104, Level = LogLevel.Error)]
    public static partial void FileManagerAddFileGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_105, Level = LogLevel.Error)]
    public static partial void FileManagerCopyFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_106, Level = LogLevel.Error)]
    public static partial void FileManagerFileExistsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_107, Level = LogLevel.Error)]
    public static partial void FileManagerGetFileStreamException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_108, Level = LogLevel.Error)]
    public static partial void FileManagerGetFileUrlException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_109, Level = LogLevel.Error)]
    public static partial void FileManagerRenameFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_110, Level = LogLevel.Error)]
    public static partial void FileManagerSetAttributesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_111, Level = LogLevel.Error)]
    public static partial void FileManagerUpdateSizeAndModificationTimeException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_112, Level = LogLevel.Error)]
    public static partial void FileManagerUpdateExtractFilesGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_113, Level = LogLevel.Error)]
    public static partial void FileManagerWriteToStreamException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_114, Level = LogLevel.Error)]
    public static partial void FileManagerWriteStreamException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_115, Level = LogLevel.Error)]
    public static partial void FileManagerDeleteFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_116, Level = LogLevel.Error)]
    public static partial void FileManagerAddFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_117, Level = LogLevel.Error)]
    public static partial void FileManagerRotateFlipImageException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.FileSystem.StandardFolderProvider (230,200 to 230,299)
    [LoggerMessage(EventId = 230_200, Level = LogLevel.Warning, Message = "{Message}")]
    public static partial void StandardFolderProviderFileStreamIOException(this ILogger logger, IOException exception, string message);

    [LoggerMessage(EventId = 230_201, Level = LogLevel.Error)]
    public static partial void StandardFolderProviderGetFileAttributesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_202, Level = LogLevel.Error)]
    public static partial void StandardFolderProviderGetLastModificationTimeException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 230_203, Level = LogLevel.Error)]
    public static partial void StandardFolderProviderFileStreamGeneralException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.FileSystem.FolderMappingsConfigController (230,300 to 230,399)
    [LoggerMessage(EventId = 230_300, Level = LogLevel.Error)]
    public static partial void FolderMappingsConfigControllerLoadConfigException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.FileSystem.FileServerHandler (230,400 to 230,499)
    [LoggerMessage(EventId = 230_400, Level = LogLevel.Error)]
    public static partial void FileServerHandlerHandleFileLinkException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.FileSystem.Internal.FileDeletionController (230,500 to 230,599)
    [LoggerMessage(EventId = 230_500, Level = LogLevel.Error)]
    public static partial void FileDeletionControllerDeleteFileException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.FileSystem.Internal.FileSecurityController (230,600 to 230,699)
    [LoggerMessage(EventId = 230_600, Level = LogLevel.Error, Message = "Create File Security Checker for '{Extension}' failed.")]
    public static partial void FileSecurityControllerCreateFileSecurityCheckerFailed(this ILogger logger, Exception exception, string extension);

    /*
     * 240,000 to 249,999
     * DotNetNuke.Services.Installer namespace
     * DotNetNuke.Services.Installer.Blocker namespace
     * DotNetNuke.Services.Installer.Dependencies namespace
     * DotNetNuke.Services.Installer.Installers namespace
     * DotNetNuke.Services.Installer.Log namespace
     * DotNetNuke.Services.Installer.Packages namespace
     * DotNetNuke.Services.Installer.Packages.WebControls namespace
     * DotNetNuke.Services.Installer.Writers namespace
     */

    // DotNetNuke.Services.Installer.Log.Logger (240,000 to 240,099)
    [LoggerMessage(EventId = 240_000, Level = LogLevel.Information, Message = "{Message}")]
    public static partial void InstallLoggerLogInfo(this ILogger logger, string message);

    [LoggerMessage(EventId = 240_001, Level = LogLevel.Warning, Message = "{Message}")]
    public static partial void InstallLoggerLogWarning(this ILogger logger, string message);

    [LoggerMessage(EventId = 240_002, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void InstallLoggerLogFailure(this ILogger logger, string message);

    // DotNetNuke.Services.Installer.Installers.CleanupInstaller (240,100 to 240,199)
    [LoggerMessage(EventId = 240_100, Level = LogLevel.Warning, Message = "Ignoring invalid cleanup folder path '{Path}' in package '{PackageName}'.")]
    public static partial void CleanupInstallerIgnoringInvalidCleanupFolderPath(this ILogger logger, string path, string packageName);

    [LoggerMessage(EventId = 240_101, Level = LogLevel.Error)]
    public static partial void CleanupInstallerCleanupFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 240_101, Level = LogLevel.Error)]
    public static partial void CleanupInstallerCleanupFolderException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Installer.Installer (240,200 to 240,299)
    [LoggerMessage(EventId = 240_200, Level = LogLevel.Error)]
    public static partial void InstallerBackupStreamInfoFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 240_201, Level = LogLevel.Error)]
    public static partial void InstallerLogInstallEventException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 240_022, Level = LogLevel.Error, Message = "Exception deleting folder {TempInstallFolder} while installing {Name}")]
    public static partial void InstallerExceptionDeletingFolderWhileInstalling(this ILogger logger, Exception exception, string tempInstallFolder, string name);

    // DotNetNuke.Services.Installer.Installers.ResourceFileInstaller (240,300 to 240,399)
    [LoggerMessage(EventId = 240_300, Level = LogLevel.Error)]
    public static partial void ResourceFileInstallerInstallFileException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Installer.Writers.ModulePackageWriter (240,400 to 240,499)
    [LoggerMessage(EventId = 240_400, Level = LogLevel.Error)]
    public static partial void ModulePackageWriterConvertControlTypeException(this ILogger logger, Exception exception);

    /*
     * 250,000 to 259,999
     * DotNetNuke.Services.Localization namespace
     * DotNetNuke.Services.Localization.Internal namespace
     * DotNetNuke.Services.Localization.Persian namespace
     */

    // DotNetNuke.Services.Localization.LocalizationProvider (250,000 to 250,099)
    [LoggerMessage(EventId = 250_000, Level = LogLevel.Warning, Message = "Missing localization key. key:{Key} resFileRoot:{ResourceFileRoot} threadCulture:{ThreadCulture} userlan:{UserLanguage}")]
    public static partial void LocalizationProviderMissingLocalizationKey(this ILogger logger, string key, string resourceFileRoot, CultureInfo threadCulture, string userLanguage);

    [LoggerMessage(EventId = 250_001, Level = LogLevel.Error)]
    public static partial void LocalizationProviderGetLocaleException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Localization.Localization (250,100 to 250,199)
    [LoggerMessage(EventId = 250_100, Level = LogLevel.Error)]
    public static partial void LocalizationGetSystemMessageException(this ILogger logger, NullReferenceException exception);

    /*
     * 260,000 to 269,999
     * DotNetNuke.Services.Log.EventLog namespace
     */

    // DotNetNuke.Services.Log.EventLog.LogController (260,000 to 260,099)
    [LoggerMessage(EventId = 260_000, Level = LogLevel.Debug)]
    public static partial void LogControllerConfigFileNotFound(this ILogger logger, FileNotFoundException exception);

    [LoggerMessage(EventId = 260_001, Level = LogLevel.Information, Message = "{LogInfo}")]
    public static partial void LogControllerLogInfo(this ILogger logger, LogInfo logInfo);

    [LoggerMessage(EventId = 260_001, Level = LogLevel.Debug)]
    public static partial void LogControllerFailureToWriteToLogFile(this ILogger logger, IOException exception);

    [LoggerMessage(EventId = 260_002, Level = LogLevel.Error, Message = "filePath={FilePath}, header={Header}, message={Message}")]
    public static partial void LogControllerRaiseError(this ILogger logger, string filePath, string header, string message);

    [LoggerMessage(EventId = 260_003, Level = LogLevel.Error)]
    public static partial void LogControllerAddLogException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 260_004, Level = LogLevel.Error)]
    public static partial void LogControllerAddLogToFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 260_005, Level = LogLevel.Error, Message = "Unable to retrieve HttpContext.Request, ignoring LogUserName")]
    public static partial void LogControllerUnableToRetrieveRequestIgnoringLogUserName(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Log.EventLog.DBLoggingProvider (260,100 to 260,199)
    [LoggerMessage(EventId = 260_100, Level = LogLevel.Error)]
    public static partial void DbLoggingProviderFillLogInfoException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 260_101, Level = LogLevel.Error)]
    public static partial void DbLoggingProviderWriteLogSqlException(this ILogger logger, SqlException exception);

    [LoggerMessage(EventId = 260_102, Level = LogLevel.Error)]
    public static partial void DbLoggingProviderWriteLogGeneralException(this ILogger logger, Exception exception);

    /*
     * 270,000 to 279,999
     * DotNetNuke.Services.Mail namespace
     * DotNetNuke.Services.Mail.OAuth namespace
     */

    // DotNetNuke.Services.Mail.SendTokenizedBulkEmail (270,000 to 270,099)
    [LoggerMessage(EventId = 270_000, Level = LogLevel.Error)]
    public static partial void SendTokenizedBulkEmailSendMailsException(this ILogger logger, Exception exception);

    /*
     * 290,000 to 299,999
     * DotNetNuke.Services.Cache namespace
     * DotNetNuke.Services.ModuleCache namespace
     * DotNetNuke.Services.OutputCache namespace
     * DotNetNuke.Services.OutputCache.Providers namespace
     */

    // DotNetNuke.Services.ModuleCache.PurgeModuleCache (290,000 to 290,099)
    [LoggerMessage(EventId = 290_000, Level = LogLevel.Debug)]
    public static partial void PurgeModuleCachePurgeNotSupportedException(this ILogger logger, NotSupportedException exception);

    // DotNetNuke.Services.OutputCache.PurgeOutputCache (290,100 to 290,199)
    [LoggerMessage(EventId = 290_100, Level = LogLevel.Debug)]
    public static partial void PurgeOutputCachePurgeNotSupportedException(this ILogger logger, NotSupportedException exception);

    // DotNetNuke.Services.Cache.CachingProvider (290,200 to 290,299)
    [LoggerMessage(EventId = 290_200, Level = LogLevel.Warning, Message = "Disable cache expiration.")]
    public static partial void CachingProviderDisableCacheExpiration(this ILogger logger);

    [LoggerMessage(EventId = 290_201, Level = LogLevel.Warning, Message = "Enable cache expiration.")]
    public static partial void CachingProviderEnableCacheExpiration(this ILogger logger);

    // DotNetNuke.Services.Cache.FBCachingProvider (290,300 to 290,399)
    [LoggerMessage(EventId = 290_300, Level = LogLevel.Error)]
    public static partial void FbCachingProviderPurgeDeleteFileException(this ILogger logger, Exception exception);

    /*
     * 300,000 to 309,999
     * DotNetNuke.Services.Search namespace
     * DotNetNuke.Services.Search.Controllers namespace
     * DotNetNuke.Services.Search.Entities namespace
     * DotNetNuke.Services.Search.Internals namespace
     */

    // DotNetNuke.Services.Search.Internals.LuceneControllerImpl (300,000 to 300,099)
    [LoggerMessage(EventId = 300_000, Level = LogLevel.Trace, Message = "Query: {Query}\n{Explanation}")]
    public static partial void LuceneControllerSearchResultExplanation(this ILogger logger, Query query, string explanation);

    [LoggerMessage(EventId = 300_001, Level = LogLevel.Debug, Message = "Compacting Search Index - started")]
    public static partial void LuceneControllerCompactingSearchIndexStarted(this ILogger logger);

    [LoggerMessage(EventId = 300_002, Level = LogLevel.Debug, Message = "Compacting Search Index - finished")]
    public static partial void LuceneControllerCompactingSearchIndexFinished(this ILogger logger);

    [LoggerMessage(EventId = 300_003, Level = LogLevel.Error)]
    public static partial void LuceneControllerSearchException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 300_004, Level = LogLevel.Error)]
    public static partial void LuceneControllerGetCustomAnalyzerException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 300_005, Level = LogLevel.Error, Message = "Search Index Folder Is Not Available: {Message}, Retry {Retries} time(s).")]
    public static partial void LuceneControllerSearchIndexFolderIsNotAvailable(this ILogger logger, Exception exception, string message, int retries);

    // DotNetNuke.Services.Search.ModuleIndexer (300,100 to 300,199)
    [LoggerMessage(EventId = 300_100, Level = LogLevel.Trace, Message = "ModuleIndexer: {Count} search documents found for module [{DesktopModuleName} mid:{ModuleId}]")]
    public static partial void ModuleIndexerSearchDocumentsFoundForModule(this ILogger logger, int count, string desktopModuleName, int moduleId);

    [LoggerMessage(EventId = 300_101, Level = LogLevel.Trace, Message = "ModuleIndexer: Search document for metaData found for module [{DesktopModuleName} mid:{ModuleId}]")]
    public static partial void ModuleIndexerSearchDocumentForMetadataFoundForModule(this ILogger logger, string desktopModuleName, int moduleId);

    [LoggerMessage(EventId = 300_102, Level = LogLevel.Error)]
    public static partial void ModuleIndexerGetModulesForIndexException(this ILogger logger, Exception exception);

    // DotNetNuke.Services.Search.TabIndexer (300,200 to 300,299)
    [LoggerMessage(EventId = 300_200, Level = LogLevel.Trace, Message = "TabIndexer: Search document for metaData added for page [{Title} tid:{TabId}]")]
    public static partial void TabIndexerPageMetadataDocumentAdded(this ILogger logger, string title, int tabId);

    // DotNetNuke.Services.Search.SearchEngineScheduler (300,300 to 300,399)
    [LoggerMessage(EventId = 300_300, Level = LogLevel.Trace, Message = "Search: Site Crawler - Starting. Content change start time {LastSuccessfulDateTime}")]
    public static partial void SearchEngineSchedulerStarting(this ILogger logger, DateTime lastSuccessfulDateTime);

    [LoggerMessage(EventId = 300_301, Level = LogLevel.Trace, Message = "Search: Site Crawler - Indexing Successful")]
    public static partial void SearchEngineSchedulerSuccessful(this ILogger logger);

    // DotNetNuke.Services.Search.SearchEngineIndexer (300,400 to 300,499)
    [LoggerMessage(EventId = 300_400, Level = LogLevel.Warning, Message = "Indexer not implemented")]
    public static partial void SearchEngineIndexerNotImplemented(this ILogger logger, NotImplementedException exception);

    // DotNetNuke.Services.Search.Internals.InternalSearchControllerImpl (300,500 to 300,599)
    [LoggerMessage(EventId = 300_500, Level = LogLevel.Error, Message = "Search Document error: {SearchDocument}")]
    public static partial void InternalSearchControllerSearchDocumentError(this ILogger logger, Exception exception, SearchDocument searchDocument);

    // DotNetNuke.Services.Search.Controllers.ModuleResultController (300,600 to 300,699)
    [LoggerMessage(EventId = 300_600, Level = LogLevel.Error)]
    public static partial void ModuleResultControllerGetModuleSearchUrlException(this ILogger logger, Exception exception);

    /*
     * 310,000 to 319,999
     * DotNetNuke.Services.Upgrade namespace
     * DotNetNuke.Services.Upgrade.InternalController.Steps namespace
     * DotNetNuke.Services.Upgrade.Internals namespace
     * DotNetNuke.Services.Upgrade.Internals.InstallConfiguration namespace
     * DotNetNuke.Services.Upgrade.Internals.Steps namespace
     */

    // DotNetNuke.Services.Upgrade.Internals.Steps.AddFcnModeStep (310,000 to 310,099)
    [LoggerMessage(EventId = 310_000, Level = LogLevel.Trace, Message = "Adding FcnMode : {ErrorMessage}")]
    public static partial void AddFcnModeStepAddingFcnMode(this ILogger logger, string errorMessage);

    // DotNetNuke.Services.Upgrade.InternalController.Steps.FilePermissionCheckStep (310,100 to 310,199)
    [LoggerMessage(EventId = 310_100, Level = LogLevel.Trace, Message = "FilePermissionCheck - {Details}")]
    public static partial void FilePermissionCheckStepCheck(this ILogger logger, string details);

    [LoggerMessage(EventId = 310_101, Level = LogLevel.Trace, Message = "FilePermissionCheck Status - {Status}")]
    public static partial void FilePermissionCheckStepStatus(this ILogger logger, StepStatus status);

    // DotNetNuke.Services.Upgrade.Internals.Steps.InstallVersionStep (310,200 to 310,299)
    [LoggerMessage(EventId = 310_200, Level = LogLevel.Trace, Message = "Adding InstallVersion : {ErrorMessage}")]
    public static partial void InstallVersionStepAddingInstallVersion(this ILogger logger, string errorMessage);

    // DotNetNuke.Services.Upgrade.Upgrade (310,300 to 310,399)
    [LoggerMessage(EventId = 310_300, Level = LogLevel.Trace, Message = "GetUpgradedScripts databaseVersion:{DatabaseVersion} applicationVersion:{ApplicationVersion}")]
    public static partial void UpgradeGetUpgradedScripts(this ILogger logger, Version databaseVersion, Version applicationVersion);

    [LoggerMessage(EventId = 310_301, Level = LogLevel.Trace, Message = "GetUpgradedScripts including {File}")]
    public static partial void UpgradeGetUpgradedScriptsIncluding(this ILogger logger, string file);

    [LoggerMessage(EventId = 310_302, Level = LogLevel.Error)]
    public static partial void UpgradeAddModuleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_303, Level = LogLevel.Error)]
    public static partial void UpgradeAddPortalException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_304, Level = LogLevel.Error)]
    public static partial void UpgradeDeleteFilesException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_305, Level = LogLevel.Error)]
    public static partial void UpgradeExceptionDeletingScriptFileAfterExecution(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_306, Level = LogLevel.Error)]
    public static partial void UpgradeExceptionDeletingPackageFileAfterInstallation(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_307, Level = LogLevel.Error)]
    public static partial void UpgradeExceptionInUpgradeApplication(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_308, Level = LogLevel.Error)]
    public static partial void UpgradeExceptionLoggingException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_309, Level = LogLevel.Error, Message = "{Version}")]
    public static partial void UpgradeExceptionDuringVersionSpecificUpgrade(this ILogger logger, Exception exception, Version version);

    [LoggerMessage(EventId = 310_310, Level = LogLevel.Error, Message = "{Version}")]
    public static partial void UpgradeExceptionWritingExceptionLogForVersionSpecificUpgrade(this ILogger logger, Exception exception, Version version);

    [LoggerMessage(EventId = 310_311, Level = LogLevel.Error)]
    public static partial void UpgradeExceptionUpdatingConfig(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_312, Level = LogLevel.Error)]
    public static partial void UpgradeExceptionLoggingExceptionFromUpdatingConfig(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_313, Level = LogLevel.Error)]
    public static partial void UpgradeUpdateNewtonsoftVersionException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_314, Level = LogLevel.Error)]
    public static partial void UpgradeCreateExecuteScriptLogException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_315, Level = LogLevel.Error)]
    public static partial void UpgradeCreateMemberRoleProviderLogException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_316, Level = LogLevel.Error)]
    public static partial void UpgradeRemoveGettingStartedPageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_317, Level = LogLevel.Error)]
    public static partial void UpgradeFixFipsComplianceAssemblyException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_318, Level = LogLevel.Error)]
    public static partial void UpgradeFindLanguageXmlDocumentException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 310_319, Level = LogLevel.Error, Message = "Error cleanup file {ListFile}")]
    public static partial void UpgradeErrorCleanupFile(this ILogger logger, Exception exception, string listFile);

    [LoggerMessage(EventId = 310_320, Level = LogLevel.Error, Message = "File deletion failed for [Install/{File}]. PLEASE REMOVE THIS MANUALLY.")]
    public static partial void UpgradeFileDeletionFailedFor(this ILogger logger, Exception exception, string file);

    [LoggerMessage(EventId = 310_321, Level = LogLevel.Error, Message = "{LogDescription}")]
    public static partial void UpgradeFailureLog(this ILogger logger, string logDescription);

    // DotNetNuke.Services.Upgrade.InternalController.Steps.InstallExtensionsStep (310,400 to 310,499)
    [LoggerMessage(EventId = 310_400, Level = LogLevel.Trace, Message = "{Details}")]
    public static partial void InstallExtensionsStepInstallingExtensionPackage(this ILogger logger, string details);

    // DotNetNuke.Services.Upgrade.InternalController.Steps.UpdateLanguagePackStep (310,500 to 310,599)
    [LoggerMessage(EventId = 310_500, Level = LogLevel.Error)]
    public static partial void UpdateLanguagePackStepExecuteException(this ILogger logger, Exception exception);

    /*
     * 601,000 to 601,999
     * DotNetNuke.UI.Containers namespace
     * DotNetNuke.UI.Containers.EventListeners namespace
     */

    // DotNetNuke.UI.Containers.Container (601,000 to 601,099)
    [LoggerMessage(EventId = 601_000, Level = LogLevel.Debug, Message = "Container.ProcessModule Start (TabId:{TabId},ModuleID: {DesktopModuleId}): Module FriendlyName: '{ModuleFriendlyName}')")]
    public static partial void ContainerProcessModuleStart(this ILogger logger, int tabId, int desktopModuleId, string moduleFriendlyName);

    [LoggerMessage(EventId = 601_001, Level = LogLevel.Debug, Message = "Container.ProcessModule Info (TabId:{TabId},ModuleID: {DesktopModuleId}): ControlPane.Controls.Add(ModuleHost:{ModuleHostId})")]
    public static partial void ContainerProcessModuleInfo(this ILogger logger, int tabId, int desktopModuleId, string moduleHostId);

    [LoggerMessage(EventId = 601_002, Level = LogLevel.Debug, Message = "Container.ProcessModule End (TabId:{TabId},ModuleID: {DesktopModuleId}): Module FriendlyName: '{ModuleFriendlyName}')")]
    public static partial void ContainerProcessModuleEnd(this ILogger logger, int tabId, int desktopModuleId, string moduleFriendlyName);

    /*
     * 603,000 to 603,999
     * DotNetNuke.UI.Modules namespace
     * DotNetNuke.UI.Modules.Html5 namespace
     */

    // DotNetNuke.UI.Modules.ModuleControlFactory (603,000 to 603,099)
    [LoggerMessage(EventId = 603_000, Level = LogLevel.Debug, Message = "ModuleControlFactory.LoadModuleControl Start (TabId:{TabId},ModuleId:{ModuleId}): ModuleControlSource:{ModuleControlSource}")]
    public static partial void ModuleControlFactoryLoadModuleControlStart(this ILogger logger, int tabId, int moduleId, string moduleControlSource);

    [LoggerMessage(EventId = 603_001, Level = LogLevel.Debug, Message = "ModuleControlFactory.LoadModuleControl End (TabId:{TabId},ModuleId:{ModuleId}): ModuleControlSource:{ModuleControlSource}")]
    public static partial void ModuleControlFactoryLoadModuleControlEnd(this ILogger logger, int tabId, int moduleId, string moduleControlSource);

    [LoggerMessage(EventId = 603_002, Level = LogLevel.Debug, Message = "ModuleControlFactory.LoadSettingsControl Start (TabId:{TabId},ModuleId:{ModuleId}): ModuleControlSource:{ModuleControlSource}")]
    public static partial void ModuleControlFactoryLoadSettingsControlStart(this ILogger logger, int tabId, int moduleId, string moduleControlSource);

    [LoggerMessage(EventId = 603_003, Level = LogLevel.Debug, Message = "ModuleControlFactory.LoadSettingsControl End (TabId:{TabId},ModuleId:{ModuleId}): ModuleControlSource:{ModuleControlSource}")]
    public static partial void ModuleControlFactoryLoadSettingsControlEnd(this ILogger logger, int tabId, int moduleId, string moduleControlSource);

    // DotNetNuke.UI.Modules.ModuleHost (603,100 to 603,199)
    [LoggerMessage(EventId = 603_100, Level = LogLevel.Debug)]
    public static partial void ModuleHostThreadAbortException(this ILogger logger, ThreadAbortException exception);

    [LoggerMessage(EventId = 603_101, Level = LogLevel.Error)]
    public static partial void ModuleHostLoadModuleControlException(this ILogger logger, Exception exception);

    /*
     * 604,000 to 604,999
     * DotNetNuke.UI.Skins namespace
     * DotNetNuke.UI.Skins.Controls namespace
     * DotNetNuke.UI.Skins.Controls.EventListeners namespace
     */

    // DotNetNuke.UI.Skins.SkinThumbNailControl (604,000 to 604,099)
    [LoggerMessage(EventId = 604_000, Level = LogLevel.Error)]
    public static partial void SkinThumbNailControlCreateThumbnailException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.Skins.SkinFileProcessor (604,100 to 604,199)
    [LoggerMessage(EventId = 604_100, Level = LogLevel.Error)]
    public static partial void SkinFileProcessorLoadXmlFileException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.Skins.SkinFileProcessor+SkinFile (604,200 to 604,299)
    [LoggerMessage(EventId = 604_200, Level = LogLevel.Error)]
    public static partial void SkinFileLoadXmlFileException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.Skins.SkinController (604,300 to 604,399)
    [LoggerMessage(EventId = 604_300, Level = LogLevel.Error)]
    public static partial void SkinControllerExceptionLoggingInstallationEvent(this ILogger logger, Exception exception);

    /*
     * 607,000 to 607,999
     * DotNetNuke.UI.WebControls namespace
     * DotNetNuke.UI.WebControls.Internal namespace
     */

    // DotNetNuke.UI.WebControls.CaptchaControl (607,000 to 607,099)
    [LoggerMessage(EventId = 607_000, Level = LogLevel.Debug)]
    public static partial void CaptchaControlDecryptException(this ILogger logger, ArgumentException exception);

    [LoggerMessage(EventId = 607_001, Level = LogLevel.Error)]
    public static partial void CaptchaControlCreateTextException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 607_002, Level = LogLevel.Error)]
    public static partial void CaptchaControlGetFontException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.WebControls.SettingInfo (607,100 to 607,199)
    [LoggerMessage(EventId = 607_100, Level = LogLevel.Error)]
    public static partial void SettingInfoBoolParseException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 607_101, Level = LogLevel.Error)]
    public static partial void SettingInfoInt32ParseException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.WebControls.TrueFalseEditControl (607,200 to 607,299)
    [LoggerMessage(EventId = 607_200, Level = LogLevel.Error)]
    public static partial void TrueFalseEditControlBooleanValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 607_201, Level = LogLevel.Error)]
    public static partial void TrueFalseEditControlOldBooleanValueException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.WebControls.IntegerEditControl (607,300 to 607,399)
    [LoggerMessage(EventId = 607_300, Level = LogLevel.Error)]
    public static partial void IntegerEditControlIntegerValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 607_301, Level = LogLevel.Error)]
    public static partial void IntegerEditControlOldIntegerValueException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.WebControls.DNNListEditControl (607,400 to 607,499)
    [LoggerMessage(EventId = 607_400, Level = LogLevel.Error)]
    public static partial void DnnListEditControlIntegerValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 607_401, Level = LogLevel.Error)]
    public static partial void DnnListEditControlOldIntegerValueException(this ILogger logger, Exception exception);

    // DotNetNuke.UI.WebControls.DateEditControl (607,500 to 607,599)
    [LoggerMessage(EventId = 607_500, Level = LogLevel.Error)]
    public static partial void DateEditControlDateValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 607_501, Level = LogLevel.Error)]
    public static partial void DateEditControlOldDateValueException(this ILogger logger, Exception exception);
}
