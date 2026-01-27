// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A ribbon bar control.</summary>
    [ParseChildren(true)]
    public class DnnRibbonBar : WebControl
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnRibbonBar"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnRibbonBar()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnRibbonBar"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnRibbonBar(IClientResourceController clientResourceController)
            : base("div")
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();

            this.CssClass = "dnnRibbon";
            Control control = this;
            Utilities.ApplyControlSkin(this.clientResourceController, control, "RibbonBar", "RibbonBar");
        }

        /// <summary>Gets the groups.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DnnRibbonBarGroupCollection Groups => (DnnRibbonBarGroupCollection)this.Controls;

        /// <inheritdoc />
        protected override void AddParsedSubObject(object obj)
        {
            if (obj is DnnRibbonBarGroup)
            {
                base.AddParsedSubObject(obj);
            }
            else
            {
                throw new NotSupportedException("DnnRibbonBarGroupCollection must contain controls of type DnnRibbonBarGroup");
            }
        }

        /// <inheritdoc />
        protected override ControlCollection CreateControlCollection()
        {
            return new DnnRibbonBarGroupCollection(this);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Visible)
            {
                Utilities.ApplyControlSkin(this.clientResourceController, this, "RibbonBar", "RibbonBar");
            }
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.Groups.Count > 0)
            {
                var firstGroup = this.Groups[0];
                firstGroup.CssClass = $"{firstGroup.CssClass} {firstGroup.CssClass.Trim()}First";
                var lastGroup = this.Groups[this.Groups.Count - 1];
                lastGroup.CssClass = $"{lastGroup.CssClass} {lastGroup.CssClass.Trim()}Last";
            }

            this.RenderBeginTag(writer);

            writer.AddAttribute("class", "barContent");
            writer.RenderBeginTag("div");

            writer.AddAttribute("cellpadding", "0");
            writer.AddAttribute("cellspacing", "0");
            writer.AddAttribute("border", "0");
            writer.RenderBeginTag("table");
            writer.RenderBeginTag("tr");

            foreach (DnnRibbonBarGroup grp in this.Groups)
            {
                if (grp.Visible)
                {
                    writer.RenderBeginTag("td");
                    grp.RenderControl(writer);
                    writer.RenderEndTag();
                }
            }

            // MyBase.RenderChildren(writer)
            writer.RenderEndTag();

            // tr
            writer.RenderEndTag();

            // table
            writer.RenderEndTag();

            // div
            writer.AddAttribute("class", "barBottomLeft");
            writer.RenderBeginTag("div");
            writer.RenderEndTag();

            writer.AddAttribute("class", "barBottomRight");
            writer.RenderBeginTag("div");
            writer.RenderEndTag();

            this.RenderEndTag(writer);
        }
    }
}
