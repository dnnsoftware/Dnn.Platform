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

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// The DNNRegionEditControl control provides a standard UI component for editing
    /// Regions.
    /// </summary>
    [ToolboxData("<{0}:DNNRegionEditControl runat=server></{0}:DNNRegionEditControl>")]
    public class DNNRegionEditControl : EditControl
    {
        private DropDownList regions;

        private TextBox region;

        private HtmlInputHidden initialValue;

        private List<ListEntryInfo> listEntries;

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        public DNNRegionEditControl()
        {
            this.Init += this.DnnRegionControl_Init;
        }

        /// <summary>Initializes a new instance of the <see cref="DNNRegionEditControl"/> class.</summary>
        /// <param name="type">A string representing the <see cref="Type"/> being edited.</param>
        public DNNRegionEditControl(string type)
        {
            this.Init += this.DnnRegionControl_Init;
            this.SystemType = type;
        }

        /// <summary>Gets or sets the parent key of the List to display.</summary>
        public string ParentKey { get; set; }

        protected string OldStringValue
        {
            get { return Convert.ToString(this.OldValue, CultureInfo.InvariantCulture); }
        }

        /// <summary>Gets the ListEntryInfo objects associated with the control.</summary>
        protected IEnumerable<ListEntryInfo> ListEntries
        {
            get
            {
                if (this.listEntries == null)
                {
                    var listController = new ListController();
                    this.listEntries = listController.GetListEntryInfoItems("Region", this.ParentKey, this.PortalId).OrderBy(s => s.SortOrder).ThenBy(s => s.Text).ToList();
                }

                return this.listEntries;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected int PortalId => PortalController.GetEffectivePortalId(PortalSettings.Current.PortalId);

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
                    if (attribute is ListAttribute)
                    {
                        var listAtt = (ListAttribute)attribute;
                        this.ParentKey = listAtt.ParentKey;
                        this.listEntries = null;
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnDataChanged(EventArgs e)
        {
            PropertyEditorEventArgs args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.StringValue;
            args.OldValue = this.OldStringValue;
            args.StringValue = this.StringValue;
            this.OnValueChanged(args);
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.Regions.ControlStyle.CopyFrom(this.ControlStyle);
            this.Regions.ID = this.ID + "_dropdown";
            this.Regions.Attributes.Add("data-editor", "DNNRegionEditControl_DropDown");
            this.Regions.Attributes.Add("aria-label", "Region");
            this.Regions.Items.Add(new ListItem() { Text = "<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", Value = string.Empty });
            this.Controls.Add(this.Regions);

            this.Region.ControlStyle.CopyFrom(this.ControlStyle);
            this.Region.ID = this.ID + "_text";
            this.Region.Attributes.Add("data-editor", "DNNRegionEditControl_Text");
            this.Controls.Add(this.Region);

            this.RegionCode.ID = this.ID + "_value";
            this.RegionCode.Attributes.Add("data-editor", "DNNRegionEditControl_Hidden");
            this.Controls.Add(this.RegionCode);
        }

        /// <inheritdoc/>
        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            this.LoadControls();

            if (this.Page != null & this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
                this.Page.RegisterRequiresPostBack(this.RegionCode);
            }
        }

        /// <inheritdoc/>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            if (this.ListEntries != null && this.ListEntries.Any())
            {
                foreach (ListEntryInfo item in this.ListEntries)
                {
                    this.Regions.Items.Add(new ListItem() { Text = item.Text, Value = item.EntryID.ToString(CultureInfo.InvariantCulture), });
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

        private void DnnRegionControl_Init(object sender, System.EventArgs e)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.js");
            ClientResourceManager.RegisterFeatureStylesheet(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.css");
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
        }

        private void LoadControls()
        {
            this.RegionCode.Value = this.StringValue;
        }
    }
}
