// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library;

using System;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The Dnn.PersonaBar.Library project has been assigned event IDs from 5,000,000 to 5,499,999.</remarks>
internal static partial class LoggerMessages
{
    /*
    # Event IDs
    - 5,000,000 to 5,499,999
      - Dnn.PersonaBar.Library project
        - 5,000,000 to 5,000,999
          - Dnn.PersonaBar.Library namespace
        - 5,001,000 to 5,001,999
          - Dnn.PersonaBar.Library.AppEvents namespace
          - Dnn.PersonaBar.Library.AppEvents.Attributes namespace
        - 5,002,000 to 5,002,999
          - Dnn.PersonaBar.Library.Attributes namespace
        - 5,003,000 to 5,003,999
          - Dnn.PersonaBar.Library.Common namespace
        - 5,004,000 to 5,004,999
          - Dnn.PersonaBar.Library.Containers namespace
        - 5,005,000 to 5,005,999
          - Dnn.PersonaBar.Library.Controllers namespace
        - 5,006,000 to 5,006,999
          - Dnn.PersonaBar.Library.Data namespace
        - 5,007,000 to 5,007,999
          - Dnn.PersonaBar.Library.Dto namespace
          - Dnn.PersonaBar.Library.Dto.Tabs namespace
        - 5,008,000 to 5,008,999
          - Dnn.PersonaBar.Library.Helper namespace
        - 5,009,000 to 5,009,999
          - Dnn.PersonaBar.Library.Model namespace
        - 5,010,000 to 5,010,999
          - Dnn.PersonaBar.Library.Permissions namespace
        - 5,011,000 to 5,011,999
          - Dnn.PersonaBar.Library.Prompt namespace
          - Dnn.PersonaBar.Library.Prompt.Attributes namespace
          - Dnn.PersonaBar.Library.Prompt.Common namespace
          - Dnn.PersonaBar.Library.Prompt.Models namespace
        - 5,012,000 to 5,012,999
          - Dnn.PersonaBar.Library.Repository namespace
        - 5,013,000 to 5,013,999
          - Dnn.PersonaBar.Library.Security namespace
    */
    // Dnn.PersonaBar.Library.AppEvents.EventsController (5,001,000 to 5,001,099)
    [LoggerMessage(EventId = 5_001_000, Level = LogLevel.Information, Message = "Type \"{TypeFullName}\"'s version ({TypeVersion}) doesn't match current version({CurrentVersion}) so ignored")]
    public static partial void EventsControllerVersionMismatch(this ILogger logger, string typeFullName, Version typeVersion, Version currentVersion);

    [LoggerMessage(EventId = 5_001_001, Level = LogLevel.Error, Message = "{TypeFullName}.ApplicationStart threw an exception.")]
    public static partial void EventsControllerApplicationStartThrewAnException(this ILogger logger, Exception exception, string typeFullName);

    [LoggerMessage(EventId = 5_001_002, Level = LogLevel.Error, Message = "{TypeFullName}.ApplicationEnd threw an exception.")]
    public static partial void EventsControllerApplicationEndThrewAnException(this ILogger logger, Exception exception, string typeFullName);

    [LoggerMessage(EventId = 5_001_003, Level = LogLevel.Error, Message = "Unable to create {TypeFullName} while calling Application start implementors.")]
    public static partial void EventsControllerUnableToCreateAppEventHandler(this ILogger logger, Exception exception, string typeFullName);

    // Dnn.PersonaBar.Library.Common.IocUtil (5,003,000 to 5,003,099)
    [LoggerMessage(EventId = 5_003_000, Level = LogLevel.Warning, Message = "No instance of type '{TypeFullName}' and name '{Name}' is registered in the IOC container.")]
    public static partial void IocUtilNoInstanceOfTypeAndNameIsRegisteredInTheIocContainer(this ILogger logger, string typeFullName, string name);

    [LoggerMessage(EventId = 5_003_001, Level = LogLevel.Error)]
    public static partial void IocUtilRegisterComponentException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_003_002, Level = LogLevel.Error)]
    public static partial void IocUtilRegisterComponentInstanceException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Library.Controllers.PersonaBarController (5,005,000 to 5,005,099)
    [LoggerMessage(EventId = 5_005_000, Level = LogLevel.Error)]
    public static partial void PersonaBarControllerIsVisibleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_005_001, Level = LogLevel.Error)]
    public static partial void PersonaBarControllerGetMenuItemControllerException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_005_002, Level = LogLevel.Error)]
    public static partial void PersonaBarControllerUpdateParametersException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_005_003, Level = LogLevel.Error)]
    public static partial void PersonaBarControllerGetMenuSettingsException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Library.Controllers.ModulesController (5,005,100 to 5,005,199)
    [LoggerMessage(EventId = 5_005_100, Level = LogLevel.Error)]
    public static partial void ModulesControllerCopyModuleException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_005_101, Level = LogLevel.Error)]
    public static partial void ModulesControllerDeleteModuleException(this ILogger logger, Exception exception);

    // Dnn.PersonaBar.Library.Permissions.MenuPermissionController (5,010,000 to 5,010,099)
    [LoggerMessage(EventId = 5_010_000, Level = LogLevel.Error)]
    public static partial void MenuPermissionControllerGetMenuPermissionsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_010_001, Level = LogLevel.Error)]
    public static partial void MenuPermissionControllerEnsureMenuDefaultPermissionsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_010_002, Level = LogLevel.Error)]
    public static partial void MenuPermissionControllerSaveMenuDefaultPermissionsException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5_010_003, Level = LogLevel.Error, Message = "Role \"{RoleName}\" in portal \"{PortalId}\" doesn't marked as system role, will ignore add this default permission to {MenuItemIdentifier}.")]
    public static partial void MenuPermissionControllerRoleInPortalNotMarkedAsSystemRoleIgnoring(this ILogger logger, string roleName, int portalId, string menuItemIdentifier);
}
