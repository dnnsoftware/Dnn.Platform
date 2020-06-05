﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Header { get; set; }

        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Content { get; set; }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            if (this.Content != null)
            {
                this.Content.InstantiateIn(this);
            }
        }

        public override Control FindControl(string id)
        {
            this.EnsureChildControls();
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
