using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.Groups.Components;

namespace DotNetNuke.Modules.Groups {
    public partial class Loader : GroupsModuleBase {
        protected void Page_Load(object sender, EventArgs e) {
            string path = Constants.ModulePath;
            switch (LoadView) {
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
            ctl = (GroupsModuleBase)LoadControl(path);
            ctl.ModuleConfiguration = this.ModuleConfiguration;
            plhContent.Controls.Add(ctl);
        }
    }
}