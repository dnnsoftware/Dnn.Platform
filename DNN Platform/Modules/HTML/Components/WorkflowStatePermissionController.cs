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
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Caching;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.Html;
using DotNetNuke.Modules.Html.Components;
using DotNetNuke.Services.Exceptions;



namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : WorkflowStatePermissionController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   WorkflowStatePermissionController provides the Business Layer for DesktopModule Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class WorkflowStatePermissionController
    {
        public const string WorkflowStatePermissionCacheKey = "WorkflowStatePermissions";
        public const CacheItemPriority WorkflowStatePermissionCachePriority = CacheItemPriority.Normal;

        public const int WorkflowStatePermissionCacheTimeOut = 20;
        private static readonly DataProvider provider = DataProvider.Instance();

        #region Private Shared Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkflowStatePermissions gets a Dictionary of WorkflowStatePermissionCollections by 
        ///   WorkflowState.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private static Dictionary<int, WorkflowStatePermissionCollection> GetWorkflowStatePermissions()
        {
            return CBO.GetCachedObject<Dictionary<int, WorkflowStatePermissionCollection>>(new CacheItemArgs(WorkflowStatePermissionCacheKey, WorkflowStatePermissionCachePriority),
                                                                                           GetWorkflowStatePermissionsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkflowStatePermissionsCallBack gets a Dictionary of WorkflowStatePermissionCollections by 
        ///   WorkflowState from the the Database.
        /// </summary>
        /// <param name = "cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call</param>
        /// -----------------------------------------------------------------------------
        private static object GetWorkflowStatePermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            return FillWorkflowStatePermissionDictionary(provider.GetWorkflowStatePermissions());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   FillWorkflowStatePermissionDictionary fills a Dictionary of WorkflowStatePermissions from a
        ///   dataReader
        /// </summary>
        /// <param name = "dr">The IDataReader</param>
        /// -----------------------------------------------------------------------------
        private static Dictionary<int, WorkflowStatePermissionCollection> FillWorkflowStatePermissionDictionary(IDataReader dr)
        {
            var dic = new Dictionary<int, WorkflowStatePermissionCollection>();
            try
            {
                WorkflowStatePermissionInfo obj = null;
                while (dr.Read())
                {
                    // fill business object
                    obj = CBO.FillObject<WorkflowStatePermissionInfo>(dr, false);

                    // add WorkflowState Permission to dictionary
                    if (dic.ContainsKey(obj.StateID))
                    {
                        //Add WorkflowStatePermission to WorkflowStatePermission Collection already in dictionary for StateId
                        dic[obj.StateID].Add(obj);
                    }
                    else
                    {
                        //Create new WorkflowStatePermission Collection for WorkflowStatePermissionID
                        var collection = new WorkflowStatePermissionCollection();

                        //Add Permission to Collection
                        collection.Add(obj);

                        //Add Collection to Dictionary
                        dic.Add(obj.StateID, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                if (dr != null)
                {
                    dr.Close();
                }
            }
            return dic;
        }

        #endregion

        #region Public Shared Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkflowStatePermissions gets a WorkflowStatePermissionCollection
        /// </summary>
        /// <param name = "StateID">The ID of the State</param>
        /// -----------------------------------------------------------------------------
        public static WorkflowStatePermissionCollection GetWorkflowStatePermissions(int StateID)
        {
            bool bFound = false;

            //Get the WorkflowStatePermission Dictionary
            Dictionary<int, WorkflowStatePermissionCollection> dicWorkflowStatePermissions = GetWorkflowStatePermissions();

            //Get the Collection from the Dictionary
            WorkflowStatePermissionCollection WorkflowStatePermissions = null;
            bFound = dicWorkflowStatePermissions.TryGetValue(StateID, out WorkflowStatePermissions);

            if (!bFound)
            {
                //try the database
                WorkflowStatePermissions = new WorkflowStatePermissionCollection(CBO.FillCollection(provider.GetWorkflowStatePermissionsByStateID(StateID), typeof (WorkflowStatePermissionInfo)),
                                                                                 StateID);
            }

            return WorkflowStatePermissions;
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   HasWorkflowStatePermission checks whether the current user has a specific WorkflowState Permission
        /// </summary>
        /// <param name = "objWorkflowStatePermissions">The Permissions for the WorkflowState</param>
        /// <param name = "permissionKey">The Permission to check</param>
        /// -----------------------------------------------------------------------------
        public static bool HasWorkflowStatePermission(WorkflowStatePermissionCollection objWorkflowStatePermissions, string permissionKey)
        {
            return PortalSecurity.IsInRoles(objWorkflowStatePermissions.ToString(permissionKey));
        }

        #endregion
    }
}