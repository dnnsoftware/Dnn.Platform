// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components
{
    using System;
    using System.Data;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The DataProvider is an abstract class that provides the Data Access Layer for the HtmlText module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class DataProvider
    {
        // singleton reference to the instantiated object
        private static readonly DataProvider provider;

        // constructor
        static DataProvider()
        {
            provider = new DataProvider();
        }

        // return the provider
        public static DataProvider Instance()
        {
            return provider;
        }

        public virtual IDataReader GetHtmlText(int ModuleID, int ItemID)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlText", ModuleID, ItemID);
        }

        public virtual IDataReader GetTopHtmlText(int ModuleID, bool IsPublished)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetTopHtmlText", ModuleID, IsPublished);
        }

        public virtual IDataReader GetAllHtmlText(int ModuleID)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetAllHtmlText", ModuleID);
        }

        public virtual int AddHtmlText(int ModuleID, string Content, string Summary, int StateID, bool IsPublished, int CreatedByUserID, int History)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteScalar<int>("AddHtmlText", ModuleID, Content, Summary, StateID, IsPublished, CreatedByUserID, History);
        }

        public virtual void UpdateHtmlText(int ItemID, string Content, string Summary, int StateID, bool IsPublished, int LastModifiedByUserID)
        {
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("UpdateHtmlText", ItemID, Content, Summary, StateID, IsPublished, LastModifiedByUserID);
        }

        public virtual void DeleteHtmlText(int ModuleID, int ItemID)
        {
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlText", ModuleID, ItemID);
        }

        public virtual IDataReader GetHtmlTextLog(int ItemID)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextLog", ItemID);
        }

        public virtual void AddHtmlTextLog(int ItemID, int StateID, string Comment, bool Approved, int CreatedByUserID)
        {
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextLog", ItemID, StateID, Comment, Approved, CreatedByUserID);
        }

        public virtual IDataReader GetHtmlTextUser(int UserID)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextUser", UserID);
        }

        public virtual void AddHtmlTextUser(int ItemID, int StateID, int ModuleID, int TabID, int UserID)
        {
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextUser", ItemID, StateID, ModuleID, TabID, UserID);
        }

        public virtual void DeleteHtmlTextUsers()
        {
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlTextUsers");
        }

        public virtual IDataReader GetWorkflows(int PortalID)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflows", PortalID);
        }

        public virtual IDataReader GetWorkflowStates(int WorkflowID)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflowStates", WorkflowID);
        }

        public virtual IDataReader GetWorkflowStatePermissions()
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflowStatePermissions");
        }

        public virtual IDataReader GetWorkflowStatePermissionsByStateID(int StateID)
        {
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflowStatePermissionsByStateID", StateID);
        }
    }
}
