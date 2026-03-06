// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
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
    using DotNetNuke.Web.Client.ResourceManager;

    using Microsoft.Extensions.DependencyInjection;

    [ToolboxData("<{0}:DnnCountryAutocompleteControl runat=server></{0}:DnnCountryAutocompleteControl>")]
    public class DnnCountryAutocompleteControl : EditControl
    {
        private readonly IServicesFramework servicesFramework;
        private readonly ListController listController;
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IPortalController portalController;

        private TextBox countryName;
        private HiddenField countryId;

        /// <summary>Initializes a new instance of the <see cref="DnnCountryAutocompleteControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DnnCountryAutocompleteControl()
            : this(null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCountryAutocompleteControl"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public DnnCountryAutocompleteControl(IServicesFramework servicesFramework)
            : this(servicesFramework, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCountryAutocompleteControl"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        public DnnCountryAutocompleteControl(IServicesFramework servicesFramework, ListController listController, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IPortalController portalController)
        {
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
            this.listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.Init += this.DnnCountryRegionControl_Init;
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCountryAutocompleteControl"/> class.</summary>
        /// <param name="type">A string representing the <see cref="Type"/> being edited.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DnnCountryAutocompleteControl(string type)
            : this(type, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCountryAutocompleteControl"/> class.</summary>
        /// <param name="type">A string representing the <see cref="Type"/> being edited.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public DnnCountryAutocompleteControl(string type, IServicesFramework servicesFramework)
            : this(type, servicesFramework, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCountryAutocompleteControl"/> class.</summary>
        /// <param name="type">A string representing the <see cref="Type"/> being edited.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        public DnnCountryAutocompleteControl(string type, IServicesFramework servicesFramework, ListController listController, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IPortalController portalController)
            : this(servicesFramework, listController, clientResourceController, appStatus, eventLogger, portalController)
        {
            this.SystemType = type;
        }

        /// <inheritdoc />
        public override string EditControlClientId
        {
            get
            {
                this.EnsureChildControls();
                return this.CountryName.ClientID;
            }
        }

        protected string OldStringValue => Convert.ToString(this.OldValue, CultureInfo.InvariantCulture);

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

        private TextBox CountryName
        {
            get
            {
                if (this.countryName == null)
                {
                    this.countryName = new TextBox();
                }

                return this.countryName;
            }
        }

        private HiddenField CountryId
        {
            get
            {
                if (this.countryId == null)
                {
                    this.countryId = new HiddenField();
                }

                return this.countryId;
            }
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = postCollection[postDataKey + "_id"];
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        /// <inheritdoc />
        protected override void OnDataChanged(EventArgs e)
        {
            PropertyEditorEventArgs args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.StringValue;
            args.OldValue = this.OldStringValue;
            args.StringValue = this.StringValue;
            this.OnValueChanged(args);
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.CountryName.ControlStyle.CopyFrom(this.ControlStyle);
            this.CountryName.ID = this.ID + "_name";
            this.CountryName.Attributes.Add("data-name", this.Name);
            this.CountryName.Attributes.Add("data-list", "Country");
            this.CountryName.Attributes.Add("data-category", this.Category);
            this.CountryName.Attributes.Add("data-editor", "DnnCountryAutocompleteControl");
            this.CountryName.Attributes.Add("data-required", this.Required.ToString().ToLowerInvariant());
            this.Controls.Add(this.CountryName);

            this.CountryId.ID = this.ID + "_id";
            this.Controls.Add(this.CountryId);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.LoadControls();

            if (this.Page != null & this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
                this.Page.RegisterRequiresPostBack(this.CountryId);
            }
        }

        /// <inheritdoc />
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }

        private void DnnCountryRegionControl_Init(object sender, EventArgs e)
        {
            this.servicesFramework.RequestAjaxAntiForgerySupport();
            this.clientResourceController.RegisterScript("~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.js");
            this.clientResourceController.CreateStylesheet("~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.css").SetPriority(FileOrder.Css.FeatureCss).Register();
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.portalController.GetCurrentSettings(), CommonJs.jQuery);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.portalController.GetCurrentSettings(), CommonJs.jQueryUI);
        }

        private void LoadControls()
        {
            this.CountryName.Text = this.StringValue;
            string countryCode = CountryLookup.CodeByName(this.StringValue);
            if (!string.IsNullOrEmpty(this.StringValue) && int.TryParse(this.StringValue, out var countryId))
            {
                var c = this.listController.GetListEntryInfo(countryId);
                this.CountryName.Text = c.Text;
                countryCode = c.Value;
            }

            this.CountryId.Value = this.StringValue;

            var regionControl2 = ControlUtilities.FindFirstDescendent<DNNRegionEditControl>(this.Page, c => this.IsCoupledRegionControl(c));
            regionControl2?.ParentKey = "Country." + countryCode;
        }

        private bool IsCoupledRegionControl(Control ctr)
        {
            if (ctr is DNNRegionEditControl regionEditControl)
            {
                if (regionEditControl.Category == this.Category)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
