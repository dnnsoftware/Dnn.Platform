#region Usings

using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.ComponentModel;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Modules.HTMLEditorProvider
{
    public abstract class HtmlEditorProvider : UserControlBase
    {
        public abstract Control HtmlEditorControl { get; }
        public abstract ArrayList AdditionalToolbars { get; set; }
        public abstract string ControlID { get; set; }
        public abstract string RootImageDirectory { get; set; }
        public abstract string Text { get; set; }
        public abstract Unit Width { get; set; }
        public abstract Unit Height { get; set; }

        //return the provider
		public static HtmlEditorProvider Instance()
        {
            return ComponentFactory.GetComponent<HtmlEditorProvider>();
        }

        public abstract void AddToolbar();

        public abstract void Initialize();
    }
}
