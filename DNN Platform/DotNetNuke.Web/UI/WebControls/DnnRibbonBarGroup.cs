// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    [ParseChildren(true)]
    public class DnnRibbonBarGroup : WebControl
    {
        private bool _CheckToolVisibility = true;
        private HtmlGenericControl _contentContainer;

        public DnnRibbonBarGroup()
            : base("div")
        {
            this.CssClass = "dnnRibbonGroup";
        }

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Footer { get; set; }

        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Content { get; set; }

        public virtual bool CheckToolVisibility
        {
            get
            {
                return this._CheckToolVisibility;
            }

            set
            {
                this._CheckToolVisibility = value;
            }
        }

        public override Control FindControl(string id)
        {
            this.EnsureChildControls();
            return base.FindControl(id);
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if (this.CheckVisibility())
            {
                this.RenderBeginTag(writer);
                this.RenderChildren(writer);
                this.RenderEndTag(writer);
            }
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            HtmlGenericControl topLeft = new HtmlGenericControl("div");
            topLeft.Attributes.Add("class", "topLeft");
            HtmlGenericControl topRight = new HtmlGenericControl("div");
            topRight.Attributes.Add("class", "topRight");

            HtmlGenericControl bottomLeft = new HtmlGenericControl("div");
            bottomLeft.Attributes.Add("class", "bottomLeft");
            HtmlGenericControl bottomRight = new HtmlGenericControl("div");
            bottomRight.Attributes.Add("class", "bottomRight");

            this._contentContainer = new HtmlGenericControl("div");
            this._contentContainer.Attributes.Add("class", "content");

            HtmlGenericControl footerContainer = new HtmlGenericControl("div");
            footerContainer.Attributes.Add("class", "footer");

            this.Controls.Add(topLeft);
            this.Controls.Add(topRight);
            this.Controls.Add(this._contentContainer);
            this.Controls.Add(footerContainer);
            this.Controls.Add(bottomLeft);
            this.Controls.Add(bottomRight);

            if (this.Content != null)
            {
                this.Content.InstantiateIn(this._contentContainer);
            }

            if (this.Footer != null)
            {
                this.Footer.InstantiateIn(footerContainer);
            }
        }

        private bool CheckVisibility()
        {
            bool returnValue = true;
            if (this.Visible && this.CheckToolVisibility)
            {
                // Hide group if all tools are invisible
                bool foundTool = false;
                ControlCollection controls = this._contentContainer.Controls;
                returnValue = this.AreChildToolsVisible(ref controls, ref foundTool);
            }

            return returnValue;
        }

        private bool AreChildToolsVisible(ref ControlCollection children, ref bool foundTool)
        {
            bool returnValue = false;

            foreach (Control ctrl in children)
            {
                if (ctrl is IDnnRibbonBarTool)
                {
                    foundTool = true;
                    if (ctrl.Visible)
                    {
                        returnValue = true;
                        break;
                    }
                }
                else
                {
                    ControlCollection controls = ctrl.Controls;
                    if (this.AreChildToolsVisible(ref controls, ref foundTool))
                    {
                        if (foundTool)
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }

            if (!foundTool)
            {
                return true;
            }

            return returnValue;
        }
    }
}
