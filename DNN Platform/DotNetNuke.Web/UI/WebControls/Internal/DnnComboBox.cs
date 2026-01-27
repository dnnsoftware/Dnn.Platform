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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    public class DnnComboBox : DropDownList
    {
        private string initValue;
        private string multipleValue;

        /// <summary>Initializes a new instance of the <see cref="DnnComboBox"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnComboBox()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnComboBox"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnComboBox(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
        {
            this.AppStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.EventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.ClientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        }

        /// <inheritdoc />
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
                    this.initValue = value;
                }

                if (this.Items.Cast<ListItem>().Any(i => i.Value == value))
                {
                    base.SelectedValue = value;
                }
            }
        }

        /// <summary>Gets or sets a value indicating whether to show checkboxes.</summary>
        public virtual bool CheckBoxes { get; set; }

        /// <summary>Gets or sets a value indicating whether to allow multiple selections.</summary>
        public virtual bool MultipleSelect { get; set; }

        /// <summary>Gets or sets the client code to run when the selected index changes.</summary>
        public virtual string OnClientSelectedIndexChanged { get; set; }

        /// <summary>Gets or sets the value.</summary>
        public string Value
        {
            get
            {
                if (this.TagKey == HtmlTextWriterTag.Input)
                {
                    return this.multipleValue ?? string.Empty;
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

        /// <summary>Gets or sets the options.</summary>
        public DnnComboBoxOption Options { get; set; } = new DnnComboBoxOption();

        /// <summary>Gets the application status.</summary>
        protected IApplicationStatusInfo AppStatus { get; }

        /// <summary>Gets the event logger.</summary>
        protected IEventLogger EventLogger { get; }

        /// <summary>Gets the client resource controller.</summary>
        protected IClientResourceController ClientResourceController { get; }

        /// <inheritdoc />
        protected override HtmlTextWriterTag TagKey => this.MultipleSelect || this.CheckBoxes ? HtmlTextWriterTag.Input : HtmlTextWriterTag.Select;

        /// <inheritdoc />
        public override void DataBind()
        {
            if (!string.IsNullOrEmpty(this.initValue))
            {
                this.DataBind(this.initValue);
            }
            else
            {
                base.DataBind();
            }
        }

        /// <summary>Adds an item to the list.</summary>
        /// <param name="text">The item text.</param>
        /// <param name="value">The item value.</param>
        public void AddItem(string text, string value)
        {
            this.Items.Add(new ListItem(text, value));
        }

        /// <summary>Inserts an item into the list.</summary>
        /// <param name="index">The location in the collection to insert the item.</param>
        /// <param name="text">The item text.</param>
        /// <param name="value">The item value.</param>
        public void InsertItem(int index, string text, string value)
        {
            this.Items.Insert(index, new ListItem(text, value));
        }

        /// <summary>Binds a data source to the invoked server control and all its child controls.</summary>
        /// <param name="initialValue">The initial value.</param>
        public void DataBind(string initialValue)
        {
            this.DataBind(initialValue, false);
        }

        /// <summary>Binds a data source to the invoked server control and all its child controls.</summary>
        /// <param name="initial">The initial value or text.</param>
        /// <param name="findByText">Whether <paramref name="initial"/> is the text or value.</param>
        public void DataBind(string initial, bool findByText)
        {
            base.DataBind();

            this.Select(initial, findByText);
        }

        /// <summary>Selects an item.</summary>
        /// <param name="initial">The item's value or text.</param>
        /// <param name="findByText">Whether <paramref name="initial"/> is the text or value.</param>
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

        /// <summary>Finds an item by its text.</summary>
        /// <param name="text">The item's text.</param>
        /// <param name="ignoreCase">Whether to do a case-insensitive search.</param>
        /// <returns>The list item or <see langword="null"/>.</returns>
        public ListItem FindItemByText(string text, bool ignoreCase = false)
        {
            return ignoreCase ? this.Items.FindByText(text) : this.Items.FindByTextWithIgnoreCase(text);
        }

        /// <summary>Finds an item by its value.</summary>
        /// <param name="value">The item's value.</param>
        /// <param name="ignoreCase">Whether to do a case-insensitive search.</param>
        /// <returns>The list item or <see langword="null"/>.</returns>
        public ListItem FindItemByValue(string value, bool ignoreCase = false)
        {
            return ignoreCase ? this.Items.FindByValue(value) : this.Items.FindByValueWithIgnoreCase(value);
        }

        /// <summary>Finds an item's index by its value.</summary>
        /// <param name="value">The item's value.</param>
        /// <returns>The index or <c>-1</c>.</returns>
        public int FindItemIndexByValue(string value)
        {
            return this.Items.IndexOf(this.FindItemByValue(value));
        }

        /// <inheritdoc />
        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var postData = postCollection[postDataKey];
            if (!string.IsNullOrEmpty(postData))
            {
                this.multipleValue = postData;
            }

            return base.LoadPostData(postDataKey, postCollection);
        }

        /// <inheritdoc />
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.TagKey == HtmlTextWriterTag.Select)
            {
                base.RenderContents(writer);
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            Utilities.ApplyControlSkin(this.ClientResourceController, this, string.Empty, string.Empty);

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
            JavaScript.RequestRegistration(this.AppStatus, this.EventLogger, PortalSettings.Current, CommonJs.DnnPlugins);

            if (this.AppStatus.Status == UpgradeStatus.None)
            {
                var package = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == "Selectize");
                if (package != null)
                {
                    JavaScript.RequestRegistration(this.AppStatus, this.EventLogger, PortalSettings.Current, "Selectize");

                    var libraryPath =
                        $"~/Resources/Libraries/{package.LibraryName}/{Globals.FormatVersion(package.Version, "00", 3, "_")}/";

                    this.ClientResourceController.RegisterScript($"{libraryPath}dnn.combobox.js");
                    this.ClientResourceController.RegisterStylesheet($"{libraryPath}selectize.css");
                    this.ClientResourceController.RegisterStylesheet($"{libraryPath}selectize.default.css");

                    var options = JsonConvert.SerializeObject(this.Options, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, });

                    var initScripts = $"$('#{this.ClientID}').dnnComboBox({options});";

                    ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), $"{this.ClientID}Sctipts", initScripts, true);
                }
            }
        }
    }
}
