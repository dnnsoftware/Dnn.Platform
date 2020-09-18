// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Web.Caching;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Services.Exceptions;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : WorkflowStatePermissionController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   WorkflowStatePermissionController provides the Business Layer for DesktopModule Permissions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class WorkflowStatePermissionController
    {
        public const string WorkflowStatePermissionCacheKey = "WorkflowStatePermissions";
        public const CacheItemPriority WorkflowStatePermissionCachePriority = CacheItemPriority.Normal;

        public const int WorkflowStatePermissionCacheTimeOut = 20;
        private static readonly DataProvider provider = DataProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkflowStatePermissions gets a WorkflowStatePermissionCollection.
        /// </summary>
        /// <param name = "StateID">The ID of the State.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static WorkflowStatePermissionCollection GetWorkflowStatePermissions(int StateID)
        {
            bool bFound = false;

            // Get the WorkflowStatePermission Dictionary
            Dictionary<int, WorkflowStatePermissionCollection> dicWorkflowStatePermissions = GetWorkflowStatePermissions();

            // Get the Collection from the Dictionary
            WorkflowStatePermissionCollection WorkflowStatePermissions = null;
            bFound = dicWorkflowStatePermissions.TryGetValue(StateID, out WorkflowStatePermissions);

            if (!bFound)
            {
                // try the database
                WorkflowStatePermissions = new WorkflowStatePermissionCollection(
                    CBO.FillCollection(provider.GetWorkflowStatePermissionsByStateID(StateID), typeof(WorkflowStatePermissionInfo)),
                    StateID);
            }

            return WorkflowStatePermissions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   HasWorkflowStatePermission checks whether the current user has a specific WorkflowState Permission.
        /// </summary>
        /// <param name = "objWorkflowStatePermissions">The Permissions for the WorkflowState.</param>
        /// <param name = "permissionKey">The Permission to check.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static bool HasWorkflowStatePermission(WorkflowStatePermissionCollection objWorkflowStatePermissions, string permissionKey)
        {
            return PortalSecurity.IsInRoles(objWorkflowStatePermissions.ToString(permissionKey));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkflowStatePermissions gets a Dictionary of WorkflowStatePermissionCollections by
        ///   WorkflowState.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private static Dictionary<int, WorkflowStatePermissionCollection> GetWorkflowStatePermissions()
        {
            return CBO.GetCachedObject<Dictionary<int, WorkflowStatePermissionCollection>>(
                new CacheItemArgs(WorkflowStatePermissionCacheKey, WorkflowStatePermissionCachePriority),
                GetWorkflowStatePermissionsCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkflowStatePermissionsCallBack gets a Dictionary of WorkflowStatePermissionCollections by
        ///   WorkflowState from the the Database.
        /// </summary>
        /// <param name = "cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        /// -----------------------------------------------------------------------------
        private static object GetWorkflowStatePermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
            return FillWorkflowStatePermissionDictionary(provider.GetWorkflowStatePermissions());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   FillWorkflowStatePermissionDictionary fills a Dictionary of WorkflowStatePermissions from a
        ///   dataReader.
        /// </summary>
        /// <param name = "dr">The IDataReader.</param>
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
                        // Add WorkflowStatePermission to WorkflowStatePermission Collection already in dictionary for StateId
                        dic[obj.StateID].Add(obj);
                    }
                    else
                    {
                        // Create new WorkflowStatePermission Collection for WorkflowStatePermissionID
                        var collection = new WorkflowStatePermissionCollection();

                        // Add Permission to Collection
                        collection.Add(obj);

                        // Add Collection to Dictionary
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
    }
}
