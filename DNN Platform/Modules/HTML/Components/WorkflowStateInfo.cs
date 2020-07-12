// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      WorkflowStateInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of a WorkflowState object.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class WorkflowStateInfo
    {
        // local property declarations
        private bool _IsActive = true;

        // public properties
        public int PortalID { get; set; }

        public int WorkflowID { get; set; }

        public string WorkflowName { get; set; }

        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public int StateID { get; set; }

        public string StateName { get; set; }

        public int Order { get; set; }

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
    }
}
