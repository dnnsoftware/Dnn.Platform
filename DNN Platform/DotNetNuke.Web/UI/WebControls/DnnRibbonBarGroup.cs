#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class DnnRibbonBarGroup : WebControl
    {
        private bool _CheckToolVisibility = true;
        private HtmlGenericControl _contentContainer;

        public DnnRibbonBarGroup() : base("div")
        {
            CssClass = "dnnRibbonGroup";
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
        public virtual ITemplate Footer { get; set; }

        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Content { get; set; }

        public virtual bool CheckToolVisibility
        {
            get
            {
                return _CheckToolVisibility;
            }
            set
            {
                _CheckToolVisibility = value;
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            HtmlGenericControl topLeft = new HtmlGenericControl("div");
            topLeft.Attributes.Add("class", "topLeft");
            HtmlGenericControl topRight = new HtmlGenericControl("div");
            topRight.Attributes.Add("class", "topRight");

            HtmlGenericControl bottomLeft = new HtmlGenericControl("div");
            bottomLeft.Attributes.Add("class", "bottomLeft");
            HtmlGenericControl bottomRight = new HtmlGenericControl("div");
            bottomRight.Attributes.Add("class", "bottomRight");

            _contentContainer = new HtmlGenericControl("div");
            _contentContainer.Attributes.Add("class", "content");

            HtmlGenericControl footerContainer = new HtmlGenericControl("div");
            footerContainer.Attributes.Add("class", "footer");

            Controls.Add(topLeft);
            Controls.Add(topRight);
            Controls.Add(_contentContainer);
            Controls.Add(footerContainer);
            Controls.Add(bottomLeft);
            Controls.Add(bottomRight);

            if (Content != null)
            {
                Content.InstantiateIn(_contentContainer);
            }

            if (Footer != null)
            {
                Footer.InstantiateIn(footerContainer);
            }
        }

        private bool CheckVisibility()
        {
            bool returnValue = true;
            if ((Visible && CheckToolVisibility))
            {
                //Hide group if all tools are invisible
                bool foundTool = false;
                ControlCollection controls = _contentContainer.Controls;
                returnValue = AreChildToolsVisible(ref controls, ref foundTool);
            }
            return returnValue;
        }

        private bool AreChildToolsVisible(ref ControlCollection children, ref bool foundTool)
        {
            bool returnValue = false;

            foreach (Control ctrl in children)
            {
                if ((ctrl is IDnnRibbonBarTool))
                {
                    foundTool = true;
                    if ((ctrl.Visible))
                    {
                        returnValue = true;
                        break;
                    }
                }
                else
                {
                    ControlCollection controls = ctrl.Controls;
                    if ((AreChildToolsVisible(ref controls, ref foundTool)))
                    {
                        if ((foundTool))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }

            if ((!foundTool))
            {
                return true;
            }

            return returnValue;
        }

        public override Control FindControl(string id)
        {
            EnsureChildControls();
            return base.FindControl(id);
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if ((CheckVisibility()))
            {
                base.RenderBeginTag(writer);
                base.RenderChildren(writer);
                base.RenderEndTag(writer);
            }
        }
    }
}