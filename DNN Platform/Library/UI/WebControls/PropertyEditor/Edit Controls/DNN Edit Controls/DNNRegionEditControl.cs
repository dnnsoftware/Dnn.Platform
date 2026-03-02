// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ResourceManager;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The DNNRegionEditControl control provides a standard UI component for editing Regions.</summary>
    [ToolboxData("<{0}:DNNRegionEditControl runat=server></{0}:DNNRegionEditControl>")]
    public class DNNRegionEditControl : EditControl
    {
        private readonly IClientResourceController clientResourceController;
        private readonly IServicesFramework servicesFramework;
        private readonly ListController listController;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;
        private readonly IEventLogger eventLogger;
        private DropDownList regions;
        private TextBox region;
        private HtmlInputHidden initialValue;
        private List<ListEntryInfo> listEntries;

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DNNRegionEditControl()
            : this(null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public DNNRegionEditControl(IServicesFramework servicesFramework)
            : this(servicesFramework, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DNNRegionEditControl(IServicesFramework servicesFramework, ListController listController, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IEventLogger eventLogger, IClientResourceController clientResourceController)
        {
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
            this.listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.Init += this.DnnRegionControl_Init;
        }

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        /// <param name="type">A string representing the <see cref="Type"/> being edited.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DNNRegionEditControl(string type)
            : this(type, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        /// <param name="type">A string representing the <see cref="Type"/> being edited.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public DNNRegionEditControl(string type, IServicesFramework servicesFramework)
            : this(type, servicesFramework, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        /// <param name="type">A string representing the <see cref="Type"/> being edited.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DNNRegionEditControl(string type, IServicesFramework servicesFramework, ListController listController, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IEventLogger eventLogger, IClientResourceController clientResourceController)
            : this(servicesFramework, listController, portalController, appStatus, portalGroupController, eventLogger, clientResourceController)
        {
            this.SystemType = type;
        }

        /// <summary>Gets or sets the parent key of the List to display.</summary>
        public string ParentKey { get; set; }

        protected string OldStringValue => Convert.ToString(this.OldValue, CultureInfo.InvariantCulture);

        /// <summary>Gets the ListEntryInfo objects associated with the control.</summary>
        protected IEnumerable<ListEntryInfo> ListEntries
        {
            get
            {
                if (this.listEntries == null)
                {
                    this.listEntries = this.listController.GetListEntryInfoItems("Region", this.ParentKey, this.PortalId).OrderBy(s => s.SortOrder).ThenBy(s => s.Text).ToList();
                }

                return this.listEntries;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected int PortalId => PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, PortalSettings.Current.PortalId);

        /// <inheritdoc />
        protected override string StringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (this.Value != null)
                {
                    strValue = Convert.ToString(this.Value, CultureInfo.InvariantCulture);
                }

                return strValue;
            }

            set
            {
                this.Value = value;
            }
        }

        private DropDownList Regions
        {
            get
            {
                if (this.regions == null)
                {
                    this.regions = new DropDownList();
                }

                return this.regions;
            }
        }

        private TextBox Region
        {
            get
            {
                if (this.region == null)
                {
                    this.region = new TextBox();
                }

                return this.region;
            }
        }

        private HtmlInputHidden RegionCode
        {
            get
            {
                if (this.initialValue == null)
                {
                    this.initialValue = new HtmlInputHidden();
                }

                return this.initialValue;
            }
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = postCollection[postDataKey + "_value"];
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        /// <summary>OnAttributesChanged runs when the CustomAttributes property has changed.</summary>
        protected override void OnAttributesChanged()
        {
            // Get the List settings out of the "Attributes"
            if (this.CustomAttributes != null)
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    if (attribute is ListAttribute listAtt)
                    {
                        this.ParentKey = listAtt.ParentKey;
                        this.listEntries = null;
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void OnDataChanged(EventArgs e)
        {
            PropertyEditorEventArgs args = new PropertyEditorEventArgs(this.Name)
            {
                Value = this.StringValue,
                OldValue = this.OldStringValue,
                StringValue = this.StringValue,
            };
            this.OnValueChanged(args);
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.Regions.ControlStyle.CopyFrom(this.ControlStyle);
            this.Regions.ID = $"{this.ID}_dropdown";
            this.Regions.Attributes.Add("data-editor", "DNNRegionEditControl_DropDown");
            this.Regions.Attributes.Add("aria-label", "Region");
            this.Regions.Items.Add(new ListItem { Text = $"<{Localization.GetString("Not_Specified", Localization.SharedResourceFile)}>", Value = string.Empty, });
            this.Controls.Add(this.Regions);

            this.Region.ControlStyle.CopyFrom(this.ControlStyle);
            this.Region.ID = $"{this.ID}_text";
            this.Region.Attributes.Add("data-editor", "DNNRegionEditControl_Text");
            this.Controls.Add(this.Region);

            this.RegionCode.ID = $"{this.ID}_value";
            this.RegionCode.Attributes.Add("data-editor", "DNNRegionEditControl_Hidden");
            this.Controls.Add(this.RegionCode);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.LoadControls();

            if (this.Page != null & this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
                this.Page.RegisterRequiresPostBack(this.RegionCode);
            }
        }

        /// <inheritdoc />
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            if (this.ListEntries != null && this.ListEntries.Any())
            {
                foreach (ListEntryInfo item in this.ListEntries)
                {
                    this.Regions.Items.Add(new ListItem { Text = item.Text, Value = item.EntryID.ToString(CultureInfo.InvariantCulture), });
                }
            }

            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute("data-name", this.Name);
            writer.AddAttribute("data-list", "Region");
            writer.AddAttribute("data-category", this.Category);
            writer.AddAttribute("data-required", this.Required.ToString().ToLowerInvariant());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            this.RenderChildren(writer);
            writer.RenderEndTag();
        }

        private void DnnRegionControl_Init(object sender, EventArgs e)
        {
            this.servicesFramework.RequestAjaxAntiForgerySupport();
            this.clientResourceController.RegisterScript("~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.js");
            this.clientResourceController
                .CreateStylesheet("~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.css")
                .SetPriority(FileOrder.Css.FeatureCss)
                .Register();
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.portalController.GetCurrentSettings(), CommonJs.jQuery);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.portalController.GetCurrentSettings(), CommonJs.jQueryUI);
        }

        private void LoadControls()
        {
            this.RegionCode.Value = this.StringValue;
        }
    }
}
