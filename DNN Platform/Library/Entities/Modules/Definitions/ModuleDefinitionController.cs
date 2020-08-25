// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Definitions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Search.Entities;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules.Definitions
    /// Class    : ModuleDefinitionController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleDefinitionController provides the Business Layer for Module Definitions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ModuleDefinitionController
    {
        private const string key = "ModuleDefID";
        private static readonly DataProvider dataProvider = DataProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinitionByID gets a Module Definition by its ID.
        /// </summary>
        /// <param name="moduleDefID">The ID of the Module Definition.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleDefinitionInfo GetModuleDefinitionByID(int moduleDefID)
        {
            return (from kvp in GetModuleDefinitions()
                    where kvp.Value.ModuleDefID == moduleDefID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinitionByFriendlyName gets a Module Definition by its Friendly
        /// Name (and DesktopModuleID).
        /// </summary>
        /// <param name="friendlyName">The friendly name.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleDefinitionInfo GetModuleDefinitionByFriendlyName(string friendlyName)
        {
            Requires.NotNullOrEmpty("friendlyName", friendlyName);

            return (from kvp in GetModuleDefinitions()
                    where kvp.Value.FriendlyName == friendlyName
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinitionByFriendlyName gets a Module Definition by its Friendly
        /// Name (and DesktopModuleID).
        /// </summary>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="desktopModuleID">The ID of the Dekstop Module.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleDefinitionInfo GetModuleDefinitionByFriendlyName(string friendlyName, int desktopModuleID)
        {
            Requires.NotNullOrEmpty("friendlyName", friendlyName);
            Requires.NotNegative("desktopModuleID", desktopModuleID);

            return (from kvp in GetModuleDefinitions()
                    where kvp.Value.FriendlyName == friendlyName && kvp.Value.DesktopModuleID == desktopModuleID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinitions gets a Dictionary of Module Definitions.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<int, ModuleDefinitionInfo> GetModuleDefinitions()
        {
            return CBO.GetCachedObject<Dictionary<int, ModuleDefinitionInfo>>(
                new CacheItemArgs(
                DataCache.ModuleDefinitionCacheKey,
                DataCache.ModuleDefinitionCachePriority),
                GetModuleDefinitionsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinitionsByDesktopModuleID gets a Dictionary of Module Definitions
        /// with a particular DesktopModuleID, keyed by the FriendlyName.
        /// </summary>
        /// <param name="desktopModuleID">The ID of the Desktop Module.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, ModuleDefinitionInfo> GetModuleDefinitionsByDesktopModuleID(int desktopModuleID)
        {
            // Iterate through cached Dictionary to get all Module Definitions with the correct DesktopModuleID
            return GetModuleDefinitions().Where(kvp => kvp.Value.DesktopModuleID == desktopModuleID)
                    .ToDictionary(kvp => kvp.Value.FriendlyName, kvp => kvp.Value);
        }

        /// <summary>
        /// Get ModuleDefinition by DefinitionName.
        /// </summary>
        /// <param name="definitionName">The defintion name.</param>
        /// <param name="desktopModuleID">The ID of the Dekstop Module.</param>
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveModuleDefinition saves the Module Definition to the database.
        /// </summary>
        /// <param name="moduleDefinition">The Module Definition to save.</param>
        /// <param name="saveChildren">A flag that determines whether the child objects are also saved.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static int SaveModuleDefinition(ModuleDefinitionInfo moduleDefinition, bool saveChildren, bool clearCache)
        {
            int moduleDefinitionID = moduleDefinition.ModuleDefID;
            if (moduleDefinitionID == Null.NullInteger)
            {
                // Add new Module Definition
                moduleDefinitionID = dataProvider.AddModuleDefinition(
                    moduleDefinition.DesktopModuleID,
                    moduleDefinition.FriendlyName,
                    moduleDefinition.DefinitionName,
                    moduleDefinition.DefaultCacheTime,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                // Upgrade Module Definition
                dataProvider.UpdateModuleDefinition(moduleDefinition.ModuleDefID, moduleDefinition.FriendlyName, moduleDefinition.DefinitionName, moduleDefinition.DefaultCacheTime, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            if (saveChildren)
            {
                foreach (KeyValuePair<string, PermissionInfo> kvp in moduleDefinition.Permissions)
                {
                    kvp.Value.ModuleDefID = moduleDefinitionID;

                    // check if permission exists
                    var permissionController = new PermissionController();
                    ArrayList permissions = permissionController.GetPermissionByCodeAndKey(kvp.Value.PermissionCode, kvp.Value.PermissionKey);
                    if (permissions != null && permissions.Count == 1)
                    {
                        var permission = (PermissionInfo)permissions[0];
                        kvp.Value.PermissionID = permission.PermissionID;
                        permissionController.UpdatePermission(kvp.Value);
                    }
                    else
                    {
                        permissionController.AddPermission(kvp.Value);
                    }
                }

                foreach (KeyValuePair<string, ModuleControlInfo> kvp in moduleDefinition.ModuleControls)
                {
                    kvp.Value.ModuleDefID = moduleDefinitionID;

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

            return moduleDefinitionID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinitionByID gets a Module Definition by its ID.
        /// </summary>
        /// <param name="objModuleDefinition">The object of the Module Definition.</param>
        /// -----------------------------------------------------------------------------
        public void DeleteModuleDefinition(ModuleDefinitionInfo objModuleDefinition)
        {
            this.DeleteModuleDefinition(objModuleDefinition.ModuleDefID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteModuleDefinition deletes a Module Definition By ID.
        /// </summary>
        /// <param name="moduleDefinitionId">The ID of the Module Definition to delete.</param>
        /// -----------------------------------------------------------------------------
        public void DeleteModuleDefinition(int moduleDefinitionId)
        {
            // Delete associated permissions
            var permissionController = new PermissionController();
            foreach (PermissionInfo permission in permissionController.GetPermissionsByModuleDefID(moduleDefinitionId))
            {
                permissionController.DeletePermission(permission.PermissionID);
            }

            dataProvider.DeleteModuleDefinition(moduleDefinitionId);
            DataCache.ClearHostCache(true);

            // queue remove module definition from search index
            var document = new SearchDocumentToDelete
            {
                ModuleDefId = moduleDefinitionId,
            };

            DataProvider.Instance().AddSearchDeletedItems(document);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleDefinitionsCallBack gets a Dictionary of Module Definitions from
        /// the Database.
        /// </summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        /// needed for the database call.</param>
        /// -----------------------------------------------------------------------------
        private static object GetModuleDefinitionsCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillDictionary(key, dataProvider.GetModuleDefinitions(), new Dictionary<int, ModuleDefinitionInfo>());
        }
    }
}
