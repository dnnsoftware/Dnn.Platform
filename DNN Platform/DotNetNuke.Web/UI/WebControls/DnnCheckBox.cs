#region Usings

using System;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnCheckBox : CheckBox
    {
        public string CommandArgument
        {
            get
            {
                return Convert.ToString(ViewState["CommandArgument"]);
            }
            set
            {
                ViewState["CommandArgument"] = value;
            }
        }
    }
}
