#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnDockCommand : DockCommand
    {
        public DnnDockCommand()
        {
        }

        public DnnDockCommand(string clientTypeName, string cssClass, string name, string text, bool autoPostBack) : base(clientTypeName, cssClass, name, text, autoPostBack)
        {
        }
    }
}
