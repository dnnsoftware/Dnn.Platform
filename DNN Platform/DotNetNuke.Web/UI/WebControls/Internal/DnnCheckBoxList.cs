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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls.Extensions;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnCheckBoxList : CheckBoxList
    {
        protected override void OnInit(EventArgs e)
        {
            RepeatColumns = 1;
            base.OnInit(e);
        }

        private string _initValue;
        public override string SelectedValue
        {
            get
            {
                return base.SelectedValue;

            }
            set
            {
                if (this.RequiresDataBinding)
                {
                    _initValue = value;
                }
                else
                {
                    base.SelectedValue = value;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Utilities.ApplySkin(this);
            RegisterRequestResources();

            base.OnPreRender(e);
        }

        public override void DataBind()
        {
            if (!string.IsNullOrEmpty(_initValue))
            {
                DataBind(_initValue);
            }
            else
            {
                base.DataBind();
            }
        }

        public void AddItem(string text, string value)
        {
            Items.Add(new ListItem(text, value));
        }

        public void InsertItem(int index, string text, string value)
        {
            Items.Insert(index, new ListItem(text, value));
        }

        public void DataBind(string initialValue)
        {
            DataBind(initialValue, false);
        }

        public void DataBind(string initial, bool findByText)
        {
            base.DataBind();

            Select(initial, findByText);
        }

        public void Select(string initial, bool findByText)
        {
            if (findByText)
            {
                if (FindItemByText(initial, true) != null)
                {
                    FindItemByText(initial, true).Selected = true;
                }
            }
            else
            {
                if (FindItemByValue(initial, true) != null)
                {
                    FindItemByValue(initial, true).Selected = true;
                }
            }
        }

        public ListItem FindItemByText(string text, bool ignoreCase = false)
        {
            return ignoreCase ? Items.FindByText(text) : Items.FindByTextWithIgnoreCase(text);
        }

        public ListItem FindItemByValue(string value, bool ignoreCase = false)
        {
            return ignoreCase ? Items.FindByValue(value) : Items.FindByValueWithIgnoreCase(value);
        }

        public int FindItemIndexByValue(string value)
        {
            return Items.IndexOf(FindItemByValue(value));
        }

        private void RegisterRequestResources()
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            if (Globals.Status == Globals.UpgradeStatus.None)
            {
                var package = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == "Selectize");
                if (package != null)
                {
                    JavaScript.RequestRegistration("Selectize");

                    var libraryPath =
                        $"~/Resources/Libraries/{package.LibraryName}/{Globals.FormatVersion(package.Version, "00", 3, "_")}/";
                    ClientResourceManager.RegisterStyleSheet(Page, $"{libraryPath}selectize.css");
                    ClientResourceManager.RegisterStyleSheet(Page, $"{libraryPath}selectize.default.css");

                    var initScripts = $"$('#{ClientID}').selectize({{}});";

                    Page.ClientScript.RegisterStartupScript(Page.GetType(), $"{ClientID}Sctipts", initScripts, true);
                }
            }
        }
    }
}