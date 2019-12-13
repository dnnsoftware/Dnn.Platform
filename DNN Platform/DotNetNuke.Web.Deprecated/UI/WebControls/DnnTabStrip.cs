#region Usings

using System;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTabStrip : RadTabStrip
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Utilities.ApplySkin(this);
        }

    }
}
