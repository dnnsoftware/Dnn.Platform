// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls.Extensions;
    using Newtonsoft.Json;

    /// <remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnComboBox : DropDownList
    {
        private string _initValue;
        private string _multipleValue;

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

                if (this.Items.Cast<ListItem>().Any(i => i.Value == value))
                {
                    base.SelectedValue = value;
                }
            }
        }

        public virtual bool CheckBoxes { get; set; } = false;

        public virtual bool MultipleSelect { get; set; } = false;

        public virtual string OnClientSelectedIndexChanged { get; set; }

        public string Value
        {
            get
            {
                if (this.TagKey == HtmlTextWriterTag.Input)
                {
                    return this._multipleValue ?? string.Empty;
                }

                return this.SelectedValue ?? string.Empty;
            }

            set
            {
                if (this.TagKey == HtmlTextWriterTag.Input)
                {
                    this.Attributes.Remove("value");
                    this.Attributes.Add("value", value);
                }

                this.SelectedValue = value;
            }
        }

        public DnnComboBoxOption Options { get; set; } = new DnnComboBoxOption();

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return this.MultipleSelect || this.CheckBoxes ? HtmlTextWriterTag.Input : HtmlTextWriterTag.Select;
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

        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var postData = postCollection[postDataKey];
            if (!string.IsNullOrEmpty(postData))
            {
                this._multipleValue = postData;
            }

            return base.LoadPostData(postDataKey, postCollection);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.TagKey == HtmlTextWriterTag.Select)
            {
                base.RenderContents(writer);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Utilities.ApplySkin(this);

            if (this.TagKey == HtmlTextWriterTag.Input)
            {
                this.Options.Items = this.Items.Cast<ListItem>();
                this.Value = string.Join(",", this.Options.Items.Where(i => i.Selected).Select(i => i.Value));
            }
            else
            {
                if (this.Items.Cast<ListItem>().Any(i => string.IsNullOrEmpty(i.Value)))
                {
                    this.Options.AllowEmptyOption = true;
                }
            }

            if (!this.Options.Localization.ContainsKey("ItemsChecked"))
            {
                this.Options.Localization.Add("ItemsChecked", Utilities.GetLocalizedString("ItemsCheckedString"));
            }

            if (!this.Options.Localization.ContainsKey("AllItemsChecked"))
            {
                this.Options.Localization.Add("AllItemsChecked", Utilities.GetLocalizedString("AllItemsCheckedString"));
            }

            this.Options.Checkbox = this.CheckBoxes;
            this.Options.OnChangeEvent = this.OnClientSelectedIndexChanged;

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

                    ClientResourceManager.RegisterScript(this.Page, $"{libraryPath}dnn.combobox.js");
                    ClientResourceManager.RegisterStyleSheet(this.Page, $"{libraryPath}selectize.css");
                    ClientResourceManager.RegisterStyleSheet(this.Page, $"{libraryPath}selectize.default.css");

                    var options = JsonConvert.SerializeObject(this.Options, Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                    });

                    var initScripts = $"$('#{this.ClientID}').dnnComboBox({options});";

                    ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), $"{this.ClientID}Sctipts", initScripts, true);
                }
            }
        }
    }
}
