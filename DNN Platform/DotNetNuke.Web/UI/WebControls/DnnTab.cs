// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System.Web.UI;
using System.Web.UI.WebControls;

[ParseChildren(true)]
public class DnnTab : WebControl
{
    /// <summary>Initializes a new instance of the <see cref="DnnTab"/> class.</summary>
    public DnnTab()
        : base("div")
    {
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override Control FindControl(string id)
    {
        this.EnsureChildControls();
        return base.FindControl(id);
    }

    /// <inheritdoc/>
    protected override void CreateChildControls()
    {
        this.Controls.Clear();

        if (this.Content != null)
        {
            this.Content.InstantiateIn(this);
        }
    }

    /// <inheritdoc/>
    protected override void Render(HtmlTextWriter writer)
    {
        this.RenderBeginTag(writer);
        this.RenderChildren(writer);
        this.RenderEndTag(writer);
    }
}
