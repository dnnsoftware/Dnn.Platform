// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>  MyWork allows a user to view any outstanding workflow items.</summary>
    public partial class MyWork : PortalModuleBase, IActionable
    {
        private readonly MvcModuleControlRenderer<MyWorkControl> renderer;

        /// <summary>Initializes a new instance of the <see cref="MyWork"/> class.</summary>
        public MyWork()
        {
            this.renderer = new MvcModuleControlRenderer<MyWorkControl>(this);
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                return this.renderer.ModuleControl.ModuleActions;
            }
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                var html = this.renderer.RenderToString();
                this.Controls.Add(new LiteralControl(html));
                this.renderer.ModuleControl.RegisterResources(this.Page);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
