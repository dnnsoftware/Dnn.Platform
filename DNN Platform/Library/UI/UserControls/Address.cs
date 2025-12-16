// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.WebControls;

    /// <summary>The Address UserControl is used to manage User Addresses.</summary>
    public abstract class Address : UserControlBase
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CountryListBox cboCountry;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected DropDownList cboRegion;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkCell;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkCity;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkCountry;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkFax;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkPostal;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkRegion;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkStreet;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected CheckBox chkTelephone;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plCell;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plCity;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plCountry;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plFax;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plPostal;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plRegion;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plStreet;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plTelephone;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected LabelControl plUnit;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divCell;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divCity;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divCountry;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divFax;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divPostal;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divRegion;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divStreet;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divTelephone;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl divUnit;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtCell;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtCity;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtFax;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtPostal;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtRegion;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtStreet;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtTelephone;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox txtUnit;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valCell;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valCity;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valCountry;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valFax;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valPostal;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valRegion1;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valRegion2;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valStreet;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RequiredFieldValidator valTelephone;
        private const string MyFileName = "Address.ascx";
        private string cell;
        private string city;
        private string controlColumnWidth = string.Empty;
        private string country;
        private string countryData = "Text";
        private string fax;
        private string labelColumnWidth = string.Empty;
        private int moduleId;
        private string postal;
        private string region;
        private string regionData = "Text";
        private bool showCell = true;
        private bool showCity = true;
        private bool showCountry = true;
        private bool showFax = true;
        private bool showPostal = true;
        private bool showRegion = true;
        private bool showStreet = true;
        private bool showTelephone = true;
        private bool showUnit = true;
        private string street;
        private string telephone;
        private string unit;

        /// <summary>Initializes a new instance of the <see cref="Address"/> class.</summary>
        protected Address()
        {
            this.StartTabIndex = 1;
        }

        public string LocalResourceFile
        {
            get
            {
                return Localization.GetResourceFile(this, MyFileName);
            }
        }

        public int ModuleId
        {
            get
            {
                return Convert.ToInt32(this.ViewState["ModuleId"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.moduleId = value;
            }
        }

        public string LabelColumnWidth
        {
            get
            {
                return Convert.ToString(this.ViewState["LabelColumnWidth"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.labelColumnWidth = value;
            }
        }

        public string ControlColumnWidth
        {
            get
            {
                return Convert.ToString(this.ViewState["ControlColumnWidth"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.controlColumnWidth = value;
            }
        }

        public int StartTabIndex { private get; set; }

        public string Street
        {
            get
            {
                return this.txtStreet.Text;
            }

            set
            {
                this.street = value;
            }
        }

        public string Unit
        {
            get
            {
                return this.txtUnit.Text;
            }

            set
            {
                this.unit = value;
            }
        }

        public string City
        {
            get
            {
                return this.txtCity.Text;
            }

            set
            {
                this.city = value;
            }
        }

        public string Country
        {
            get
            {
                var retValue = string.Empty;
                if (this.cboCountry.SelectedItem != null)
                {
                    switch (this.countryData.ToLowerInvariant())
                    {
                        case "text":
                            retValue = this.cboCountry.SelectedIndex == 0 ? string.Empty : this.cboCountry.SelectedItem.Text;
                            break;
                        case "value":
                            retValue = this.cboCountry.SelectedItem.Value;
                            break;
                    }
                }

                return retValue;
            }

            set
            {
                this.country = value;
            }
        }

        public string Region
        {
            get
            {
                var retValue = string.Empty;
                if (this.cboRegion.Visible)
                {
                    if (this.cboRegion.SelectedItem != null)
                    {
                        switch (this.regionData.ToLowerInvariant())
                        {
                            case "text":
                                if (this.cboRegion.SelectedIndex > 0)
                                {
                                    retValue = this.cboRegion.SelectedItem.Text;
                                }

                                break;
                            case "value":
                                retValue = this.cboRegion.SelectedItem.Value;
                                break;
                        }
                    }
                }
                else
                {
                    retValue = this.txtRegion.Text;
                }

                return retValue;
            }

            set
            {
                this.region = value;
            }
        }

        public string Postal
        {
            get
            {
                return this.txtPostal.Text;
            }

            set
            {
                this.postal = value;
            }
        }

        public string Telephone
        {
            get
            {
                return this.txtTelephone.Text;
            }

            set
            {
                this.telephone = value;
            }
        }

        public string Cell
        {
            get
            {
                return this.txtCell.Text;
            }

            set
            {
                this.cell = value;
            }
        }

        public string Fax
        {
            get
            {
                return this.txtFax.Text;
            }

            set
            {
                this.fax = value;
            }
        }

        public bool ShowStreet
        {
            set
            {
                this.showStreet = value;
            }
        }

        public bool ShowUnit
        {
            set
            {
                this.showUnit = value;
            }
        }

        public bool ShowCity
        {
            set
            {
                this.showCity = value;
            }
        }

        public bool ShowCountry
        {
            set
            {
                this.showCountry = value;
            }
        }

        public bool ShowRegion
        {
            set
            {
                this.showRegion = value;
            }
        }

        public bool ShowPostal
        {
            set
            {
                this.showPostal = value;
            }
        }

        public bool ShowTelephone
        {
            set
            {
                this.showTelephone = value;
            }
        }

        public bool ShowCell
        {
            set
            {
                this.showCell = value;
            }
        }

        public bool ShowFax
        {
            set
            {
                this.showFax = value;
            }
        }

        public string CountryData
        {
            set
            {
                this.countryData = value;
            }
        }

        public string RegionData
        {
            set
            {
                this.regionData = value;
            }
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cboCountry.SelectedIndexChanged += this.OnCountryIndexChanged;
            this.chkCell.CheckedChanged += this.OnCellCheckChanged;
            this.chkCity.CheckedChanged += this.OnCityCheckChanged;
            this.chkCountry.CheckedChanged += this.OnCountryCheckChanged;
            this.chkFax.CheckedChanged += this.OnFaxCheckChanged;
            this.chkPostal.CheckedChanged += this.OnPostalCheckChanged;
            this.chkRegion.CheckedChanged += this.OnRegionCheckChanged;
            this.chkStreet.CheckedChanged += this.OnStreetCheckChanged;
            this.chkTelephone.CheckedChanged += this.OnTelephoneCheckChanged;

            try
            {
                this.valStreet.ErrorMessage = Localization.GetString("StreetRequired", Localization.GetResourceFile(this, MyFileName));
                this.valCity.ErrorMessage = Localization.GetString("CityRequired", Localization.GetResourceFile(this, MyFileName));
                this.valCountry.ErrorMessage = Localization.GetString("CountryRequired", Localization.GetResourceFile(this, MyFileName));
                this.valPostal.ErrorMessage = Localization.GetString("PostalRequired", Localization.GetResourceFile(this, MyFileName));
                this.valTelephone.ErrorMessage = Localization.GetString("TelephoneRequired", Localization.GetResourceFile(this, MyFileName));
                this.valCell.ErrorMessage = Localization.GetString("CellRequired", Localization.GetResourceFile(this, MyFileName));
                this.valFax.ErrorMessage = Localization.GetString("FaxRequired", Localization.GetResourceFile(this, MyFileName));

                if (!this.Page.IsPostBack)
                {
                    this.txtStreet.TabIndex = Convert.ToInt16(this.StartTabIndex);
                    this.txtUnit.TabIndex = Convert.ToInt16(this.StartTabIndex + 1);
                    this.txtCity.TabIndex = Convert.ToInt16(this.StartTabIndex + 2);
                    this.cboCountry.TabIndex = Convert.ToInt16(this.StartTabIndex + 3);
                    this.cboRegion.TabIndex = Convert.ToInt16(this.StartTabIndex + 4);
                    this.txtRegion.TabIndex = Convert.ToInt16(this.StartTabIndex + 5);
                    this.txtPostal.TabIndex = Convert.ToInt16(this.StartTabIndex + 6);
                    this.txtTelephone.TabIndex = Convert.ToInt16(this.StartTabIndex + 7);
                    this.txtCell.TabIndex = Convert.ToInt16(this.StartTabIndex + 8);
                    this.txtFax.TabIndex = Convert.ToInt16(this.StartTabIndex + 9);

                    // <tam:note modified to test Lists
                    // Dim objRegionalController As New RegionalController
                    // cboCountry.DataSource = objRegionalController.GetCountries
                    // <this test using method 2: get empty collection then get each entry list on demand & store into cache
                    var ctlEntry = new ListController();
                    var entryCollection = ctlEntry.GetListEntryInfoItems("Country");

                    this.cboCountry.DataSource = entryCollection;
                    this.cboCountry.DataBind();
                    this.cboCountry.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", string.Empty));

                    switch (this.countryData.ToLowerInvariant())
                    {
                        case "text":
                            if (string.IsNullOrEmpty(this.country))
                            {
                                this.cboCountry.SelectedIndex = 0;
                            }
                            else
                            {
                                if (this.cboCountry.Items.FindByText(this.country) != null)
                                {
                                    this.cboCountry.ClearSelection();
                                    this.cboCountry.Items.FindByText(this.country).Selected = true;
                                }
                            }

                            break;
                        case "value":
                            if (this.cboCountry.Items.FindByValue(this.country) != null)
                            {
                                this.cboCountry.ClearSelection();
                                this.cboCountry.Items.FindByValue(this.country).Selected = true;
                            }

                            break;
                    }

                    this.Localize();

                    if (this.cboRegion.Visible)
                    {
                        switch (this.regionData.ToLowerInvariant())
                        {
                            case "text":
                                if (string.IsNullOrEmpty(this.region))
                                {
                                    this.cboRegion.SelectedIndex = 0;
                                }
                                else
                                {
                                    if (this.cboRegion.Items.FindByText(this.region) != null)
                                    {
                                        this.cboRegion.Items.FindByText(this.region).Selected = true;
                                    }
                                }

                                break;
                            case "value":
                                if (this.cboRegion.Items.FindByValue(this.region) != null)
                                {
                                    this.cboRegion.Items.FindByValue(this.region).Selected = true;
                                }

                                break;
                        }
                    }
                    else
                    {
                        this.txtRegion.Text = this.region;
                    }

                    this.txtStreet.Text = this.street;
                    this.txtUnit.Text = this.unit;
                    this.txtCity.Text = this.city;
                    this.txtPostal.Text = this.postal;
                    this.txtTelephone.Text = this.telephone;
                    this.txtCell.Text = this.cell;
                    this.txtFax.Text = this.fax;

                    this.divStreet.Visible = this.showStreet;
                    this.divUnit.Visible = this.showUnit;
                    this.divCity.Visible = this.showCity;
                    this.divCountry.Visible = this.showCountry;
                    this.divRegion.Visible = this.showRegion;
                    this.divPostal.Visible = this.showPostal;
                    this.divTelephone.Visible = this.showTelephone;
                    this.divCell.Visible = this.showCell;
                    this.divFax.Visible = this.showFax;

                    if (TabPermissionController.CanAdminPage())
                    {
                        this.chkStreet.Visible = true;
                        this.chkCity.Visible = true;
                        this.chkCountry.Visible = true;
                        this.chkRegion.Visible = true;
                        this.chkPostal.Visible = true;
                        this.chkTelephone.Visible = true;
                        this.chkCell.Visible = true;
                        this.chkFax.Visible = true;
                    }

                    this.ViewState["ModuleId"] = Convert.ToString(this.moduleId, CultureInfo.InvariantCulture);
                    this.ViewState["LabelColumnWidth"] = this.labelColumnWidth;
                    this.ViewState["ControlColumnWidth"] = this.controlColumnWidth;

                    this.ShowRequiredFields();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnCountryIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.Localize();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnCityCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnCountryCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnPostalCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnRegionCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnStreetCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnTelephoneCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnCellCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnFaxCheckChanged(object sender, EventArgs e)
        {
            try
            {
                this.UpdateRequiredFields();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>Localize correctly sets up the control for US/Canada/Other Countries.</summary>
        private void Localize()
        {
            var countryCode = this.cboCountry.SelectedItem.Value;
            var ctlEntry = new ListController();

            // listKey in format "Country.US:Region"
            var listKey = "Country." + countryCode;
            var entryCollection = ctlEntry.GetListEntryInfoItems("Region", listKey);

            if (entryCollection.Any())
            {
                this.cboRegion.Visible = true;
                this.txtRegion.Visible = false;
                {
                    this.cboRegion.Items.Clear();
                    this.cboRegion.DataSource = entryCollection;
                    this.cboRegion.DataBind();
                    this.cboRegion.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", string.Empty));
                }

                if (countryCode.Equals("us", StringComparison.OrdinalIgnoreCase))
                {
                    this.valRegion1.Enabled = true;
                    this.valRegion2.Enabled = false;
                    this.valRegion1.ErrorMessage = Localization.GetString("StateRequired", Localization.GetResourceFile(this, MyFileName));
                    this.plRegion.Text = Localization.GetString("plState", Localization.GetResourceFile(this, MyFileName));
                    this.plRegion.HelpText = Localization.GetString("plState.Help", Localization.GetResourceFile(this, MyFileName));
                    this.plPostal.Text = Localization.GetString("plZip", Localization.GetResourceFile(this, MyFileName));
                    this.plPostal.HelpText = Localization.GetString("plZip.Help", Localization.GetResourceFile(this, MyFileName));
                }
                else
                {
                    this.valRegion1.ErrorMessage = Localization.GetString("ProvinceRequired", Localization.GetResourceFile(this, MyFileName));
                    this.plRegion.Text = Localization.GetString("plProvince", Localization.GetResourceFile(this, MyFileName));
                    this.plRegion.HelpText = Localization.GetString("plProvince.Help", Localization.GetResourceFile(this, MyFileName));
                    this.plPostal.Text = Localization.GetString("plPostal", Localization.GetResourceFile(this, MyFileName));
                    this.plPostal.HelpText = Localization.GetString("plPostal.Help", Localization.GetResourceFile(this, MyFileName));
                }

                this.valRegion1.Enabled = true;
                this.valRegion2.Enabled = false;
            }
            else
            {
                this.cboRegion.ClearSelection();
                this.cboRegion.Visible = false;
                this.txtRegion.Visible = true;
                this.valRegion1.Enabled = false;
                this.valRegion2.Enabled = true;
                this.valRegion2.ErrorMessage = Localization.GetString("RegionRequired", Localization.GetResourceFile(this, MyFileName));
                this.plRegion.Text = Localization.GetString("plRegion", Localization.GetResourceFile(this, MyFileName));
                this.plRegion.HelpText = Localization.GetString("plRegion.Help", Localization.GetResourceFile(this, MyFileName));
                this.plPostal.Text = Localization.GetString("plPostal", Localization.GetResourceFile(this, MyFileName));
                this.plPostal.HelpText = Localization.GetString("plPostal.Help", Localization.GetResourceFile(this, MyFileName));
            }

            var reqRegion = PortalController.GetPortalSettingAsBoolean("addressregion", this.PortalSettings.PortalId, true);
            if (reqRegion)
            {
                this.valRegion1.Enabled = false;
                this.valRegion2.Enabled = false;
            }
        }

        /// <summary>ShowRequiredFields sets up displaying which fields are required.</summary>
        private void ShowRequiredFields()
        {
            var reqStreet = PortalController.GetPortalSettingAsBoolean("addressstreet", this.PortalSettings.PortalId, true);
            var reqCity = PortalController.GetPortalSettingAsBoolean("addresscity", this.PortalSettings.PortalId, true);
            var reqCountry = PortalController.GetPortalSettingAsBoolean("addresscountry", this.PortalSettings.PortalId, true);
            var reqRegion = PortalController.GetPortalSettingAsBoolean("addressregion", this.PortalSettings.PortalId, true);
            var reqPostal = PortalController.GetPortalSettingAsBoolean("addresspostal", this.PortalSettings.PortalId, true);
            var reqTelephone = PortalController.GetPortalSettingAsBoolean("addresstelephone", this.PortalSettings.PortalId, true);
            var reqCell = PortalController.GetPortalSettingAsBoolean("addresscell", this.PortalSettings.PortalId, true);
            var reqFax = PortalController.GetPortalSettingAsBoolean("addressfax", this.PortalSettings.PortalId, true);

            if (TabPermissionController.CanAdminPage())
            {
                if (reqCountry)
                {
                    this.chkCountry.Checked = true;
                    this.valCountry.Enabled = true;
                    this.cboCountry.CssClass = "dnnFormRequired";
                }
                else
                {
                    this.valCountry.Enabled = false;
                    this.cboCountry.CssClass = string.Empty;
                }

                if (reqRegion)
                {
                    this.chkRegion.Checked = true;
                    this.txtRegion.CssClass = "dnnFormRequired";
                    this.cboRegion.CssClass = "dnnFormRequired";

                    if (this.cboRegion.Visible)
                    {
                        this.valRegion1.Enabled = true;
                        this.valRegion2.Enabled = false;
                    }
                    else
                    {
                        this.valRegion1.Enabled = false;
                        this.valRegion2.Enabled = true;
                    }
                }
                else
                {
                    this.valRegion1.Enabled = false;
                    this.valRegion2.Enabled = false;
                    this.txtRegion.CssClass = string.Empty;
                    this.cboRegion.CssClass = string.Empty;
                }

                if (reqCity)
                {
                    this.chkCity.Checked = true;
                    this.valCity.Enabled = true;
                    this.txtCity.CssClass = "dnnFormRequired";
                }
                else
                {
                    this.valCity.Enabled = false;
                    this.txtCity.CssClass = string.Empty;
                }

                if (reqStreet)
                {
                    this.chkStreet.Checked = true;
                    this.valStreet.Enabled = true;
                    this.txtStreet.CssClass = "dnnFormRequired";
                }
                else
                {
                    this.valStreet.Enabled = false;
                    this.txtStreet.CssClass = string.Empty;
                }

                if (reqPostal)
                {
                    this.chkPostal.Checked = true;
                    this.valPostal.Enabled = true;
                    this.txtPostal.CssClass = "dnnFormRequired";
                }
                else
                {
                    this.valPostal.Enabled = false;
                    this.txtPostal.CssClass = string.Empty;
                }

                if (reqTelephone)
                {
                    this.chkTelephone.Checked = true;
                    this.valTelephone.Enabled = true;
                    this.txtTelephone.CssClass = "dnnFormRequired";
                }
                else
                {
                    this.valTelephone.Enabled = false;
                    this.txtTelephone.CssClass = string.Empty;
                }

                if (reqCell)
                {
                    this.chkCell.Checked = true;
                    this.valCell.Enabled = true;
                    this.txtCell.CssClass = "dnnFormRequired";
                }
                else
                {
                    this.valCell.Enabled = false;
                    this.txtCell.CssClass = string.Empty;
                }

                if (reqFax)
                {
                    this.chkFax.Checked = true;
                    this.valFax.Enabled = true;
                    this.txtFax.CssClass = "dnnFormRequired";
                }
                else
                {
                    this.valFax.Enabled = false;
                    this.txtFax.CssClass = string.Empty;
                }
            }
        }

        /// <summary>UpdateRequiredFields updates which fields are required.</summary>
        private void UpdateRequiredFields()
        {
            if (this.chkCountry.Checked == false)
            {
                this.chkRegion.Checked = false;
            }

            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addressstreet", this.chkStreet.Checked ? string.Empty : "N");
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addresscity", this.chkCity.Checked ? string.Empty : "N");
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addresscountry", this.chkCountry.Checked ? string.Empty : "N");
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addressregion", this.chkRegion.Checked ? string.Empty : "N");
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addresspostal", this.chkPostal.Checked ? string.Empty : "N");
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addresstelephone", this.chkTelephone.Checked ? string.Empty : "N");
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addresscell", this.chkCell.Checked ? string.Empty : "N");
            PortalController.UpdatePortalSetting(this.PortalSettings.PortalId, "addressfax", this.chkFax.Checked ? string.Empty : "N");

            this.ShowRequiredFields();
        }
    }
}
