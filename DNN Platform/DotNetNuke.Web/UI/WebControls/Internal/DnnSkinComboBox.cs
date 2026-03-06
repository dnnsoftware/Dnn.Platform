// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    [ToolboxData("<{0}:DnnSkinComboBox runat='server'></{0}:DnnSkinComboBox>")]
    public class DnnSkinComboBox : DnnComboBox
    {
        /// <summary>Initializes a new instance of the <see cref="DnnSkinComboBox"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnSkinComboBox()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnSkinComboBox"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnSkinComboBox(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
            : base(
                appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>())
        {
            this.PortalId = Null.NullInteger;
        }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; }

        /// <summary>Gets or sets the root path.</summary>
        public string RootPath { get; set; }

        /// <summary>Gets or sets the skin scope.</summary>
        public SkinScope Scope { get; set; }

        /// <summary>Gets or sets a value indicating whether to include a "None Specified" item.</summary>
        public bool IncludeNoneSpecificItem { get; set; }

        /// <summary>Gets or sets the text of the "None Specified" item.</summary>
        public string NoneSpecificText { get; set; }

        private PortalInfo Portal => this.PortalId == Null.NullInteger ? null : PortalController.Instance.GetPortal(this.PortalId);

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.DataTextField = "Key";
            this.DataValueField = "Value";

            if (!this.Page.IsPostBack && !string.IsNullOrEmpty(this.RootPath))
            {
                this.DataSource = SkinController.GetSkins(this.Portal, this.RootPath, this.Scope)
                                           .ToDictionary(skin => skin.Key, skin => skin.Value);

                if (string.IsNullOrEmpty(this.SelectedValue))
                {
                    this.DataBind();
                }
                else
                {
                    this.DataBind(this.SelectedValue);
                }

                if (this.IncludeNoneSpecificItem)
                {
                    this.InsertItem(0, this.NoneSpecificText, string.Empty);
                }
            }

            this.AttachEvents();
        }

        /// <inheritdoc />
        protected override void PerformDataBinding(IEnumerable dataSource)
        {
            // do not select item during data binding, item will select later
            var selectedValue = this.SelectedValue;
            this.SelectedValue = null;

            base.PerformDataBinding(dataSource);

            this.SelectedValue = selectedValue;
        }

        private void AttachEvents()
        {
            if (!UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return;
            }

            this.Attributes.Add("PortalPath", this.Portal != null ? this.Portal.HomeDirectory : string.Empty);
            this.Attributes.Add("HostPath", Globals.HostPath);

            // OnClientSelectedIndexChanged = "selectedIndexChangedMethod";
            var indexChangedMethod = @"function selectedIndexChangedMethod(sender, eventArgs){
    var value = eventArgs.get_item().get_value();
    value = value.replace('[L]', sender.get_attributes().getAttribute('PortalPath'));
    value = value.replace('[G]', sender.get_attributes().getAttribute('HostPath'));
    sender.get_inputDomElement().title = value;
}";
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "OnClientSelectedIndexChanged", indexChangedMethod, true);

            // foreach (var item in Items)
            // {
            //    if (string.IsNullOrEmpty(item.Value))
            //    {
            //        continue;
            //    }

            // var tooltip = item.Value.Replace("[G]", Globals.HostPath);
            //    if (Portal != null)
            //    {
            //        tooltip = tooltip.Replace("[L]", Portal.HomeDirectory);
            //    }

            // item.ToolTip = tooltip;
            //    if (item.Value.Equals(SelectedValue))
            //    {
            //        ToolTip = tooltip;
            //    }
            // }
        }
    }
}
