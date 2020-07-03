// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.Groups.Components;

    public partial class Loader : GroupsModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string path = Constants.ModulePath;
            switch (this.LoadView)
            {
                case GroupMode.Setup:
                    path += "Setup.ascx";
                    break;
                case GroupMode.List:
                    path += "List.ascx";
                    break;
                case GroupMode.View:
                    path += "GroupView.ascx";
                    break;
            }

            GroupsModuleBase ctl = new GroupsModuleBase();
            ctl = (GroupsModuleBase)this.LoadControl(path);
            ctl.ModuleConfiguration = this.ModuleConfiguration;
            this.plhContent.Controls.Add(ctl);
        }
    }
}
