// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Definitions
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Search.Entities;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ModuleDefinitionController provides the Business Layer for Module Definitions.</summary>
    public partial class ModuleDefinitionController
    {
        private const string Key = "ModuleDefID";
        private static readonly DataProvider DataProvider = DataProvider.Instance();
        private readonly IPermissionDefinitionService permissionDefinitionService;

        /// <summary>Initializes a new instance of the <see cref="ModuleDefinitionController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public ModuleDefinitionController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleDefinitionController"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public ModuleDefinitionController(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        }

        /// <summary>GetModuleDefinitionByID gets a Module Definition by its ID.</summary>
        /// <param name="moduleDefID">The ID of the Module Definition.</param>
        /// <returns>The <see cref="ModuleDefinitionInfo"/> or <see langword="null"/>.</returns>
        public static ModuleDefinitionInfo GetModuleDefinitionByID(int moduleDefID)
        {
            return (from kvp in GetModuleDefinitions()
                    where kvp.Value.ModuleDefID == moduleDefID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// <summary>GetModuleDefinitionByFriendlyName gets a Module Definition by its Friendly Name (and DesktopModuleID).</summary>
        /// <param name="friendlyName">The friendly name.</param>
        /// <returns>The <see cref="ModuleDefinitionInfo"/> or <see langword="null"/>.</returns>
        public static ModuleDefinitionInfo GetModuleDefinitionByFriendlyName(string friendlyName)
        {
            Requires.NotNullOrEmpty("friendlyName", friendlyName);

            return (from kvp in GetModuleDefinitions()
                    where kvp.Value.FriendlyName == friendlyName
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// <summary>GetModuleDefinitionByFriendlyName gets a Module Definition by its Friendly Name (and DesktopModuleID).</summary>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="desktopModuleID">The ID of the Desktop Module.</param>
        /// <returns>The <see cref="ModuleDefinitionInfo"/> or <see langword="null"/>.</returns>
        public static ModuleDefinitionInfo GetModuleDefinitionByFriendlyName(string friendlyName, int desktopModuleID)
        {
            Requires.NotNullOrEmpty("friendlyName", friendlyName);
            Requires.NotNegative("desktopModuleID", desktopModuleID);

            return (from kvp in GetModuleDefinitions()
                    where kvp.Value.FriendlyName == friendlyName && kvp.Value.DesktopModuleID == desktopModuleID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// <summary>GetModuleDefinitions gets a Dictionary of Module Definitions.</summary>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping module definition ID to <see cref="ModuleDefinitionInfo"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial Dictionary<int, ModuleDefinitionInfo> GetModuleDefinitions()
            => GetModuleDefinitions(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());

        /// <summary>GetModuleDefinitions gets a Dictionary of Module Definitions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping module definition ID to <see cref="ModuleDefinitionInfo"/>.</returns>
        public static Dictionary<int, ModuleDefinitionInfo> GetModuleDefinitions(IHostSettings hostSettings)
        {
            return CBO.GetCachedObject<Dictionary<int, ModuleDefinitionInfo>>(
                hostSettings,
                new CacheItemArgs(DataCache.ModuleDefinitionCacheKey, DataCache.ModuleDefinitionCachePriority),
                GetModuleDefinitionsCallBack);
        }

        /// <summary>GetModuleDefinitionsByDesktopModuleID gets a Dictionary of Module Definitions with a particular DesktopModuleID, keyed by the FriendlyName.</summary>
        /// <param name="desktopModuleID">The ID of the Desktop Module.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping module definition friendly name to <see cref="ModuleDefinitionInfo"/>.</returns>
        public static Dictionary<string, ModuleDefinitionInfo> GetModuleDefinitionsByDesktopModuleID(int desktopModuleID)
        {
            // Iterate through cached Dictionary to get all Module Definitions with the correct DesktopModuleID
            return GetModuleDefinitions().Where(kvp => kvp.Value.DesktopModuleID == desktopModuleID)
                    .ToDictionary(kvp => kvp.Value.FriendlyName, kvp => kvp.Value);
        }

        /// <summary>Get ModuleDefinition by DefinitionName.</summary>
        /// <param name="definitionName">The definition name.</param>
        /// <param name="desktopModuleID">The ID of the Desktop Module.</param>
        /// <returns>A ModuleDefinition or null if not found.</returns>
        public static ModuleDefinitionInfo GetModuleDefinitionByDefinitionName(string definitionName, int desktopModuleID)
        {
            Requires.NotNullOrEmpty("definitionName", definitionName);
            Requires.NotNegative("desktopModuleID", desktopModuleID);

            return (from kvp in GetModuleDefinitions()
                    where kvp.Value.DefinitionName == definitionName && kvp.Value.DesktopModuleID == desktopModuleID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// <summary>SaveModuleDefinition saves the Module Definition to the database.</summary>
        /// <param name="moduleDefinition">The Module Definition to save.</param>
        /// <param name="saveChildren">A flag that determines whether the child objects are also saved.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns>The module definition ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionService")]
        public static partial int SaveModuleDefinition(ModuleDefinitionInfo moduleDefinition, bool saveChildren, bool clearCache)
            => SaveModuleDefinition(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), moduleDefinition, saveChildren, clearCache);

        /// <summary>SaveModuleDefinition saves the Module Definition to the database.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="moduleDefinition">The Module Definition to save.</param>
        /// <param name="saveChildren">A flag that determines whether the child objects are also saved.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns>The module definition ID.</returns>
        public static int SaveModuleDefinition(IPermissionDefinitionService permissionDefinitionService, ModuleDefinitionInfo moduleDefinition, bool saveChildren, bool clearCache)
        {
            int moduleDefinitionId = moduleDefinition.ModuleDefID;
            if (moduleDefinitionId == Null.NullInteger)
            {
                // Add new Module Definition
                moduleDefinitionId = DataProvider.AddModuleDefinition(
                    moduleDefinition.DesktopModuleID,
                    moduleDefinition.FriendlyName,
                    moduleDefinition.DefinitionName,
                    moduleDefinition.DefaultCacheTime,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                // Upgrade Module Definition
                DataProvider.UpdateModuleDefinition(moduleDefinition.ModuleDefID, moduleDefinition.FriendlyName, moduleDefinition.DefinitionName, moduleDefinition.DefaultCacheTime, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            if (saveChildren)
            {
                foreach (KeyValuePair<string, PermissionInfo> kvp in moduleDefinition.Permissions)
                {
                    kvp.Value.ModuleDefID = moduleDefinitionId;

                    // check if permission exists
                    var permissions = permissionDefinitionService.GetDefinitionsByCodeAndKey(kvp.Value.PermissionCode, kvp.Value.PermissionKey);
                    var permission = permissions.FirstOrDefault();
                    if (permission is not null)
                    {
                        ((IPermissionDefinitionInfo)kvp.Value).PermissionId = permission.PermissionId;
                        permissionDefinitionService.UpdateDefinition(kvp.Value);
                    }
                    else
                    {
                        permissionDefinitionService.AddDefinition(kvp.Value);
                    }
                }

                foreach (KeyValuePair<string, ModuleControlInfo> kvp in moduleDefinition.ModuleControls)
                {
                    kvp.Value.ModuleDefID = moduleDefinitionId;

                    // check if definition exists
                    ModuleControlInfo moduleControl = ModuleControlController.GetModuleControlByControlKey(kvp.Value.ControlKey, kvp.Value.ModuleDefID);
                    if (moduleControl != null)
                    {
                        kvp.Value.ModuleControlID = moduleControl.ModuleControlID;
                    }

                    ModuleControlController.SaveModuleControl(kvp.Value, clearCache);
                }
            }

            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }

            return moduleDefinitionId;
        }

        /// <summary>GetModuleDefinitionByID gets a Module Definition by its ID.</summary>
        /// <param name="objModuleDefinition">The object of the Module Definition.</param>
        public void DeleteModuleDefinition(ModuleDefinitionInfo objModuleDefinition)
        {
            this.DeleteModuleDefinition(objModuleDefinition.ModuleDefID);
        }

        /// <summary>DeleteModuleDefinition deletes a Module Definition By ID.</summary>
        /// <param name="moduleDefinitionId">The ID of the Module Definition to delete.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void DeleteModuleDefinition(int moduleDefinitionId)
        {
            // Delete associated permissions
            foreach (var permission in this.permissionDefinitionService.GetDefinitionsByModuleDefId(moduleDefinitionId))
            {
                this.permissionDefinitionService.DeleteDefinition(permission);
            }

            DataProvider.DeleteModuleDefinition(moduleDefinitionId);
            DataCache.ClearHostCache(true);

            // queue remove module definition from search index
            var document = new SearchDocumentToDelete
            {
                ModuleDefId = moduleDefinitionId,
            };

            DataProvider.Instance().AddSearchDeletedItems(document);
        }

        /// <summary>GetModuleDefinitionsCallBack gets a Dictionary of Module Definitions from the Database.</summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        private static object GetModuleDefinitionsCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillDictionary(Key, DataProvider.GetModuleDefinitions(), new Dictionary<int, ModuleDefinitionInfo>());
        }
    }
}
