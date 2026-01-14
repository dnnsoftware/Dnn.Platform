// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>Defines an instance of an HtmlTextUser object.</summary>
    public class HtmlTextUserInfo
    {
        // local property declarations
        private ModuleInfo module;

        public string ModuleTitle
        {
            get
            {
                string moduleTitle = Null.NullString;
                if (this.Module != null)
                {
                    moduleTitle = this.Module.ModuleTitle;
                }

                return moduleTitle;
            }
        }

        public ModuleInfo Module
        {
            get
            {
                if (this.module == null)
                {
                    this.module = ModuleController.Instance.GetModule(this.ModuleID, this.TabID, false);
                }

                return this.module;
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
