// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls;

using System;
using System.ComponentModel;
using System.IO;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>A drop down list of countries.</summary>
/// <seealso cref="DropDownList"/>
[ToolboxData("<{0}:CountryListBox runat=server></{0}:CountryListBox>")]
public class CountryListBox : DropDownList
{
    private bool cacheGeoIPData = true;

    /// <summary>Gets or sets a value indicating whether to cache the GeoIP data.</summary>
    [Bindable(true)]
    [Category("Caching")]
    [DefaultValue(true)]
    public bool CacheGeoIPData
    {
        get
        {
            return this.cacheGeoIPData;
        }

        set
        {
            this.cacheGeoIPData = value;
            if (value == false)
            {
                this.Context.Cache.Remove("GeoIPData");
            }
        }
    }

    /// <summary>Gets or sets the path to the GeoIP file.</summary>
    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue("")]
    public string GeoIPFile { get; set; }

    /// <summary>Gets or sets the test IP address.</summary>
    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue("")]
    public string TestIP { get; set; }

    /// <summary>Gets or sets country code to use for localhost.</summary>
    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue("")]
    public string LocalhostCountryCode { get; set; }

    /// <inheritdoc/>
    protected override void OnDataBinding(EventArgs e)
    {
        bool isLocal = false;
        string ip;
        if (!this.Page.IsPostBack)
        {
            // If GeoIPFile is not provided, assume they put it in BIN.
            if (string.IsNullOrEmpty(this.GeoIPFile))
            {
                this.GeoIPFile = "controls/CountryListBox/Data/GeoIP.dat";
            }

            this.EnsureChildControls();

            // Check to see if a TestIP is specified
            if (!string.IsNullOrEmpty(this.TestIP))
            {
                // TestIP is specified, let's use it
                ip = this.TestIP;
            }
            else if (this.Page.Request.UserHostAddress == "127.0.0.1")
            {
                // The country cannot be detected because the user is local.
                isLocal = true;

                // Set the IP address in case they didn't specify LocalhostCountryCode
                ip = this.Page.Request.UserHostAddress;
            }
            else
            {
                // Set the IP address so we can find the country
                ip = this.Page.Request.UserHostAddress;
            }

            // Check to see if we need to generate the Cache for the GeoIPData file
            if (this.Context.Cache.Get("GeoIPData") == null && this.cacheGeoIPData)
            {
                // Store it as  well as setting a dependency on the file
                this.Context.Cache.Insert("GeoIPData", CountryLookup.FileToMemory(this.Context.Server.MapPath(this.GeoIPFile)), new CacheDependency(this.Context.Server.MapPath(this.GeoIPFile)));
            }

            // Check to see if the request is a localhost request
            // and see if the LocalhostCountryCode is specified
            if (isLocal && !string.IsNullOrEmpty(this.LocalhostCountryCode))
            {
                // Bing the data
                base.OnDataBinding(e);

                // Pre-Select the value in the drop-down based
                // on the LocalhostCountryCode specified.
                if (this.Items.FindByValue(this.LocalhostCountryCode) != null)
                {
                    this.Items.FindByValue(this.LocalhostCountryCode).Selected = true;
                }
            }
            else
            {
                // Either this is a remote request or it is a local
                // request with no LocalhostCountryCode specified
                CountryLookup countryLookup;

                // Check to see if we are using the Cached
                // version of the GeoIPData file
                if (this.cacheGeoIPData)
                {
                    // Yes, get it from cache
                    countryLookup = new CountryLookup((MemoryStream)this.Context.Cache.Get("GeoIPData"));
                }
                else
                {
                    // No, get it from file
                    countryLookup = new CountryLookup(this.Context.Server.MapPath(this.GeoIPFile));
                }

                // Get the country code based on the IP address
                string userCountryCode = countryLookup.LookupCountryCode(ip);

                // Bind the datasource
                base.OnDataBinding(e);

                // Make sure the value returned is actually
                // in the drop-down list.
                if (this.Items.FindByValue(userCountryCode) != null)
                {
                    // Yes, it's there, select it based on its value
                    this.Items.FindByValue(userCountryCode).Selected = true;
                }
                else
                {
                    // No it's not there.  Let's get the Country description
                    // and add a new list item for the Country detected
                    string userCountry = countryLookup.LookupCountryName(ip);
                    if (userCountry != "N/A")
                    {
                        var newItem = new ListItem();
                        newItem.Value = userCountryCode;
                        newItem.Text = userCountry;
                        this.Items.Insert(0, newItem);

                        // Now let's Pre-Select it
                        this.Items.FindByValue(userCountryCode).Selected = true;
                    }
                }
            }
        }
    }
}
