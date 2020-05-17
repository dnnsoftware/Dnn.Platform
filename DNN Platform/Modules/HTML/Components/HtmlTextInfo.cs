﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Entities;

namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of an HtmlText object
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Serializable]
    public class HtmlTextInfo : BaseEntityInfo
    {
        // local property declarations
        private bool _Approved = true;
        private string _Comment = "";
        private bool _IsActive = true;
        private int _ItemID = -1;

        // initialization

        // public properties
        public int ItemID
        {
            get
            {
                return _ItemID;
            }
            set
            {
                _ItemID = value;
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
                return _IsActive;
            }
            set
            {
                _IsActive = value;
            }
        }

        public string Comment
        {
            get
            {
                return _Comment;
            }
            set
            {
                _Comment = value;
            }
        }

        public bool Approved
        {
            get
            {
                return _Approved;
            }
            set
            {
                _Approved = value;
            }
        }

        public string DisplayName { get; set; }

		public string Summary { get; set; }
    }
}
