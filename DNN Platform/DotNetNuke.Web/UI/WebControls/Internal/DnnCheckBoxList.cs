// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    /// <remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnCheckBoxList : CheckBoxList
    {
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
                    this._initValue = value;
                }
                else
                {
                    base.SelectedValue = value;
                }
            }
        }

        public override void DataBind()
        {
            if (!string.IsNullOrEmpty(this._initValue))
            {
                this.DataBind(this._initValue);
            }
            else
            {
                base.DataBind();
            }
        }

        public void AddItem(string text, string value)
        {
            this.Items.Add(new ListItem(text, value));
        }

        public void InsertItem(int index, string text, string value)
        {
            this.Items.Insert(index, new ListItem(text, value));
        }

        public void DataBind(string initialValue)
        {
            this.DataBind(initialValue, false);
        }

        public void DataBind(string initial, bool findByText)
        {
            base.DataBind();

            this.Select(initial, findByText);
        }

        public void Select(string initial, bool findByText)
        {
            if (findByText)
            {
                if (this.FindItemByText(initial, true) != null)
                {
                    this.FindItemByText(initial, true).Selected = true;
                }
            }
            else
            {
                if (this.FindItemByValue(initial, true) != null)
                {
                    this.FindItemByValue(initial, true).Selected = true;
                }
            }
        }

        public ListItem FindItemByText(string text, bool ignoreCase = false)
        {
            return ignoreCase ? this.Items.FindByText(text) : this.Items.FindByTextWithIgnoreCase(text);
        }

        public ListItem FindItemByValue(string value, bool ignoreCase = false)
        {
            return ignoreCase ? this.Items.FindByValue(value) : this.Items.FindByValueWithIgnoreCase(value);
        }

        public int FindItemIndexByValue(string value)
        {
            return this.Items.IndexOf(this.FindItemByValue(value));
        }

        protected override void OnInit(EventArgs e)
        {
            this.RepeatColumns = 1;
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            Utilities.ApplySkin(this);
            this.RegisterRequestResources();

            base.OnPreRender(e);
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
                    ClientResourceManager.RegisterStyleSheet(this.Page, $"{libraryPath}selectize.css");
                    ClientResourceManager.RegisterStyleSheet(this.Page, $"{libraryPath}selectize.default.css");

                    var initScripts = $"$('#{this.ClientID}').selectize({{}});";

                    this.Page.ClientScript.RegisterStartupScript(this.Page.GetType(), $"{this.ClientID}Sctipts", initScripts, true);
                }
            }
        }
    }
}
