// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Web.Caching;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:CountryListBox runat=server></{0}:CountryListBox>")]
    public class CountryListBox : DropDownList
    {
        private bool _CacheGeoIPData = true;
        private string _GeoIPFile;
        private string _LocalhostCountryCode;
        private string _TestIP;

        [Bindable(true)]
        [Category("Caching")]
        [DefaultValue(true)]
        public bool CacheGeoIPData
        {
            get
            {
                return this._CacheGeoIPData;
            }

            set
            {
                this._CacheGeoIPData = value;
                if (value == false)
                {
                    this.Context.Cache.Remove("GeoIPData");
                }
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string GeoIPFile
        {
            get
            {
                return this._GeoIPFile;
            }

            set
            {
                this._GeoIPFile = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string TestIP
        {
            get
            {
                return this._TestIP;
            }

            set
            {
                this._TestIP = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string LocalhostCountryCode
        {
            get
            {
                return this._LocalhostCountryCode;
            }

            set
            {
                this._LocalhostCountryCode = value;
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            bool IsLocal = false;
            string IP;
            if (!this.Page.IsPostBack)
            {
                // If GeoIPFile is not provided, assume they put it in BIN.
                if (string.IsNullOrEmpty(this._GeoIPFile))
                {
                    this._GeoIPFile = "controls/CountryListBox/Data/GeoIP.dat";
                }

                this.EnsureChildControls();

                // Check to see if a TestIP is specified
                if (!string.IsNullOrEmpty(this._TestIP))
                {
                    // TestIP is specified, let's use it
                    IP = this._TestIP;
                }
                else if (this.Page.Request.UserHostAddress == "127.0.0.1")
                {
                    // The country cannot be detected because the user is local.
                    IsLocal = true;

                    // Set the IP address in case they didn't specify LocalhostCountryCode
                    IP = this.Page.Request.UserHostAddress;
                }
                else
                {
                    // Set the IP address so we can find the country
                    IP = this.Page.Request.UserHostAddress;
                }

                // Check to see if we need to generate the Cache for the GeoIPData file
                if (this.Context.Cache.Get("GeoIPData") == null && this._CacheGeoIPData)
                {
                    // Store it as  well as setting a dependency on the file
                    this.Context.Cache.Insert("GeoIPData", CountryLookup.FileToMemory(this.Context.Server.MapPath(this._GeoIPFile)), new CacheDependency(this.Context.Server.MapPath(this._GeoIPFile)));
                }

                // Check to see if the request is a localhost request
                // and see if the LocalhostCountryCode is specified
                if (IsLocal && !string.IsNullOrEmpty(this._LocalhostCountryCode))
                {
                    // Bing the data
                    base.OnDataBinding(e);

                    // Pre-Select the value in the drop-down based
                    // on the LocalhostCountryCode specified.
                    if (this.Items.FindByValue(this._LocalhostCountryCode) != null)
                    {
                        this.Items.FindByValue(this._LocalhostCountryCode).Selected = true;
                    }
                }
                else
                {
                    // Either this is a remote request or it is a local
                    // request with no LocalhostCountryCode specified
                    CountryLookup _CountryLookup;

                    // Check to see if we are using the Cached
                    // version of the GeoIPData file
                    if (this._CacheGeoIPData)
                    {
                        // Yes, get it from cache
                        _CountryLookup = new CountryLookup((MemoryStream)this.Context.Cache.Get("GeoIPData"));
                    }
                    else
                    {
                        // No, get it from file
                        _CountryLookup = new CountryLookup(this.Context.Server.MapPath(this._GeoIPFile));
                    }

                    // Get the country code based on the IP address
                    string _UserCountryCode = _CountryLookup.LookupCountryCode(IP);

                    // Bind the datasource
                    base.OnDataBinding(e);

                    // Make sure the value returned is actually
                    // in the drop-down list.
                    if (this.Items.FindByValue(_UserCountryCode) != null)
                    {
                        // Yes, it's there, select it based on its value
                        this.Items.FindByValue(_UserCountryCode).Selected = true;
                    }
                    else
                    {
                        // No it's not there.  Let's get the Country description
                        // and add a new list item for the Country detected
                        string _UserCountry = _CountryLookup.LookupCountryName(IP);
                        if (_UserCountry != "N/A")
                        {
                            var newItem = new ListItem();
                            newItem.Value = _UserCountryCode;
                            newItem.Text = _UserCountry;
                            this.Items.Insert(0, newItem);

                            // Now let's Pre-Select it
                            this.Items.FindByValue(_UserCountryCode).Selected = true;
                        }
                    }
                }
            }
        }
    }
}
