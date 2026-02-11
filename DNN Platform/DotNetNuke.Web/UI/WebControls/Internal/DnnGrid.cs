// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    public class DnnGrid : GridView
    {
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalAliasService. Scheduled removal in v12.0.0.")]
        public DnnGrid()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnGrid"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnGrid(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        }

        /// <summary>Gets the item style.</summary>
        public TableItemStyle ItemStyle => this.RowStyle;

        /// <summary>Gets the alternating item style.</summary>
        public TableItemStyle AlternatingItemStyle => this.AlternatingRowStyle;

        /// <summary>Gets the edit item style.</summary>
        public TableItemStyle EditItemStyle => this.EditRowStyle;

        /// <summary>Gets the selected item style.</summary>
        public TableItemStyle SelectedItemStyle => this.SelectedRowStyle;

        /// <summary>Gets or sets the screen row number.</summary>
        public int ScreenRowNumber { get; set; }

        /// <summary>Gets or sets the row height.</summary>
        public int RowHeight { get; set; }

        /// <summary>Gets or sets the current page index.</summary>
        public int CurrentPageIndex
        {
            get => this.PageIndex;
            set => this.PageIndex = value;
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.CssClass = "dnn-grid";
            Utilities.ApplyControlSkin(this.clientResourceController, this, string.Empty, string.Empty);

            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.DnnPlugins);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.AlternatingRowStyle.CssClass = "alter-row";
            this.Style.Remove("border-collapse");
        }
    }
}
