#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Modules.ContentList
{

    public partial class ContentList : PortalModuleBase
    {
        private string _tagQuery = Null.NullString;

        private void BindData()
        {
            using (var dt = new DataTable())
            {
                dt.Columns.Add(new DataColumn("TabId", typeof (Int32)));
                dt.Columns.Add(new DataColumn("ContentKey", typeof (String)));
                dt.Columns.Add(new DataColumn("Title", typeof (String)));
                dt.Columns.Add(new DataColumn("Description", typeof (String)));
                dt.Columns.Add(new DataColumn("PubDate", typeof (DateTime)));

                var results = new ContentController().GetContentItemsByTerm(_tagQuery).ToList();
                var tabController = new TabController();

                if (_tagQuery.Length > 0)
                {
                    foreach (var item in results)
                    {
                        var dr = dt.NewRow();
                        dr["TabId"] = item.TabID;
                        dr["ContentKey"] = item.ContentKey;
                        dr["Title"] = item.Content;

                        //get tab info and use the tab description, if tab is deleted then ignore the item.
                        var tab = tabController.GetTab(item.TabID, PortalId, false);
                        if(tab != null)
                        {
							if (tab.IsDeleted)
							{
								continue;
							}

							dr["Title"] = string.IsNullOrEmpty(tab.Title) ? tab.TabName : tab.Title;
                            dr["Description"] = tab.Description;
                        }
                        else
                        {
                            dr["Description"] = item.Content.Length > 1000 ? item.Content.Substring(0, 1000) : item.Content;
                        }

                        dr["PubDate"] = item.CreatedOnDate;
                        dt.Rows.Add(dr);
                    }
                }

                //Bind Search Results Grid
                var dv = new DataView(dt);
                dgResults.DataSource = dv;
                dgResults.DataBind();
              
                if (results.Count == 0)
                {
                    dgResults.Visible = false;
                    lblMessage.Text = string.Format(Localization.GetString("NoResults", LocalResourceFile), _tagQuery);
                }
                else
                {
                    lblMessage.Text = string.Format(Localization.GetString("Results", LocalResourceFile), _tagQuery);
                }
            }
        }

        protected string FormatDate(DateTime pubDate)
        {
            return pubDate.ToString(CultureInfo.InvariantCulture);
        }

        protected string FormatURL(int tabID, string link)
        {
            var strURL = string.IsNullOrEmpty(link) ? Globals.NavigateURL(tabID) : Globals.NavigateURL(tabID, "", link);

            return strURL;
        }

        protected bool ShowDescription()
        {
            bool show = string.IsNullOrEmpty(Convert.ToString(Settings["showdescription"])) || Convert.ToString(Settings["showdescription"]) == "Y";

            return show;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var objSecurity = new PortalSecurity();
            if ((Request.Params["Tag"] != null))
            {
                _tagQuery = HttpContext.Current.Server.HtmlEncode(objSecurity.InputFilter(Request.Params["Tag"], PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoMarkup));
            }

            if (_tagQuery.Length > 0)
            {
//                if (!Page.IsPostBack)
//                {
                    BindData();
//                }
            }
            else
            {
                if (IsEditable)
                {
                   UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ModuleHidden", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    ContainerControl.Visible = false;
                }
            }
        }
    }
}