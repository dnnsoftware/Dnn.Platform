// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

using System;
using System.Globalization;
using System.Linq;
using DNN.Integration.Test.Framework.Helpers;

namespace DNN.Integration.Test.Framework.Controllers
{
    public static class MechanicsController
    {
        public const string ActionLinkClicked = "LinkClicked";
        public const string ActionPageViewed = "PageViewed";
        public const string ActionRegister = "Register";
        public const string ActionLogin = "Login";

        public const string ActionFileAdded = "FileAdded";
        public const string ActionFileDeleted = "FileDeleted";
        public const string ActionFileUpdated = "FileUpdated";
        public const string ActionFilePropertiesChanged = "FilePropertiesChanged";
        public const string ActionFolderAdded = "FolderAdded";
        public const string ActionFolderDeleted = "FolderDeleted";
        public const string ActionFolderUpdated = "FolderUpdated";

        public const string ActionTabCreated = "TabCreated";
        public const string ActionTabSoftDeleted = "TabSoftDeleted";
        public const string ActionTabHardDeleted = "TabHardDeleted";
        public const string ActionTabUpdated = "TabUpdated";
        public const string ActionModuleCreated = "ModuleCreated";
        public const string ActionModuleSoftDeleted = "ModuleSoftDeleted";
        public const string ActionModuleHardDeleted = "ModuleHardDeleted";
        public const string ActionModuleUpdated = "ModuleUpdated";

        /// <summary>
        /// Returns the last UserScoringLog entry.
        /// </summary>
        public static int GetLastUserScoringLogId(string actionName, int userId, int portalId, DateTime createdDateTimeAfter)
        {
            const string sql = "SELECT TOP 1 USL.UserScoringLogId from {objectQualifier}Mechanics_UserScoringLog USL " +
                               "INNER JOIN {objectQualifier}vw_Mechanics_ScoringActionDefinitions SAD " +
                               "ON USL.ScoringActionDefId = SAD.ScoringActionDefId " +
                               "WHERE UserId = {0}  " +
                               "AND PortalId = {1}" + 
                               "AND SAD.ActionName = N'{2}' " + 
                               "AND USL.CreatedOnDate > '{3}' " +
                               "ORDER BY USL.UserScoringLogId desc";
            var query = string.Format(sql.Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier), userId, portalId, actionName, createdDateTimeAfter.ToString(CultureInfo.InvariantCulture));

            var results = DatabaseHelper.ExecuteQuery(query).ToArray();
            return results.SelectMany(x => x.Where(y => y.Key == "UserScoringLogId").Select(y => (int)y.Value)).FirstOrDefault();
        }
 
    }
}