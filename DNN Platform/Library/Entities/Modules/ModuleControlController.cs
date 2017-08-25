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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class	 : ModuleControlController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleControlController provides the Business Layer for Module Controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ModuleControlController
    {
        private const string key = "ModuleControlID";
        private static readonly DataProvider dataProvider = DataProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControls gets a Dictionary of Module Controls from
        /// the Cache.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private static Dictionary<int, ModuleControlInfo> GetModuleControls()
        {
            return CBO.GetCachedObject<Dictionary<int, ModuleControlInfo>>(new CacheItemArgs(DataCache.ModuleControlsCacheKey, 
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
        /// needed for the database call</param>
        /// -----------------------------------------------------------------------------
        private static object GetModuleControlsCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillDictionary(key, dataProvider.GetModuleControls(), new Dictionary<int, ModuleControlInfo>());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleControl adds a new Module Control to the database
        /// </summary>
        /// <param name="objModuleControl">The Module Control to save</param>
        /// -----------------------------------------------------------------------------
        public static void AddModuleControl(ModuleControlInfo objModuleControl)
        {
            SaveModuleControl(objModuleControl, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteModuleControl deletes a Module Control in the database
        /// </summary>
        /// <param name="moduleControlID">The ID of the Module Control to delete</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteModuleControl(int moduleControlID)
        {
            dataProvider.DeleteModuleControl(moduleControlID);
            DataCache.ClearHostCache(true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControl gets a single Module Control from the database
        /// </summary>
        /// <param name="moduleControlID">The ID of the Module Control to fetch</param>
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
        /// GetModuleControl gets a Dictionary of Module Controls by Module Definition
        /// </summary>
        /// <param name="moduleDefID">The ID of the Module Definition</param>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, ModuleControlInfo> GetModuleControlsByModuleDefinitionID(int moduleDefID)
        {
            return GetModuleControls().Where(kvp => kvp.Value.ModuleDefID == moduleDefID)
                   .ToDictionary(kvp => kvp.Value.ControlKey, kvp => kvp.Value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModuleControlByControlKey gets a single Module Control from the database
        /// </summary>
        /// <param name="controlKey">The key for the control</param>
        /// <param name="moduleDefID">The ID of the Module Definition</param>
        /// -----------------------------------------------------------------------------
        public static ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID)
        {
            return (from kvp in GetModuleControls()
                    where kvp.Value.ControlKey.ToLowerInvariant() == controlKey.ToLowerInvariant() 
                                && kvp.Value.ModuleDefID == moduleDefID
                    select kvp.Value)
                   .FirstOrDefault();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveModuleControl updates a Module Control in the database
        /// </summary>
        /// <param name="moduleControl">The Module Control to save</param>
        /// <param name="clearCache">A flag that determines whether to clear the host cache</param>
        /// -----------------------------------------------------------------------------
        public static int SaveModuleControl(ModuleControlInfo moduleControl, bool clearCache)
        {
            int moduleControlID = moduleControl.ModuleControlID;
            if (moduleControlID == Null.NullInteger)
            {
				//Add new Module Definition
                moduleControlID = dataProvider.AddModuleControl(moduleControl.ModuleDefID,
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
				//Upgrade Module Control
                dataProvider.UpdateModuleControl(moduleControl.ModuleControlID,
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
        /// UpdateModuleControl updates a Module Control in the database
        /// </summary>
        /// <param name="objModuleControl">The Module Control to save</param>
        /// -----------------------------------------------------------------------------
        public static void UpdateModuleControl(ModuleControlInfo objModuleControl)
        {
            SaveModuleControl(objModuleControl, true);
        }
    }
}
