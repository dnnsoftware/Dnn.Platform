#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using DotNetNuke.Modules.MemberDirectory.Presenters;
using DotNetNuke.Modules.MemberDirectory.ViewModels;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls.Extensions;

using WebFormsMvp;

#endregion

namespace DotNetNuke.Modules.MemberDirectory
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    [PresenterBinding(typeof(ModuleSettingsPresenter))]
    public partial class Settings : SettingsView<MemberDirectorySettingsModel>
    {
        private static string templatePath = "~/DesktopModules/MemberDirectory/Templates/";

        private string _defaultSearchField1 = "DisplayName";
        private string _defaultSearchField2 = "Email";
        private string _defaultSearchField3 = "City";
        private string _defaultSearchField4 = "Country";

        private string _defaultSortField = "DisplayName";
        private string _defaultSortOrder = "ASC";

        private string _defaultFilterBy = "None";
        private string _defaultFilterValue = String.Empty;

        private string _defaultDisplaySearch = "Both";
        private string _defaultEnablePopUp = "false";

        private string _filterBy;
        private string _filterValue;

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

        public const int DefaultPageSize = 20;

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

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            AutoDataBind = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!IsPostBack)
            {
                if (Model.Groups.Count > 0)
                {
                    groupList.DataSource = Model.Groups;
                    groupList.DataBind();
					groupList.Items.Insert(0, new ListItem(Localization.GetString("None_Specified"), Null.NullInteger.ToString()));
                }
                else
                {
                    filterBySelector.Items.FindByValue("Group").Enabled = false;
                }

                foreach (var rel in Model.Relationships)
                {
                    relationShipList.AddItem(Localization.GetString(rel.Name,Localization.SharedResourceFile),rel.RelationshipId.ToString());
                }


                var profileResourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";

                
                System.Web.UI.WebControls.ListItemCollection propertiesCollection = GetPropertiesCollection(profileResourceFile);
                    
                
                //Bind the ListItemCollection to the list
                propertyList.DataSource = propertiesCollection;
                propertyList.DataBind();

                //Insert custom properties to the Search field lists
                propertiesCollection.Insert(0,new ListItem(Localization.GetString("Username",LocalResourceFile),"Username"));
                propertiesCollection.Insert(1, new ListItem(Localization.GetString("DisplayName", LocalResourceFile), "DisplayName"));
                propertiesCollection.Insert(2, new ListItem(Localization.GetString("Email", LocalResourceFile), "Email"));

                //Bind the properties collection in the Search Field Lists

                searchField1List.DataSource = propertiesCollection;
                searchField1List.DataBind();

                searchField2List.DataSource = propertiesCollection;
                searchField2List.DataBind();

                searchField3List.DataSource = propertiesCollection;
                searchField3List.DataBind();

                searchField4List.DataSource = propertiesCollection;
                searchField4List.DataBind();

                filterBySelector.Select(_filterBy, false, 0);

                switch (_filterBy)
                {
                    case "Group":
                        groupList.Select(_filterValue, false, 0);
                        break;
                    case "Relationship":
                        relationShipList.Select(_filterValue, false, 0);
                        break;
                    case "ProfileProperty":
                        propertyList.Select(_filterValue, false, 0);
                        break;
                    case "User":
                        break;
                }

                searchField1List.Select(GetTabModuleSetting("SearchField1", _defaultSearchField1));
                searchField2List.Select(GetTabModuleSetting("SearchField2", _defaultSearchField2));
                searchField3List.Select(GetTabModuleSetting("SearchField3", _defaultSearchField3));
                searchField4List.Select(GetTabModuleSetting("SearchField4", _defaultSearchField4));

                ExcludeHostUsersCheckBox.Checked = Boolean.Parse(GetTabModuleSetting("ExcludeHostUsers", "false"));
            }
        }

        private ListItemCollection GetPropertiesCollection(string profileResourceFile)
        {
            var result = new ListItemCollection();
            foreach (var property in Model.ProfileProperties)
            {
                result.Add(new ListItem(GetLocalizeName(property.PropertyName,profileResourceFile),property.PropertyName));
            }
            
            return result;
        }

        protected override void OnSettingsLoaded()
        {
            base.OnSettingsLoaded();

            if(!IsPostBack)
            {
                BindSortList();

                itemTemplate.Text = GetTabModuleSetting("ItemTemplate", DefaultItemTemplate);
                alternateItemTemplate.Text = GetTabModuleSetting("AlternateItemTemplate", DefaultAlternateItemTemplate);
                popUpTemplate.Text = GetTabModuleSetting("PopUpTemplate", DefaultPopUpTemplate);
                displaySearchList.Select(GetTabModuleSetting("DisplaySearch", _defaultDisplaySearch));
                enablePopUp.Checked = Boolean.Parse(GetTabModuleSetting("EnablePopUp", _defaultEnablePopUp));

                _filterBy = GetModuleSetting("FilterBy", _defaultFilterBy);
                _filterValue = GetModuleSetting("FilterValue", _defaultFilterValue);
                propertyValue.Text = GetModuleSetting("FilterPropertyValue", String.Empty);

                sortFieldList.Select(GetTabModuleSetting("SortField", _defaultSortField));
                sortOrderList.Select(GetTabModuleSetting("SortOrder", _defaultSortOrder));

                pageSize.Text = GetTabModuleSetting("PageSize", DefaultPageSize.ToString(CultureInfo.InvariantCulture));
                disablePager.Checked = Boolean.Parse(GetTabModuleSetting("DisablePaging", "False"));
            }
        }

        protected override void OnSavingSettings()
        {
            Model.TabModuleSettings["ItemTemplate"] = itemTemplate.Text;
            Model.TabModuleSettings["AlternateItemTemplate"] = alternateItemTemplate.Text;
            Model.TabModuleSettings["PopUpTemplate"] = popUpTemplate.Text;
            Model.TabModuleSettings["EnablePopUp"] = enablePopUp.Checked.ToString(CultureInfo.InvariantCulture);

            _filterBy = filterBySelector.SelectedValue;
            Model.ModuleSettings["FilterBy"] = _filterBy;

            switch (_filterBy)
            {
                case "Group":
                    Model.ModuleSettings["FilterValue"] = groupList.SelectedValue;
                    break;
                case "Relationship":
                    Model.ModuleSettings["FilterValue"] = relationShipList.SelectedValue;
                    break;
                case "ProfileProperty":
                    Model.ModuleSettings["FilterValue"] = propertyList.SelectedValue;
                    break;
            }

            Model.ModuleSettings["FilterPropertyValue"] = propertyValue.Text;

            Model.TabModuleSettings["SortField"] = sortFieldList.SelectedValue;
            Model.TabModuleSettings["SortOrder"] = sortOrderList.SelectedValue;

            Model.TabModuleSettings["SearchField1"] = searchField1List.SelectedValue;
            Model.TabModuleSettings["SearchField2"] = searchField2List.SelectedValue;
            Model.TabModuleSettings["SearchField3"] = searchField3List.SelectedValue;
            Model.TabModuleSettings["SearchField4"] = searchField4List.SelectedValue;
            Model.TabModuleSettings["DisplaySearch"] = displaySearchList.SelectedValue;

            Model.TabModuleSettings["DisablePaging"] = disablePager.Checked.ToString(CultureInfo.InvariantCulture);
            Model.TabModuleSettings["PageSize"] = pageSize.Text;
            
            Model.TabModuleSettings["ExcludeHostUsers"] = ExcludeHostUsersCheckBox.Checked.ToString(CultureInfo.InvariantCulture);

            base.OnSavingSettings();
        }

        private string GetLocalizeName(string propertyName, string resourceFile)
        {
            var name = Localization.GetString("ProfileProperties_" + propertyName, resourceFile);
            return string.IsNullOrEmpty(name) ? propertyName : name.Trim(':');
        }

        private void BindSortList()
        {
            sortFieldList.Items.Add(AddSearchItem("UserId"));
            sortFieldList.Items.Add(AddSearchItem("LastName"));
            sortFieldList.Items.Add(AddSearchItem("DisplayName"));
            sortFieldList.Items.Add(AddSearchItem("CreatedOnDate", "DateCreated"));
            var controller = new ListController();
            var imageDataType = controller.GetListEntryInfo("DataType", "Image");
            foreach (ProfilePropertyDefinition definition in Model.ProfileProperties)
            {
                if (imageDataType != null && definition.DataType != imageDataType.EntryID)
                {
                    sortFieldList.Items.Add(AddSearchItem(definition.PropertyName));
                }
            }
        }

        private ListItem AddSearchItem(string name)
        {
            return AddSearchItem(name, name);
        }

        private ListItem AddSearchItem(string name, string resourceKey)
        {
            var text = Localization.GetString(resourceKey, LocalResourceFile);
            if (String.IsNullOrEmpty(text))
            {
                text = resourceKey;
            }
            var item = new ListItem(text, name);
            return item;
        }
    }
}