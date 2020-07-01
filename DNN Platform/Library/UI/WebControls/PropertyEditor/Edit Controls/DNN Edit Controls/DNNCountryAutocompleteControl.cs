// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    [ToolboxData("<{0}:DnnCountryAutocompleteControl runat=server></{0}:DnnCountryAutocompleteControl>")]
    public class DnnCountryAutocompleteControl : EditControl
    {
        private TextBox _CountryName;

        private HiddenField _CountryId;

        public DnnCountryAutocompleteControl()
        {
            this.Init += this.DnnCountryRegionControl_Init;
        }

        public DnnCountryAutocompleteControl(string type)
        {
            this.Init += this.DnnCountryRegionControl_Init;
            this.SystemType = type;
        }

        public override string EditControlClientId
        {
            get
            {
                this.EnsureChildControls();
                return this.CountryName.ClientID;
            }
        }

        protected string OldStringValue
        {
            get { return Convert.ToString(this.OldValue); }
        }

        protected override string StringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (this.Value != null)
                {
                    strValue = Convert.ToString(this.Value);
                }

                return strValue;
            }

            set { this.Value = value; }
        }

        private TextBox CountryName
        {
            get
            {
                if (this._CountryName == null)
                {
                    this._CountryName = new TextBox();
                }

                return this._CountryName;
            }
        }

        private HiddenField CountryId
        {
            get
            {
                if (this._CountryId == null)
                {
                    this._CountryId = new HiddenField();
                }

                return this._CountryId;
            }
        }

        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = postCollection[postDataKey + "_id"];
            if (!presentValue.Equals(postedValue))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        protected override void OnDataChanged(EventArgs e)
        {
            PropertyEditorEventArgs args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.StringValue;
            args.OldValue = this.OldStringValue;
            args.StringValue = this.StringValue;
            this.OnValueChanged(args);
        }

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

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            this.LoadControls();

            if (this.Page != null & this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
                this.Page.RegisterRequiresPostBack(this.CountryId);
            }
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }

        private void DnnCountryRegionControl_Init(object sender, System.EventArgs e)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.js");
            ClientResourceManager.RegisterFeatureStylesheet(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.css");
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
        }

        private void LoadControls()
        {
            this.CountryName.Text = this.StringValue;
            int countryId = -1;
            string countryCode = this.StringValue;
            if (!string.IsNullOrEmpty(this.StringValue) && int.TryParse(this.StringValue, out countryId))
            {
                var listController = new ListController();
                var c = listController.GetListEntryInfo(countryId);
                this.CountryName.Text = c.Text;
                countryCode = c.Value;
            }

            this.CountryId.Value = this.StringValue;

            var regionControl2 = ControlUtilities.FindFirstDescendent<DNNRegionEditControl>(this.Page, c => this.IsCoupledRegionControl(c));
            if (regionControl2 != null)
            {
                regionControl2.ParentKey = "Country." + countryCode;
            }
        }

        private bool IsCoupledRegionControl(Control ctr)
        {
            if (ctr is DNNRegionEditControl)
            {
                var c = (DNNRegionEditControl)ctr;
                if (c.Category == this.Category)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
