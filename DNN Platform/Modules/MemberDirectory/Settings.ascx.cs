// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.MemberDirectory;

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Modules.MemberDirectory.Presenters;
using DotNetNuke.Modules.MemberDirectory.ViewModels;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls.Extensions;
using WebFormsMvp;

[DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
[PresenterBinding(typeof(ModuleSettingsPresenter))]
public partial class Settings : SettingsView<MemberDirectorySettingsModel>
{
    public const int DefaultPageSize = 20;

    private static string templatePath = "~/DesktopModules/MemberDirectory/Templates/";

    private string defaultSearchField1 = "DisplayName";
    private string defaultSearchField2 = "Email";
    private string defaultSearchField3 = "City";
    private string defaultSearchField4 = "Country";

    private string defaultSortField = "DisplayName";
    private string defaultSortOrder = "ASC";

    private string defaultFilterBy = "None";
    private string defaultFilterValue = string.Empty;

    private string defaultDisplaySearch = "Both";
    private string defaultEnablePopUp = "false";

    private string filterBy;
    private string filterValue;

    public static string DefaultAlternateItemTemplate
    {
        get
        {
            string template;
            using (StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath(templatePath + "AlternateItemTemplate.htm")))
            {
                template = sr.ReadToEnd();
            }

            return template;
        }
    }

    public static string DefaultItemTemplate
    {
        get
        {
            string template;
            using (StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath(templatePath + "ItemTemplate.htm")))
            {
                template = sr.ReadToEnd();
            }

            return template;
        }
    }

    public static string DefaultPopUpTemplate
    {
        get
        {
            string template;
            using (StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath(templatePath + "PopUpTemplate.htm")))
            {
                template = sr.ReadToEnd();
            }

            return template;
        }
    }

    /// <inheritdoc/>
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        this.AutoDataBind = false;
    }

    /// <inheritdoc/>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (!this.IsPostBack)
        {
            if (this.Model.Groups.Count > 0)
            {
                this.groupList.DataSource = this.Model.Groups;
                this.groupList.DataBind();
                this.groupList.Items.Insert(0, new ListItem(Localization.GetString("None_Specified"), Null.NullInteger.ToString()));
            }
            else
            {
                this.filterBySelector.Items.FindByValue("Group").Enabled = false;
            }

            foreach (var rel in this.Model.Relationships)
            {
                this.relationShipList.AddItem(Localization.GetString(rel.Name, Localization.SharedResourceFile), rel.RelationshipId.ToString());
            }

            var profileResourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";

            System.Web.UI.WebControls.ListItemCollection propertiesCollection = this.GetPropertiesCollection(profileResourceFile);

            // Bind the ListItemCollection to the list
            this.propertyList.DataSource = propertiesCollection;
            this.propertyList.DataBind();

            // Insert custom properties to the Search field lists
            propertiesCollection.Insert(0, new ListItem(Localization.GetString("Username", this.LocalResourceFile), "Username"));
            propertiesCollection.Insert(1, new ListItem(Localization.GetString("DisplayName", this.LocalResourceFile), "DisplayName"));
            propertiesCollection.Insert(2, new ListItem(Localization.GetString("Email", this.LocalResourceFile), "Email"));

            // Bind the properties collection in the Search Field Lists
            this.searchField1List.DataSource = propertiesCollection;
            this.searchField1List.DataBind();

            this.searchField2List.DataSource = propertiesCollection;
            this.searchField2List.DataBind();

            this.searchField3List.DataSource = propertiesCollection;
            this.searchField3List.DataBind();

            this.searchField4List.DataSource = propertiesCollection;
            this.searchField4List.DataBind();

            this.filterBySelector.Select(this.filterBy, false, 0);

            switch (this.filterBy)
            {
                case "Group":
                    this.groupList.Select(this.filterValue, false, 0);
                    break;
                case "Relationship":
                    this.relationShipList.Select(this.filterValue, false, 0);
                    break;
                case "ProfileProperty":
                    this.propertyList.Select(this.filterValue, false, 0);
                    break;
                case "User":
                    break;
            }

            this.searchField1List.Select(this.GetTabModuleSetting("SearchField1", this.defaultSearchField1));
            this.searchField2List.Select(this.GetTabModuleSetting("SearchField2", this.defaultSearchField2));
            this.searchField3List.Select(this.GetTabModuleSetting("SearchField3", this.defaultSearchField3));
            this.searchField4List.Select(this.GetTabModuleSetting("SearchField4", this.defaultSearchField4));

            this.ExcludeHostUsersCheckBox.Checked = bool.Parse(this.GetTabModuleSetting("ExcludeHostUsers", "false"));
        }
    }

    /// <inheritdoc/>
    protected override void OnSettingsLoaded()
    {
        base.OnSettingsLoaded();

        if (!this.IsPostBack)
        {
            this.BindSortList();

            this.itemTemplate.Text = this.GetTabModuleSetting("ItemTemplate", DefaultItemTemplate);
            this.alternateItemTemplate.Text = this.GetTabModuleSetting("AlternateItemTemplate", DefaultAlternateItemTemplate);
            this.popUpTemplate.Text = this.GetTabModuleSetting("PopUpTemplate", DefaultPopUpTemplate);
            this.displaySearchList.Select(this.GetTabModuleSetting("DisplaySearch", this.defaultDisplaySearch));
            this.enablePopUp.Checked = bool.Parse(this.GetTabModuleSetting("EnablePopUp", this.defaultEnablePopUp));

            this.filterBy = this.GetModuleSetting("FilterBy", this.defaultFilterBy);
            this.filterValue = this.GetModuleSetting("FilterValue", this.defaultFilterValue);
            this.propertyValue.Text = this.GetModuleSetting("FilterPropertyValue", string.Empty);

            this.sortFieldList.Select(this.GetTabModuleSetting("SortField", this.defaultSortField));
            this.sortOrderList.Select(this.GetTabModuleSetting("SortOrder", this.defaultSortOrder));

            this.pageSize.Text = this.GetTabModuleSetting("PageSize", DefaultPageSize.ToString(CultureInfo.InvariantCulture));
            this.disablePager.Checked = bool.Parse(this.GetTabModuleSetting("DisablePaging", "False"));
        }
    }

    /// <inheritdoc/>
    protected override void OnSavingSettings()
    {
        this.Model.TabModuleSettings["ItemTemplate"] = this.itemTemplate.Text;
        this.Model.TabModuleSettings["AlternateItemTemplate"] = this.alternateItemTemplate.Text;
        this.Model.TabModuleSettings["PopUpTemplate"] = this.popUpTemplate.Text;
        this.Model.TabModuleSettings["EnablePopUp"] = this.enablePopUp.Checked.ToString(CultureInfo.InvariantCulture);

        this.filterBy = this.filterBySelector.SelectedValue;
        this.Model.ModuleSettings["FilterBy"] = this.filterBy;

        switch (this.filterBy)
        {
            case "Group":
                this.Model.ModuleSettings["FilterValue"] = this.groupList.SelectedValue;
                break;
            case "Relationship":
                this.Model.ModuleSettings["FilterValue"] = this.relationShipList.SelectedValue;
                break;
            case "ProfileProperty":
                this.Model.ModuleSettings["FilterValue"] = this.propertyList.SelectedValue;
                break;
        }

        this.Model.ModuleSettings["FilterPropertyValue"] = this.propertyValue.Text;

        this.Model.TabModuleSettings["SortField"] = this.sortFieldList.SelectedValue;
        this.Model.TabModuleSettings["SortOrder"] = this.sortOrderList.SelectedValue;

        this.Model.TabModuleSettings["SearchField1"] = this.searchField1List.SelectedValue;
        this.Model.TabModuleSettings["SearchField2"] = this.searchField2List.SelectedValue;
        this.Model.TabModuleSettings["SearchField3"] = this.searchField3List.SelectedValue;
        this.Model.TabModuleSettings["SearchField4"] = this.searchField4List.SelectedValue;
        this.Model.TabModuleSettings["DisplaySearch"] = this.displaySearchList.SelectedValue;

        this.Model.TabModuleSettings["DisablePaging"] = this.disablePager.Checked.ToString(CultureInfo.InvariantCulture);
        this.Model.TabModuleSettings["PageSize"] = this.pageSize.Text;

        this.Model.TabModuleSettings["ExcludeHostUsers"] = this.ExcludeHostUsersCheckBox.Checked.ToString(CultureInfo.InvariantCulture);

        base.OnSavingSettings();
    }

    private ListItemCollection GetPropertiesCollection(string profileResourceFile)
    {
        var result = new ListItemCollection();
        foreach (var property in this.Model.ProfileProperties)
        {
            result.Add(new ListItem(this.GetLocalizeName(property.PropertyName, profileResourceFile), property.PropertyName));
        }

        return result;
    }

    private string GetLocalizeName(string propertyName, string resourceFile)
    {
        var name = Localization.GetString("ProfileProperties_" + propertyName, resourceFile);
        return string.IsNullOrEmpty(name) ? propertyName : name.Trim(':');
    }

    private void BindSortList()
    {
        this.sortFieldList.Items.Add(this.AddSearchItem("UserId"));
        this.sortFieldList.Items.Add(this.AddSearchItem("LastName"));
        this.sortFieldList.Items.Add(this.AddSearchItem("DisplayName"));
        this.sortFieldList.Items.Add(this.AddSearchItem("CreatedOnDate", "DateCreated"));
        var controller = new ListController();
        var imageDataType = controller.GetListEntryInfo("DataType", "Image");
        foreach (ProfilePropertyDefinition definition in this.Model.ProfileProperties)
        {
            if (imageDataType != null && definition.DataType != imageDataType.EntryID)
            {
                this.sortFieldList.Items.Add(this.AddSearchItem(definition.PropertyName));
            }
        }
    }

    private ListItem AddSearchItem(string name)
    {
        return this.AddSearchItem(name, name);
    }

    private ListItem AddSearchItem(string name, string resourceKey)
    {
        var text = Localization.GetString(resourceKey, this.LocalResourceFile);
        if (string.IsNullOrEmpty(text))
        {
            text = resourceKey;
        }

        var item = new ListItem(text, name);
        return item;
    }
}
