// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System;

    using DotNetNuke.Entities;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of an HtmlText object.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Serializable]
    public class HtmlTextInfo : BaseEntityInfo
    {
        // local property declarations
        private bool _Approved = true;
        private string _Comment = string.Empty;
        private bool _IsActive = true;
        private int _ItemID = -1;

        // initialization

        // public properties
        public int ItemID
        {
            get
            {
                return this._ItemID;
            }

            set
            {
                this._ItemID = value;
            }
        }

        public int ModuleID { get; set; }

        public string Content { get; set; }

        public int Version { get; set; }

        public int WorkflowID { get; set; }

        public string WorkflowName { get; set; }

        public int StateID { get; set; }

        public string StateName { get; set; }

        public bool IsPublished { get; set; }

        public int PortalID { get; set; }

        public bool Notify { get; set; }

        public bool IsActive
        {
            get
            {
                return this._IsActive;
            }

            set
            {
                this._IsActive = value;
            }
        }

        public string Comment
        {
            get
            {
                return this._Comment;
            }

            set
            {
                this._Comment = value;
            }
        }

        public bool Approved
        {
            get
            {
                return this._Approved;
            }

            set
            {
                this._Approved = value;
            }
        }

        public string DisplayName { get; set; }

        public string Summary { get; set; }
    }
}
