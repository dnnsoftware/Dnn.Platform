﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

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

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnComboBox : DropDownList
    {
        #region Fields

        private string _initValue;
        private string _multipleValue;

        #endregion

        #region Properties

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

                if (Items.Cast<ListItem>().Any(i => i.Value == value))
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
                if (TagKey == HtmlTextWriterTag.Input)
                {
                    return _multipleValue ?? string.Empty;
                }

                return SelectedValue ?? string.Empty;
            }
            set
            {
                if (TagKey == HtmlTextWriterTag.Input)
                {
                    Attributes.Remove("value");
                    Attributes.Add("value", value);
                }

                SelectedValue = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return MultipleSelect || CheckBoxes ? HtmlTextWriterTag.Input : HtmlTextWriterTag.Select;
            }
        }

        public DnnComboBoxOption Options { get; set; } = new DnnComboBoxOption();

        #endregion

        #region Override Methods

        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var postData = postCollection[postDataKey];
            if (!string.IsNullOrEmpty(postData))
            {
                _multipleValue = postData;
            }

            return base.LoadPostData(postDataKey, postCollection);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (TagKey == HtmlTextWriterTag.Select)
            {
                base.RenderContents(writer);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Utilities.ApplySkin(this);

            if (TagKey == HtmlTextWriterTag.Input)
            {
                Options.Items = Items.Cast<ListItem>();
                Value = string.Join(",", Options.Items.Where(i => i.Selected).Select(i => i.Value));
            }
            else
            {
                if (Items.Cast<ListItem>().Any(i => string.IsNullOrEmpty(i.Value)))
                {
                    Options.AllowEmptyOption = true;
                }
            }

            if (!Options.Localization.ContainsKey("ItemsChecked"))
            {
                Options.Localization.Add("ItemsChecked", Utilities.GetLocalizedString("ItemsCheckedString"));
            }
            if (!Options.Localization.ContainsKey("AllItemsChecked"))
            {
                Options.Localization.Add("AllItemsChecked", Utilities.GetLocalizedString("AllItemsCheckedString"));
            }

            Options.Checkbox = CheckBoxes;
            Options.OnChangeEvent = OnClientSelectedIndexChanged;

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

        #endregion

        #region Public Methods

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

        #endregion

        #region Private Methods

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

                    ClientResourceManager.RegisterScript(Page, $"{libraryPath}dnn.combobox.js");
                    ClientResourceManager.RegisterStyleSheet(Page, $"{libraryPath}selectize.css");
                    ClientResourceManager.RegisterStyleSheet(Page, $"{libraryPath}selectize.default.css");

                    var options = JsonConvert.SerializeObject(Options, Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore
                                    });

                    var initScripts = $"$('#{ClientID}').dnnComboBox({options});";

                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), $"{ClientID}Sctipts", initScripts, true);
                }
            }
        }

        #endregion
    }
}
