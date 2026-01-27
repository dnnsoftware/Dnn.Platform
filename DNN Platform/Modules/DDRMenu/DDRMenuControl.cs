// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.Localisation;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>DDR Menu WebControl.</summary>
    internal class DDRMenuControl : WebControl, IPostBackEventHandler
    {
        private readonly ILocaliser localiser;
        private readonly IHostSettings hostSettings;
        private readonly ITabController tabController;
        private MenuBase menu;

        /// <summary>Initializes a new instance of the <see cref="DDRMenuControl"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DDRMenuControl(ILocaliser localiser)
            : this(localiser, null, null)
        {
            this.localiser = localiser;
        }

        /// <summary>Initializes a new instance of the <see cref="DDRMenuControl"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="tabController">The tab controller.</param>
        public DDRMenuControl(ILocaliser localiser, IHostSettings hostSettings, ITabController tabController)
        {
            this.localiser = localiser ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        /// <summary>Handles a click on the menu.</summary>
        /// <param name="id">The id of the menu item being clicked.</param>
        public delegate void MenuClickEventHandler(string id);

        /// <summary>Handles a click on a node of the menu.</summary>
        public event MenuClickEventHandler NodeClick;

        /// <inheritdoc />
        public override bool EnableViewState
        {
            get { return false; }
            set { }
        }

        /// <summary>Gets or sets the menu root node.</summary>
        internal MenuNode RootNode { get; set; }

        /// <summary>Gets or sets a value indicating whether to skip the localization.</summary>
        internal bool SkipLocalisation { get; set; }

        /// <summary>Gets or sets the menu settings.</summary>
        internal Settings MenuSettings { get; set; }

        /// <inheritdoc />
        public void RaisePostBackEvent(string eventArgument)
        {
            using (new DNNContext(this))
            {
                if (this.NodeClick != null)
                {
                    this.NodeClick(eventArgument);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            using (new DNNContext(this))
            {
                base.OnPreRender(e);

                this.MenuSettings.MenuStyle ??= "DNNMenu";
                this.menu = MenuBase.Instantiate(this.localiser, this.hostSettings, this.tabController, this.MenuSettings.MenuStyle);
                this.menu.RootNode = this.RootNode ?? new MenuNode();
                this.menu.SkipLocalisation = this.SkipLocalisation;
                this.menu.ApplySettings(this.MenuSettings);

                this.menu.PreRender();
            }
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter htmlWriter)
        {
            using (new DNNContext(this))
            {
                this.menu.Render(htmlWriter);
            }
        }
    }
}
