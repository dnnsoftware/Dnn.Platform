// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ModuleControlController provides the Business Layer for Module Controls.</summary>
    public partial class ModuleControlController
    {
        private const string Key = "ModuleControlID";
        private static readonly DataProvider DataProvider = DataProvider.Instance();

        /// <summary>AddModuleControl adds a new Module Control to the database.</summary>
        /// <param name="objModuleControl">The Module Control to save.</param>
        public static void AddModuleControl(ModuleControlInfo objModuleControl)
        {
            SaveModuleControl(objModuleControl, true);
        }

        /// <summary>DeleteModuleControl deletes a Module Control in the database.</summary>
        /// <param name="moduleControlID">The ID of the Module Control to delete.</param>
        public static void DeleteModuleControl(int moduleControlID)
        {
            DataProvider.DeleteModuleControl(moduleControlID);
            DataCache.ClearHostCache(true);
        }

        /// <summary>GetModuleControl gets a single Module Control from the database.</summary>
        /// <param name="moduleControlID">The ID of the Module Control to fetch.</param>
        /// <returns>The <see cref="ModuleControlInfo"/> or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial ModuleControlInfo GetModuleControl(int moduleControlID)
            => GetModuleControl(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), moduleControlID);

        /// <summary>GetModuleControl gets a single Module Control from the database.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="moduleControlId">The ID of the Module Control to fetch.</param>
        /// <returns>The <see cref="ModuleControlInfo"/> or <see langword="null"/>.</returns>
        public static ModuleControlInfo GetModuleControl(IHostSettings hostSettings, int moduleControlId)
        {
            return (from kvp in GetModuleControls(hostSettings)
                    where kvp.Key == moduleControlId
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// <summary>Gets a Dictionary of Module Controls by Module Definition.</summary>
        /// <param name="moduleDefID">The ID of the Module Definition.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping control key to <see cref="ModuleControlInfo"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial Dictionary<string, ModuleControlInfo> GetModuleControlsByModuleDefinitionID(int moduleDefID)
            => GetModuleControlsByModuleDefinitionID(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), moduleDefID);

        /// <summary>Gets a Dictionary of Module Controls by Module Definition.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="moduleDefId">The ID of the Module Definition.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping control key to <see cref="ModuleControlInfo"/>.</returns>
        public static Dictionary<string, ModuleControlInfo> GetModuleControlsByModuleDefinitionID(IHostSettings hostSettings, int moduleDefId)
        {
            return GetModuleControls(hostSettings).Where(kvp => kvp.Value.ModuleDefID == moduleDefId)
                   .ToDictionary(kvp => kvp.Value.ControlKey, kvp => kvp.Value);
        }

        /// <summary>GetModuleControlByControlKey gets a single Module Control from the database.</summary>
        /// <param name="controlKey">The key for the control.</param>
        /// <param name="moduleDefID">The ID of the Module Definition.</param>
        /// <returns>The <see cref="ModuleControlInfo"/> or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID)
            => GetModuleControlByControlKey(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), controlKey, moduleDefID);

        /// <summary>GetModuleControlByControlKey gets a single Module Control from the database.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="controlKey">The key for the control.</param>
        /// <param name="moduleDefId">The ID of the Module Definition.</param>
        /// <returns>The <see cref="ModuleControlInfo"/> or <see langword="null"/>.</returns>
        public static ModuleControlInfo GetModuleControlByControlKey(IHostSettings hostSettings, string controlKey, int moduleDefId)
        {
            return (from kvp in GetModuleControls(hostSettings)
                    where kvp.Value.ControlKey.Equals(controlKey, StringComparison.OrdinalIgnoreCase)
                                && kvp.Value.ModuleDefID == moduleDefId
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// <summary>SaveModuleControl updates a Module Control in the database.</summary>
        /// <param name="moduleControl">The Module Control to save.</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache.</param>
        /// <returns>The module control ID.</returns>
        public static int SaveModuleControl(ModuleControlInfo moduleControl, bool clearCache)
        {
            int moduleControlID = moduleControl.ModuleControlID;
            if (moduleControlID == Null.NullInteger)
            {
                // Add new Module Definition
                moduleControlID = DataProvider.AddModuleControl(
                    moduleControl.ModuleDefID,
                    moduleControl.ControlKey,
                    moduleControl.ControlTitle,
                    moduleControl.ControlSrc,
                    moduleControl.IconFile,
                    (int)moduleControl.ControlType,
                    moduleControl.ViewOrder,
                    moduleControl.HelpURL,
                    moduleControl.SupportsPartialRendering,
                    moduleControl.SupportsPopUps,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                // Upgrade Module Control
                DataProvider.UpdateModuleControl(
                    moduleControl.ModuleControlID,
                    moduleControl.ModuleDefID,
                    moduleControl.ControlKey,
                    moduleControl.ControlTitle,
                    moduleControl.ControlSrc,
                    moduleControl.IconFile,
                    (int)moduleControl.ControlType,
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

        /// <summary>UpdateModuleControl updates a Module Control in the database.</summary>
        /// <param name="objModuleControl">The Module Control to save.</param>
        public static void UpdateModuleControl(ModuleControlInfo objModuleControl)
        {
            SaveModuleControl(objModuleControl, true);
        }

        /// <summary>GetModuleControls gets a Dictionary of Module Controls from the Cache.</summary>
        private static Dictionary<int, ModuleControlInfo> GetModuleControls(IHostSettings hostSettings)
        {
            return CBO.GetCachedObject<Dictionary<int, ModuleControlInfo>>(
                hostSettings,
                new CacheItemArgs(DataCache.ModuleControlsCacheKey, DataCache.ModuleControlsCacheTimeOut, DataCache.ModuleControlsCachePriority),
                GetModuleControlsCallBack);
        }

        /// <summary>GetModuleControlsCallBack gets a Dictionary of Module Controls from the Database.</summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        private static object GetModuleControlsCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillDictionary(Key, DataProvider.GetModuleControls(), new Dictionary<int, ModuleControlInfo>());
        }
    }
}
