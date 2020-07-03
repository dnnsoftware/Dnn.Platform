// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System.ComponentModel;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.ExtensionPoints.Filters;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ToolBarButtonExtensionControl runat=server></{0}:ToolBarButtonExtensionControl>")]
    public class ToolBarButtonExtensionControl : DefaultExtensionControl
    {
        private IExtensionControlRenderer btnRenderer;

        [Bindable(true)]
        [DefaultValue(false)]
        public bool IsHost { get; set; }

        protected override void RenderContents(HtmlTextWriter output)
        {
            var extensionPointManager = new ExtensionPointManager();

            var str = new StringBuilder();

            var filter = new CompositeFilter()
                .And(new FilterByHostMenu(this.IsHost))
                .And(new FilterByUnauthenticated(HttpContext.Current.Request.IsAuthenticated));

            foreach (var extension in extensionPointManager.GetToolBarButtonExtensionPoints(this.Module, this.Group, filter))
            {
                if (extension is IToolBarMenuButtonExtensionPoint)
                {
                    this.btnRenderer = new ToolBarMenuButtonRenderer();
                    str.AppendFormat(this.btnRenderer.GetOutput(extension));
                }
                else
                {
                    extension.ModuleContext = this.ModuleContext;
                    this.btnRenderer = new ToolBarButtonRenderer();
                    str.AppendFormat(this.btnRenderer.GetOutput(extension));
                }
            }

            output.Write(str.ToString());
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }
    }
}
