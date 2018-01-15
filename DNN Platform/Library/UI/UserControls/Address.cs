#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
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

#endregion

namespace DotNetNuke.UI.UserControls
{

	/// <summary>
	/// The Address UserControl is used to manage User Addresses
	/// </summary>
	/// <remarks>
	/// </remarks>
	public abstract class Address : UserControlBase
	{

		#region Private Members
		
		private const string MyFileName = "Address.ascx";
		private string _cell;
		private string _city;
		private string _controlColumnWidth = "";
		private string _country;
		private string _countryData = "Text";
		private string _fax;
		private string _labelColumnWidth = "";
		private int _moduleId;
		private string _postal;
		private string _region;
		private string _regionData = "Text";
		private bool _showCell = true;
		private bool _showCity = true;
		private bool _showCountry = true;
		private bool _showFax = true;
		private bool _showPostal = true;
		private bool _showRegion = true;
		private bool _showStreet = true;
		private bool _showTelephone = true;
		private bool _showUnit = true;
		private string _street;
		private string _telephone;
		private string _unit;
		protected CountryListBox cboCountry;
		protected DropDownList cboRegion;
		protected CheckBox chkCell;
		protected CheckBox chkCity;
		protected CheckBox chkCountry;
		protected CheckBox chkFax;
		protected CheckBox chkPostal;
		protected CheckBox chkRegion;
		protected CheckBox chkStreet;
		protected CheckBox chkTelephone;
		protected LabelControl plCell;
		protected LabelControl plCity;
		protected LabelControl plCountry;
		protected LabelControl plFax;
		protected LabelControl plPostal;
		protected LabelControl plRegion;
		protected LabelControl plStreet;
		protected LabelControl plTelephone;
		protected LabelControl plUnit;
		protected HtmlGenericControl divCell;
		protected HtmlGenericControl divCity;
		protected HtmlGenericControl divCountry;
		protected HtmlGenericControl divFax;
		protected HtmlGenericControl divPostal;
		protected HtmlGenericControl divRegion;
		protected HtmlGenericControl divStreet;
		protected HtmlGenericControl divTelephone;
		protected HtmlGenericControl divUnit;
		protected TextBox txtCell;
		protected TextBox txtCity;
		protected TextBox txtFax;
		protected TextBox txtPostal;
		protected TextBox txtRegion;
		protected TextBox txtStreet;
		protected TextBox txtTelephone;
		protected TextBox txtUnit;
		protected RequiredFieldValidator valCell;
		protected RequiredFieldValidator valCity;
		protected RequiredFieldValidator valCountry;
		protected RequiredFieldValidator valFax;
		protected RequiredFieldValidator valPostal;
		protected RequiredFieldValidator valRegion1;
		protected RequiredFieldValidator valRegion2;
		protected RequiredFieldValidator valStreet;
		protected RequiredFieldValidator valTelephone;
		
		#endregion

		#region Constructors

		protected Address()
		{
			StartTabIndex = 1;
		}
		
		#endregion

		#region Properties

		public int ModuleId
		{
			get
			{
				return Convert.ToInt32(ViewState["ModuleId"]);
			}
			set
			{
				_moduleId = value;
			}
		}

		public string LabelColumnWidth
		{
			get
			{
				return Convert.ToString(ViewState["LabelColumnWidth"]);
			}
			set
			{
				_labelColumnWidth = value;
			}
		}

		public string ControlColumnWidth
		{
			get
			{
				return Convert.ToString(ViewState["ControlColumnWidth"]);
			}
			set
			{
				_controlColumnWidth = value;
			}
		}

		public int StartTabIndex { private get; set; }

		public string Street
		{
			get
			{
				return txtStreet.Text;
			}
			set
			{
				_street = value;
			}
		}

		public string Unit
		{
			get
			{
				return txtUnit.Text;
			}
			set
			{
				_unit = value;
			}
		}

		public string City
		{
			get
			{
				return txtCity.Text;
			}
			set
			{
				_city = value;
			}
		}

		public string Country
		{
			get
			{
				var retValue = "";
				if (cboCountry.SelectedItem != null)
				{
					switch (_countryData.ToLower())
					{
						case "text":
							retValue = cboCountry.SelectedIndex == 0 ? "" : cboCountry.SelectedItem.Text;
							break;
						case "value":
							retValue = cboCountry.SelectedItem.Value;
							break;
					}
				}
				return retValue;
			}
			set
			{
				_country = value;
			}
		}

		public string Region
		{
			get
			{
				var retValue = "";
				if (cboRegion.Visible)
				{
					if (cboRegion.SelectedItem != null)
					{
						switch (_regionData.ToLower())
						{
							case "text":
								if (cboRegion.SelectedIndex > 0)
								{
									retValue = cboRegion.SelectedItem.Text;
								}
								break;
							case "value":
								retValue = cboRegion.SelectedItem.Value;
								break;
						}
					}
				}
				else
				{
					retValue = txtRegion.Text;
				}
				return retValue;
			}
			set
			{
				_region = value;
			}
		}

		public string Postal
		{
			get
			{
				return txtPostal.Text;
			}
			set
			{
				_postal = value;
			}
		}

		public string Telephone
		{
			get
			{
				return txtTelephone.Text;
			}
			set
			{
				_telephone = value;
			}
		}

		public string Cell
		{
			get
			{
				return txtCell.Text;
			}
			set
			{
				_cell = value;
			}
		}

		public string Fax
		{
			get
			{
				return txtFax.Text;
			}
			set
			{
				_fax = value;
			}
		}

		public bool ShowStreet
		{
			set
			{
				_showStreet = value;
			}
		}

		public bool ShowUnit
		{
			set
			{
				_showUnit = value;
			}
		}

		public bool ShowCity
		{
			set
			{
				_showCity = value;
			}
		}

		public bool ShowCountry
		{
			set
			{
				_showCountry = value;
			}
		}

		public bool ShowRegion
		{
			set
			{
				_showRegion = value;
			}
		}

		public bool ShowPostal
		{
			set
			{
				_showPostal = value;
			}
		}

		public bool ShowTelephone
		{
			set
			{
				_showTelephone = value;
			}
		}

		public bool ShowCell
		{
			set
			{
				_showCell = value;
			}
		}

		public bool ShowFax
		{
			set
			{
				_showFax = value;
			}
		}

		public string CountryData
		{
			set
			{
				_countryData = value;
			}
		}

		public string RegionData
		{
			set
			{
				_regionData = value;
			}
		}

		public string LocalResourceFile
		{
			get
			{
				return Localization.GetResourceFile(this, MyFileName);
			}
		}
		
		#endregion

		#region Private Methods

		/// <summary>
		/// Localize correctly sets up the control for US/Canada/Other Countries
		/// </summary>
		/// <remarks>
		/// </remarks>
		private void Localize()
		{
			var countryCode = cboCountry.SelectedItem.Value;
			var ctlEntry = new ListController();
			//listKey in format "Country.US:Region"
			var listKey = "Country." + countryCode;
			var entryCollection = ctlEntry.GetListEntryInfoItems("Region", listKey);

			if (entryCollection.Any())
			{
				cboRegion.Visible = true;
				txtRegion.Visible = false;
				{
					cboRegion.Items.Clear();
					cboRegion.DataSource = entryCollection;
					cboRegion.DataBind();
					cboRegion.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", ""));
				}
				if (countryCode.ToLower() == "us")
				{
					valRegion1.Enabled = true;
					valRegion2.Enabled = false;
					valRegion1.ErrorMessage = Localization.GetString("StateRequired", Localization.GetResourceFile(this, MyFileName));
					plRegion.Text = Localization.GetString("plState", Localization.GetResourceFile(this, MyFileName));
					plRegion.HelpText = Localization.GetString("plState.Help", Localization.GetResourceFile(this, MyFileName));
					plPostal.Text = Localization.GetString("plZip", Localization.GetResourceFile(this, MyFileName));
					plPostal.HelpText = Localization.GetString("plZip.Help", Localization.GetResourceFile(this, MyFileName));
				}
				else
				{
					valRegion1.ErrorMessage = Localization.GetString("ProvinceRequired", Localization.GetResourceFile(this, MyFileName));
					plRegion.Text = Localization.GetString("plProvince", Localization.GetResourceFile(this, MyFileName));
					plRegion.HelpText = Localization.GetString("plProvince.Help", Localization.GetResourceFile(this, MyFileName));
					plPostal.Text = Localization.GetString("plPostal", Localization.GetResourceFile(this, MyFileName));
					plPostal.HelpText = Localization.GetString("plPostal.Help", Localization.GetResourceFile(this, MyFileName));
				}
				valRegion1.Enabled = true;
				valRegion2.Enabled = false;
			}
			else
			{
				cboRegion.ClearSelection();
				cboRegion.Visible = false;
				txtRegion.Visible = true;
				valRegion1.Enabled = false;
				valRegion2.Enabled = true;
				valRegion2.ErrorMessage = Localization.GetString("RegionRequired", Localization.GetResourceFile(this, MyFileName));
				plRegion.Text = Localization.GetString("plRegion", Localization.GetResourceFile(this, MyFileName));
				plRegion.HelpText = Localization.GetString("plRegion.Help", Localization.GetResourceFile(this, MyFileName));
				plPostal.Text = Localization.GetString("plPostal", Localization.GetResourceFile(this, MyFileName));
				plPostal.HelpText = Localization.GetString("plPostal.Help", Localization.GetResourceFile(this, MyFileName));
			}

			var reqRegion = PortalController.GetPortalSettingAsBoolean("addressregion", PortalSettings.PortalId, true);
			if (reqRegion)
			{
				valRegion1.Enabled = false;
				valRegion2.Enabled = false;
			}
		}

		/// <summary>
		/// ShowRequiredFields sets up displaying which fields are required
		/// </summary>
		/// <remarks>
		/// </remarks>
		private void ShowRequiredFields()
		{
			var reqStreet = PortalController.GetPortalSettingAsBoolean("addressstreet", PortalSettings.PortalId, true);
			var reqCity = PortalController.GetPortalSettingAsBoolean("addresscity", PortalSettings.PortalId, true);
			var reqCountry = PortalController.GetPortalSettingAsBoolean("addresscountry", PortalSettings.PortalId, true);
			var reqRegion = PortalController.GetPortalSettingAsBoolean("addressregion", PortalSettings.PortalId, true);
			var reqPostal = PortalController.GetPortalSettingAsBoolean("addresspostal", PortalSettings.PortalId, true);
			var reqTelephone = PortalController.GetPortalSettingAsBoolean("addresstelephone", PortalSettings.PortalId, true);
			var reqCell = PortalController.GetPortalSettingAsBoolean("addresscell", PortalSettings.PortalId, true);
			var reqFax = PortalController.GetPortalSettingAsBoolean("addressfax", PortalSettings.PortalId, true);

			if (TabPermissionController.CanAdminPage())
			{
				if (reqCountry)
				{
					chkCountry.Checked = true;
					valCountry.Enabled = true;
					cboCountry.CssClass = "dnnFormRequired";
				}
				else
				{
				    valCountry.Enabled = false;
					cboCountry.CssClass = "";
				}
				if (reqRegion)
				{
					chkRegion.Checked = true;
					txtRegion.CssClass = "dnnFormRequired";
					cboRegion.CssClass = "dnnFormRequired";

					if (cboRegion.Visible)
					{
						valRegion1.Enabled = true;
						valRegion2.Enabled = false;
					}
					else
					{
						valRegion1.Enabled = false;
						valRegion2.Enabled = true;
					}
				}
				else
				{
				    valRegion1.Enabled = false;
				    valRegion2.Enabled = false;
					txtRegion.CssClass = "";
					cboRegion.CssClass = "";
				}
				if (reqCity)
				{
					chkCity.Checked = true;
					valCity.Enabled = true;
					txtCity.CssClass = "dnnFormRequired";
				}
				else
				{
				    valCity.Enabled = false;
					txtCity.CssClass = "";
				}
				if (reqStreet)
				{
					chkStreet.Checked = true;
					valStreet.Enabled = true;
					txtStreet.CssClass = "dnnFormRequired";
				}
				else
				{
				    valStreet.Enabled = false;
					txtStreet.CssClass = "";
				}
				if (reqPostal)
				{
					chkPostal.Checked = true;
					valPostal.Enabled = true;
					txtPostal.CssClass = "dnnFormRequired";
				}
				else
				{
				    valPostal.Enabled = false;
					txtPostal.CssClass = "";
				}
				if (reqTelephone)
				{
					chkTelephone.Checked = true;
					valTelephone.Enabled = true;
					txtTelephone.CssClass = "dnnFormRequired";
				}
				else
				{
				    valTelephone.Enabled = false;
					txtTelephone.CssClass = "";
				}
				if (reqCell)
				{
					chkCell.Checked = true;
					valCell.Enabled = true;
					txtCell.CssClass = "dnnFormRequired";
				}
				else
				{
				    valCell.Enabled = false;
					txtCell.CssClass = "";
				}
				if (reqFax)
				{
					chkFax.Checked = true;
					valFax.Enabled = true;
					txtFax.CssClass = "dnnFormRequired";
				}
				else
				{
				    valFax.Enabled = false;
					txtFax.CssClass = "";
				}
			}
		}

		/// <summary>
		/// UpdateRequiredFields updates which fields are required
		/// </summary>
		/// <remarks>
		/// </remarks>
		private void UpdateRequiredFields()
		{
			if (chkCountry.Checked == false)
			{
				chkRegion.Checked = false;
			}
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addressstreet", chkStreet.Checked ? "" : "N");
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addresscity", chkCity.Checked ? "" : "N");
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addresscountry", chkCountry.Checked ? "" : "N");
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addressregion", chkRegion.Checked ? "" : "N");
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addresspostal", chkPostal.Checked ? "" : "N");
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addresstelephone", chkTelephone.Checked ? "" : "N");
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addresscell", chkCell.Checked ? "" : "N");
			PortalController.UpdatePortalSetting(PortalSettings.PortalId, "addressfax", chkFax.Checked ? "" : "N");

			ShowRequiredFields();
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Page_Load runs when the control is loaded
		/// </summary>
		/// <remarks>
		/// </remarks>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			cboCountry.SelectedIndexChanged += OnCountryIndexChanged;
			chkCell.CheckedChanged += OnCellCheckChanged;
			chkCity.CheckedChanged += OnCityCheckChanged;
			chkCountry.CheckedChanged += OnCountryCheckChanged;
			chkFax.CheckedChanged += OnFaxCheckChanged;
			chkPostal.CheckedChanged += OnPostalCheckChanged;
			chkRegion.CheckedChanged += OnRegionCheckChanged;
			chkStreet.CheckedChanged += OnStreetCheckChanged;
			chkTelephone.CheckedChanged += OnTelephoneCheckChanged;

			try
			{
				valStreet.ErrorMessage = Localization.GetString("StreetRequired", Localization.GetResourceFile(this, MyFileName));
				valCity.ErrorMessage = Localization.GetString("CityRequired", Localization.GetResourceFile(this, MyFileName));
				valCountry.ErrorMessage = Localization.GetString("CountryRequired", Localization.GetResourceFile(this, MyFileName));
				valPostal.ErrorMessage = Localization.GetString("PostalRequired", Localization.GetResourceFile(this, MyFileName));
				valTelephone.ErrorMessage = Localization.GetString("TelephoneRequired", Localization.GetResourceFile(this, MyFileName));
				valCell.ErrorMessage = Localization.GetString("CellRequired", Localization.GetResourceFile(this, MyFileName));
				valFax.ErrorMessage = Localization.GetString("FaxRequired", Localization.GetResourceFile(this, MyFileName));

				if (!Page.IsPostBack)
				{
					txtStreet.TabIndex = Convert.ToInt16(StartTabIndex);
					txtUnit.TabIndex = Convert.ToInt16(StartTabIndex + 1);
					txtCity.TabIndex = Convert.ToInt16(StartTabIndex + 2);
					cboCountry.TabIndex = Convert.ToInt16(StartTabIndex + 3);
					cboRegion.TabIndex = Convert.ToInt16(StartTabIndex + 4);
					txtRegion.TabIndex = Convert.ToInt16(StartTabIndex + 5);
					txtPostal.TabIndex = Convert.ToInt16(StartTabIndex + 6);
					txtTelephone.TabIndex = Convert.ToInt16(StartTabIndex + 7);
					txtCell.TabIndex = Convert.ToInt16(StartTabIndex + 8);
					txtFax.TabIndex = Convert.ToInt16(StartTabIndex + 9);

					//<tam:note modified to test Lists
					//Dim objRegionalController As New RegionalController
					//cboCountry.DataSource = objRegionalController.GetCountries
					//<this test using method 2: get empty collection then get each entry list on demand & store into cache

					var ctlEntry = new ListController();
					var entryCollection = ctlEntry.GetListEntryInfoItems("Country");

					cboCountry.DataSource = entryCollection;
					cboCountry.DataBind();
					cboCountry.Items.Insert(0, new ListItem("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", ""));

					switch (_countryData.ToLower())
					{
						case "text":
							if (String.IsNullOrEmpty(_country))
							{
								cboCountry.SelectedIndex = 0;
							}
							else
							{
								if (cboCountry.Items.FindByText(_country) != null)
								{
									cboCountry.ClearSelection();
									cboCountry.Items.FindByText(_country).Selected = true;
								}
							}
							break;
						case "value":
							if (cboCountry.Items.FindByValue(_country) != null)
							{
								cboCountry.ClearSelection();
								cboCountry.Items.FindByValue(_country).Selected = true;
							}
							break;
					}
					Localize();

					if (cboRegion.Visible)
					{
						switch (_regionData.ToLower())
						{
							case "text":
								if (String.IsNullOrEmpty(_region))
								{
									cboRegion.SelectedIndex = 0;
								}
								else
								{
									if (cboRegion.Items.FindByText(_region) != null)
									{
										cboRegion.Items.FindByText(_region).Selected = true;
									}
								}
								break;
							case "value":
								if (cboRegion.Items.FindByValue(_region) != null)
								{
									cboRegion.Items.FindByValue(_region).Selected = true;
								}
								break;
						}
					}
					else
					{
						txtRegion.Text = _region;
					}
					txtStreet.Text = _street;
					txtUnit.Text = _unit;
					txtCity.Text = _city;
					txtPostal.Text = _postal;
					txtTelephone.Text = _telephone;
					txtCell.Text = _cell;
					txtFax.Text = _fax;

					divStreet.Visible = _showStreet;
					divUnit.Visible = _showUnit;
					divCity.Visible = _showCity;
					divCountry.Visible = _showCountry;
					divRegion.Visible = _showRegion;
					divPostal.Visible = _showPostal;
					divTelephone.Visible = _showTelephone;
					divCell.Visible = _showCell;
					divFax.Visible = _showFax;

					if (TabPermissionController.CanAdminPage())
					{
						chkStreet.Visible = true;
						chkCity.Visible = true;
						chkCountry.Visible = true;
						chkRegion.Visible = true;
						chkPostal.Visible = true;
						chkTelephone.Visible = true;
						chkCell.Visible = true;
						chkFax.Visible = true;
					}
					ViewState["ModuleId"] = Convert.ToString(_moduleId);
					ViewState["LabelColumnWidth"] = _labelColumnWidth;
					ViewState["ControlColumnWidth"] = _controlColumnWidth;

					ShowRequiredFields();
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
				Localize();
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
				UpdateRequiredFields();
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void OnCountryCheckChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateRequiredFields();
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
				UpdateRequiredFields();
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
				UpdateRequiredFields();
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
				UpdateRequiredFields();
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
				UpdateRequiredFields();
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
				UpdateRequiredFields();
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
				UpdateRequiredFields();
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}
		
		#endregion

	}
}
