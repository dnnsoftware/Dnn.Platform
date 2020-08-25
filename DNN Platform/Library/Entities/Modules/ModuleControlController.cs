// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class    : ModuleControlController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleControlController provides the Business Layer for Module Controls.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ModuleControlController
    {
        private const string key = "ModuleControlID";
        private static readonly DataProvider dataProvider = DataProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleControl adds a new Module Control to the database.
        /// </summary>
        /// <param name="objModuleControl">The Module Control to save.</param>
        /// -----------------------------------------------------------------------------
        public static void AddModuleControl(ModuleControlInfo objModuleControl)
        {
            SaveModuleControl(objModuleControl, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteModuleControl deletes a Module Control in the database.
        /// </summary>
        /// <param name="moduleControlID">The ID of the Module Control to delete.</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteModuleControl(int moduleControlID)
        {
            dataProvider.DeleteModuleControl(moduleControlID);
            DataCache.ClearHostCache(true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControl gets a single Module Control from the database.
        /// </summary>
        /// <param name="moduleControlID">The ID of the Module Control to fetch.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleControlInfo GetModuleControl(int moduleControlID)
        {
            return (from kvp in GetModuleControls()
                    where kvp.Key == moduleControlID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControl gets a Dictionary of Module Controls by Module Definition.
        /// </summary>
        /// <param name="moduleDefID">The ID of the Module Definition.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, ModuleControlInfo> GetModuleControlsByModuleDefinitionID(int moduleDefID)
        {
            return GetModuleControls().Where(kvp => kvp.Value.ModuleDefID == moduleDefID)
                   .ToDictionary(kvp => kvp.Value.ControlKey, kvp => kvp.Value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControlByControlKey gets a single Module Control from the database.
        /// </summary>
        /// <param name="controlKey">The key for the control.</param>
        /// <param name="moduleDefID">The ID of the Module Definition.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID)
        {
            return (from kvp in GetModuleControls()
                    where kvp.Value.ControlKey.Equals(controlKey, StringComparison.InvariantCultureIgnoreCase)
                                && kvp.Value.ModuleDefID == moduleDefID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveModuleControl updates a Module Control in the database.
        /// </summary>
        /// <param name="moduleControl">The Module Control to save.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static int SaveModuleControl(ModuleControlInfo moduleControl, bool clearCache)
        {
            int moduleControlID = moduleControl.ModuleControlID;
            if (moduleControlID == Null.NullInteger)
            {
                // Add new Module Definition
                moduleControlID = dataProvider.AddModuleControl(
                    moduleControl.ModuleDefID,
                    moduleControl.ControlKey,
                    moduleControl.ControlTitle,
                    moduleControl.ControlSrc,
                    moduleControl.IconFile,
                    Convert.ToInt32(moduleControl.ControlType),
                    moduleControl.ViewOrder,
                    moduleControl.HelpURL,
                    moduleControl.SupportsPartialRendering,
                    moduleControl.SupportsPopUps,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                // Upgrade Module Control
                dataProvider.UpdateModuleControl(
                    moduleControl.ModuleControlID,
                    moduleControl.ModuleDefID,
                    moduleControl.ControlKey,
                    moduleControl.ControlTitle,
                    moduleControl.ControlSrc,
                    moduleControl.IconFile,
                    Convert.ToInt32(moduleControl.ControlType),
                    moduleControl.ViewOrder,
                    moduleControl.HelpURL,
                    moduleControl.SupportsPartialRendering,
                    moduleControl.SupportsPopUps,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }

            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }

            return moduleControlID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateModuleControl updates a Module Control in the database.
        /// </summary>
        /// <param name="objModuleControl">The Module Control to save.</param>
        /// -----------------------------------------------------------------------------
        public static void UpdateModuleControl(ModuleControlInfo objModuleControl)
        {
            SaveModuleControl(objModuleControl, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControls gets a Dictionary of Module Controls from
        /// the Cache.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private static Dictionary<int, ModuleControlInfo> GetModuleControls()
        {
            return CBO.GetCachedObject<Dictionary<int, ModuleControlInfo>>(
                new CacheItemArgs(
                DataCache.ModuleControlsCacheKey,
                DataCache.ModuleControlsCacheTimeOut,
                DataCache.ModuleControlsCachePriority),
                GetModuleControlsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControlsCallBack gets a Dictionary of Module Controls from
        /// the Database.
        /// </summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        /// needed for the database call.</param>
        /// -----------------------------------------------------------------------------
        private static object GetModuleControlsCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillDictionary(key, dataProvider.GetModuleControls(), new Dictionary<int, ModuleControlInfo>());
        }
    }
}
