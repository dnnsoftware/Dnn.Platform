#region Usings

using System;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTimePicker : RadTimePicker
    {
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			base.EnableEmbeddedBaseStylesheet = true;
			Utilities.ApplySkin(this, string.Empty, "DatePicker");
		}
    }
}
