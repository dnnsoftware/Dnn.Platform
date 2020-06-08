// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
