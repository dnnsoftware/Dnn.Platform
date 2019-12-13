#region Usings

using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class DnnTab : WebControl
    {
        public DnnTab() : base("div")
        {
        }

        public override ControlCollection Controls
        {
            get
            {
                EnsureChildControls();
                return base.Controls;
            }
        }

        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Header { get; set; }

        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Content { get; set; }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            if (Content != null)
            {
                Content.InstantiateIn(this);
            }
        }

        public override Control FindControl(string id)
        {
            EnsureChildControls();
            return base.FindControl(id);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.RenderBeginTag(writer);
            base.RenderChildren(writer);
            base.RenderEndTag(writer);
        }
    }
}
