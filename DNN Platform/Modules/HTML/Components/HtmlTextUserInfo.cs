// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextUserInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of an HtmlTextUser object.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextUserInfo
    {
        // local property declarations
        private ModuleInfo _Module;

        public string ModuleTitle
        {
            get
            {
                string _ModuleTitle = Null.NullString;
                if (this.Module != null)
                {
                    _ModuleTitle = this.Module.ModuleTitle;
                }

                return _ModuleTitle;
            }
        }

        public ModuleInfo Module
        {
            get
            {
                if (this._Module == null)
                {
                    this._Module = ModuleController.Instance.GetModule(this.ModuleID, this.TabID, false);
                }

                return this._Module;
            }
        }

        public int ItemID { get; set; }

        public int StateID { get; set; }

        public string StateName { get; set; }

        public int ModuleID { get; set; }

        public int TabID { get; set; }

        public int UserID { get; set; }

        public DateTime CreatedOnDate { get; set; }
    }
}
