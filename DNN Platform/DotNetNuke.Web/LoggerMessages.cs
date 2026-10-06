// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web;

using System;
using System.IO;

using DotNetNuke.Entities.Users;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The DotNetNuke.Web project has been assigned event IDs from 1,200,000 to 1,499,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 1_200_000, Message = "{Message}")]
    public static partial void TraceWriterLogMessage(this ILogger logger, LogLevel logLevel, string message);

    [LoggerMessage(EventId = 1_200_100, Level = LogLevel.Information, Message = "Watcher Activity: {ChangeType}. Path: {FullPath}")]
    public static partial void ShutdownOverloadWatcherActivity(this ILogger logger, WatcherChangeTypes changeType, string fullPath);

    [LoggerMessage(EventId = 1_200_101, Level = LogLevel.Information, Message = "Watcher Activity: {ChangeType}. New Path: {NewPath}. Old Path: {OldPath}")]
    public static partial void ShutdownOverloadWatcherRenamedActivity(this ILogger logger, WatcherChangeTypes changeType, string newPath, string oldPath);

    [LoggerMessage(EventId = 1_200_102, Level = LogLevel.Information, Message = "Watcher Activity: N/A. Error:")]
    public static partial void ShutdownOverloadWatcherError(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_103, Level = LogLevel.Information)]
    public static partial void ShutdownOverloadInitializeFcnSettingsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_104, Level = LogLevel.Information, Message = "fileChangesMonitor is null")]
    public static partial void ShutdownOverloadFileChangesMonitorIsNull(this ILogger logger);

    [LoggerMessage(EventId = 1_200_105, Level = LogLevel.Information, Message = "FCNMode = {fcnMode} (Modes: NotSet/Default=0, Disabled=1, Single=2)")]
    public static partial void ShutdownOverloadFileChangeNotificationMode(this ILogger logger, object fcnMode);

    [LoggerMessage(EventId = 1_200_106, Level = LogLevel.Trace, Message = "DirMonCompletion count: {Count}")]
    public static partial void ShutdownOverloadDirMonCompletionCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 1_200_107, Level = LogLevel.Trace, Message = @"Added watcher for: {WatcherPath}/{WatcherFilter}")]
    public static partial void ShutdownOverloadAddedWatcherFor(this ILogger logger, string watcherPath, string watcherFilter);

    [LoggerMessage(EventId = 1_200_108, Level = LogLevel.Trace, Message = "Error adding our own file monitoring object.")]
    public static partial void ShutdownOverloadErrorAddingOurOwnFileMonitoringObject(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_109, Level = LogLevel.Error)]
    public static partial void ShutdownOverloadUnloadAppDomainException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_200, Level = LogLevel.Information, Message = "Application Starting ({ElapsedSinceAppStart})")]
    public static partial void ApplicationStarting(this ILogger logger, TimeSpan elapsedSinceAppStart);

    [LoggerMessage(EventId = 1_200_201, Level = LogLevel.Information, Message = "Application Started ({ElapsedSinceAppStart})")]
    public static partial void ApplicationStarted(this ILogger logger, TimeSpan elapsedSinceAppStart);

    [LoggerMessage(EventId = 1_200_202, Level = LogLevel.Information, Message = "Application Ending")]
    public static partial void ApplicationEnding(this ILogger logger);

    [LoggerMessage(EventId = 1_200_203, Level = LogLevel.Information, Message = "Application Ended")]
    public static partial void ApplicationEnded(this ILogger logger);

    [LoggerMessage(EventId = 1_200_204, Level = LogLevel.Trace, Message = "Disposing Lucene")]
    public static partial void ApplicationDisposingLucene(this ILogger logger);

    [LoggerMessage(EventId = 1_200_205, Level = LogLevel.Trace, Message = "Dumping all Application Errors")]
    public static partial void ApplicationDumpingAllApplicationErrors(this ILogger logger);

    [LoggerMessage(EventId = 1_200_206, Level = LogLevel.Trace, Message = "End Dumping all Application Errors")]
    public static partial void ApplicationEndDumpingAllApplicationErrors(this ILogger logger);

    [LoggerMessage(EventId = 1_200_207, Level = LogLevel.Error)]
    public static partial void ApplicationLogEndException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_208, Level = LogLevel.Error)]
    public static partial void ApplicationStopSchedulerException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_209, Level = LogLevel.Critical)]
    public static partial void ApplicationLogApplicationError(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_300, Level = LogLevel.Trace, Message = "Authorization header scheme in the request is not equal to {AuthScheme}")]
    public static partial void ApiTokenControllerAuthorizationHeaderSchemeDoesNotMatchAuthScheme(this ILogger logger, string authScheme);

    [LoggerMessage(EventId = 1_200_301, Level = LogLevel.Trace, Message = "Missing authorization header value in the request")]
    public static partial void ApiTokenControllerMissingAuthorizationHeaderValue(this ILogger logger);

    [LoggerMessage(EventId = 1_200_302, Level = LogLevel.Trace, Message = "Token expired")]
    public static partial void ApiTokenControllerTokenExpired(this ILogger logger);

    [LoggerMessage(EventId = 1_200_303, Level = LogLevel.Trace, Message = "Invalid user")]
    public static partial void ApiTokenControllerInvalidUser(this ILogger logger);

    [LoggerMessage(EventId = 1_200_304, Level = LogLevel.Trace, Message = "{SchemeType} is not registered/enabled in web.config file")]
    public static partial void ApiTokenControllerSchemeIsNotEnabledInWebConfig(this ILogger logger, string schemeType);

    [LoggerMessage(EventId = 1_200_400, Level = LogLevel.Trace, Message = "Authenticated using API token {ApiTokenId}")]
    public static partial void ApiTokenAuthMessageHandlerAuthenticatedUsingApiToken(this ILogger logger, int apiTokenId);

    [LoggerMessage(EventId = 1_200_401, Level = LogLevel.Error, Message = "Unexpected error authenticating API Token.")]
    public static partial void ApiTokenAuthMessageHandlerUnexpectedErrorAuthenticatingApiToken(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_500, Level = LogLevel.Trace, Message = "{AuthScheme}: Validating request vs. SSL mode ({ForceSsl}) failed. ")]
    public static partial void AuthMessageHandlerBaseValidatingRequestVsSslModeFailed(this ILogger logger, string authScheme, bool forceSsl);

    [LoggerMessage(EventId = 1_200_600, Level = LogLevel.Trace, Message = "Mapping route: {FullRouteName} @ {RouteUrl}")]
    public static partial void ServicesRoutingManagerMappingRoute(this ILogger logger, string fullRouteName, string routeUrl);

    [LoggerMessage(EventId = 1_200_601, Level = LogLevel.Trace, Message = "Mapping route: {OldRouteName} @ {OldRouteUrl}")]
    public static partial void ServicesRoutingManagerMappingOldRoute(this ILogger logger, string oldRouteName, string oldRouteUrl);

    [LoggerMessage(EventId = 1_200_602, Level = LogLevel.Trace, Message = "Registered a total of {Count} routes")]
    public static partial void ServicesRoutingManagerRegisteredRoutes(this ILogger logger, int count);

    [LoggerMessage(EventId = 1_200_603, Level = LogLevel.Trace, Message = "The following handler is disabled {ClassName}")]
    public static partial void ServicesRoutingManagerHandlerIsDisabled(this ILogger logger, string className);

    [LoggerMessage(EventId = 1_200_604, Level = LogLevel.Trace, Message = "The following handler scheme '{ClassName}' is already added and will be skipped")]
    public static partial void ServicesRoutingManagerHandlerIsAlreadyAdded(this ILogger logger, string className);

    [LoggerMessage(EventId = 1_200_605, Level = LogLevel.Trace, Message = "Instantiated/Activated instance of {AuthScheme}, class: {ClassFullName}")]
    public static partial void ServicesRoutingManagerHandlerIsActivated(this ILogger logger, string authScheme, string classFullName);

    [LoggerMessage(EventId = 1_200_606, Level = LogLevel.Error, Message = "{FullTypeName}.RegisterRoutes threw an exception.")]
    public static partial void ServicesRoutingManagerRegisterRoutesThrewAnException(this ILogger logger, Exception exception, string fullTypeName);

    [LoggerMessage(EventId = 1_200_607, Level = LogLevel.Error, Message = "Unable to create {fullTypeName} while registering service routes.")]
    public static partial void ServicesRoutingManagerUnableToCreateRouteMapper(this ILogger logger, Exception exception, string fullTypeName);

    [LoggerMessage(EventId = 1_200_608, Level = LogLevel.Error, Message = "Cannot instantiate/activate instance of {ClassName}")]
    public static partial void ServicesRoutingManagerCannotInstantiateInstanceOf(this ILogger logger, Exception exception, string className);

    [LoggerMessage(EventId = 1_200_700, Level = LogLevel.Warning, Message = "The specified moniker ({Moniker}) is not defined in the system")]
    public static partial void StandardTabAndModuleInfoProviderMonikerIsNotDefined(this ILogger logger, string moniker);

    [LoggerMessage(EventId = 1_200_800, Level = LogLevel.Warning, Message = "Unable to create thumbnail for {PhysicalPath}")]
    public static partial void DnnFilePickerUnableToCreateThumbnail(this ILogger logger, string physicalPath);

    [LoggerMessage(EventId = 1_200_801, Level = LogLevel.Error)]
    public static partial void DnnFilePickerAddFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_900, Level = LogLevel.Warning, Message = "Unable to get image dimensions for image file")]
    public static partial void FileUploadControllerUnableToGetImageDimensions(this ILogger logger, ArgumentException exception);

    [LoggerMessage(EventId = 1_200_901, Level = LogLevel.Error)]
    public static partial void FileUploadControllerSaveFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_200_902, Level = LogLevel.Error)]
    public static partial void FileUploadControllerUploadFileException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_000, Level = LogLevel.Warning, Message = "While loading IDnnStartup types, the following assemblies had types that could not be loaded. This is only an issue if these types contain DNN startup logic that could not be loaded:\n{Message}")]
    public static partial void DependencyInjectionInitializeAssembliesCouldNotBeLoaded(this ILogger logger, string message);

    [LoggerMessage(EventId = 1_201_001, Level = LogLevel.Error, Message = "Unable to configure services for {FullTypeName}, see exception for details")]
    public static partial void DependencyInjectionInitializeUnableToConfigureServicesFor(this ILogger logger, Exception exception, string fullTypeName);

    [LoggerMessage(EventId = 1_201_001, Level = LogLevel.Error, Message = "Unable to instantiate startup code for {FullTypeName}")]
    public static partial void DependencyInjectionInitializeUnableToInstantiateStartupCodeFor(this ILogger logger, Exception exception, string fullTypeName);

    [LoggerMessage(EventId = 1_201_100, Level = LogLevel.Error)]
    public static partial void BuildUpExtensionsSetValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_200, Level = LogLevel.Error)]
    public static partial void UserFileControllerGetItemsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_300, Level = LogLevel.Error)]
    public static partial void ControlBarControllerParseVisibilityException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_301, Level = LogLevel.Error)]
    public static partial void ControlBarControllerParseSortException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_302, Level = LogLevel.Error)]
    public static partial void ControlBarControllerParseModuleIdException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_303, Level = LogLevel.Error)]
    public static partial void ControlBarControllerParsePageIdException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_304, Level = LogLevel.Error)]
    public static partial void ControlBarControllerAddModuleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_400, Level = LogLevel.Error)]
    public static partial void EventLogServiceControllerGetLogDetailsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_500, Level = LogLevel.Error)]
    public static partial void MessagingServiceControllerWaitTimeForNextMessageException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_501, Level = LogLevel.Error)]
    public static partial void MessagingServiceControllerCreateException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_502, Level = LogLevel.Error)]
    public static partial void MessagingServiceControllerSearchException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_600, Level = LogLevel.Error)]
    public static partial void NotificationsServiceControllerDismissException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_700, Level = LogLevel.Error)]
    public static partial void RelationshipServiceControllerAcceptFriendException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_701, Level = LogLevel.Error)]
    public static partial void RelationshipServiceControllerFollowBackUserRelationshipExistsException(this ILogger logger, UserRelationshipExistsException exception);

    [LoggerMessage(EventId = 1_201_702, Level = LogLevel.Error)]
    public static partial void RelationshipServiceControllerFollowBackGeneralException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_800, Level = LogLevel.Error)]
    public static partial void ItemListServiceControllerSearchUserException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_201_900, Level = LogLevel.Error)]
    public static partial void RibbonBarManagerAddOrUpdateTabException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_202_000, Level = LogLevel.Error)]
    public static partial void DateTimeEditControlDateValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_202_001, Level = LogLevel.Error)]
    public static partial void DateTimeEditControlOldDateValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_203_000, Level = LogLevel.Error)]
    public static partial void DateEditControlDateValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_203_001, Level = LogLevel.Error)]
    public static partial void DateEditControlOldDateValueException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1_203_100, Level = LogLevel.Error, Message = "Cannot find module ID {ModuleId} (tab ID {TabId}, portal ID {PortalId})")]
    public static partial void ModuleServiceControllerCannotFindModule(this ILogger logger, int moduleId, int tabId, int portalId);
}
