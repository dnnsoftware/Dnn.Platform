// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Localization;

    public class DualListBox : WebControl, IPostBackEventHandler, IPostBackDataHandler
    {
        private readonly Style _AvailableListBoxStyle = new Style();
        private readonly Style _ButtonStyle = new Style();
        private readonly TableStyle _ContainerStyle = new TableStyle();
        private readonly Style _HeaderStyle = new Style();
        private readonly Style _SelectedListBoxStyle = new Style();
        private List<string> _AddValues;
        private List<string> _RemoveValues;

        public DualListBox()
        {
            this.ShowAddButton = true;
            this.ShowAddAllButton = true;
            this.ShowRemoveButton = true;
            this.ShowRemoveAllButton = true;
        }

        public event DualListBoxEventHandler AddButtonClick;

        public event EventHandler AddAllButtonClick;

        public event DualListBoxEventHandler RemoveButtonClick;

        public event EventHandler RemoveAllButtonClick;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Available List Box Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Available List Box.")]
        public Style AvailableListBoxStyle
        {
            get
            {
                return this._AvailableListBoxStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Button Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Button.")]
        public Style ButtonStyle
        {
            get
            {
                return this._ButtonStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Container Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Container.")]
        public TableStyle ContainerStyle
        {
            get
            {
                return this._ContainerStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Header Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Header.")]
        public Style HeaderStyle
        {
            get
            {
                return this._HeaderStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Selected List Box Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Selected List Box.")]
        public Style SelectedListBoxStyle
        {
            get
            {
                return this._SelectedListBoxStyle;
            }
        }

        public string AddAllImageURL { get; set; }

        public string AddAllKey { get; set; }

        public string AddAllText { get; set; }

        public string AddImageURL { get; set; }

        public string AddKey { get; set; }

        public string AddText { get; set; }

        public object AvailableDataSource { get; set; }

        public string DataTextField { private get; set; }

        public string DataValueField { private get; set; }

        public string LocalResourceFile { get; set; }

        public string RemoveAllImageURL { get; set; }

        public string RemoveAllKey { get; set; }

        public string RemoveAllText { get; set; }

        public string RemoveImageURL { get; set; }

        public string RemoveKey { get; set; }

        public string RemoveText { get; set; }

        public object SelectedDataSource { get; set; }

        public bool ShowAddButton { get; set; }

        public bool ShowAddAllButton { get; set; }

        public bool ShowRemoveButton { get; set; }

        public bool ShowRemoveAllButton { get; set; }

        public bool CausesValidation { get; set; }

        public string ValidationGroup { get; set; }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool retValue = Null.NullBoolean;
            string addItems = postCollection[postDataKey + "_Available"];
            if (!string.IsNullOrEmpty(addItems))
            {
                this._AddValues = new List<string>();
                foreach (string addItem in addItems.Split(','))
                {
                    this._AddValues.Add(addItem);
                }

                retValue = true;
            }

            string removeItems = postCollection[postDataKey + "_Selected"];
            if (!string.IsNullOrEmpty(removeItems))
            {
                this._RemoveValues = new List<string>();
                foreach (string removeItem in removeItems.Split(','))
                {
                    this._RemoveValues.Add(removeItem);
                }

                retValue = true;
            }

            return retValue;
        }

        public void RaisePostDataChangedEvent()
        {
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            switch (eventArgument)
            {
                case "Add":
                    this.OnAddButtonClick(new DualListBoxEventArgs(this._AddValues));
                    break;
                case "AddAll":
                    this.OnAddAllButtonClick(new EventArgs());
                    break;
                case "Remove":
                    this.OnRemoveButtonClick(new DualListBoxEventArgs(this._RemoveValues));
                    break;
                case "RemoveAll":
                    this.OnRemoveAllButtonClick(new EventArgs());
                    break;
            }
        }

        protected virtual PostBackOptions GetPostBackOptions(string argument)
        {
            var postBackOptions = new PostBackOptions(this, argument) { RequiresJavaScriptProtocol = true };

            if (this.CausesValidation && this.Page.GetValidators(this.ValidationGroup).Count > 0)
            {
                postBackOptions.PerformValidation = true;
                postBackOptions.ValidationGroup = this.ValidationGroup;
            }

            return postBackOptions;
        }

        protected void OnAddButtonClick(DualListBoxEventArgs e)
        {
            if (this.AddButtonClick != null)
            {
                this.AddButtonClick(this, e);
            }
        }

        protected void OnAddAllButtonClick(EventArgs e)
        {
            if (this.AddAllButtonClick != null)
            {
                this.AddAllButtonClick(this, e);
            }
        }

        protected void OnRemoveButtonClick(DualListBoxEventArgs e)
        {
            if (this.RemoveButtonClick != null)
            {
                this.RemoveButtonClick(this, e);
            }
        }

        protected void OnRemoveAllButtonClick(EventArgs e)
        {
            if (this.RemoveAllButtonClick != null)
            {
                this.RemoveAllButtonClick(this, e);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            // render table
            if (this.ContainerStyle != null)
            {
                this.ContainerStyle.AddAttributesToRender(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            // Render Header Row
            this.RenderHeader(writer);

            // Render ListBox row
            this.RenderListBoxes(writer);

            // Render end of table
            writer.RenderEndTag();
        }

        private NameValueCollection GetList(string listType, object dataSource)
        {
            var dataList = dataSource as IEnumerable;
            var list = new NameValueCollection();
            if (dataList == null)
            {
                throw new ArgumentException("The " + listType + "DataSource must implement the IEnumerable Interface");
            }
            else
            {
                foreach (object item in dataList)
                {
                    BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                    PropertyInfo objTextProperty = item.GetType().GetProperty(this.DataTextField, bindings);
                    PropertyInfo objValueProperty = item.GetType().GetProperty(this.DataValueField, bindings);
                    string objValue = Convert.ToString(objValueProperty.GetValue(item, null));
                    string objText = Convert.ToString(objTextProperty.GetValue(item, null));

                    list.Add(objText, objValue);
                }
            }

            return list;
        }

        private void RenderButton(string buttonType, HtmlTextWriter writer)
        {
            string buttonText = Null.NullString;
            string imageURL = Null.NullString;

            // Begin Button Row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // Begin Button Cell
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            switch (buttonType)
            {
                case "Add":
                    buttonText = string.IsNullOrEmpty(this.AddKey)
                                    ? this.AddText
                                    : Localization.GetString(this.AddKey, this.LocalResourceFile);
                    imageURL = this.AddImageURL;
                    break;
                case "AddAll":
                    buttonText = string.IsNullOrEmpty(this.AddAllKey)
                                    ? this.AddAllText
                                    : Localization.GetString(this.AddAllKey, this.LocalResourceFile);
                    imageURL = this.AddAllImageURL;
                    break;
                case "Remove":
                    buttonText = string.IsNullOrEmpty(this.RemoveKey)
                                    ? this.RemoveText
                                    : Localization.GetString(this.RemoveKey, this.LocalResourceFile);
                    imageURL = this.RemoveImageURL;
                    break;
                case "RemoveAll":
                    buttonText = string.IsNullOrEmpty(this.RemoveAllKey)
                                    ? this.RemoveAllText
                                    : Localization.GetString(this.RemoveAllKey, this.LocalResourceFile);
                    imageURL = this.RemoveAllImageURL;
                    break;
            }

            // Render Hyperlink
            writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackEventReference(this.GetPostBackOptions(buttonType)));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, buttonText);
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            // Render Image
            if (!string.IsNullOrEmpty(imageURL))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveClientUrl(imageURL));
                writer.AddAttribute(HtmlTextWriterAttribute.Title, buttonText);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }
            else
            {
                writer.Write(buttonText);
            }

            // End of Hyperlink
            writer.RenderEndTag();

            // End of Button Cell
            writer.RenderEndTag();

            // Render end of Button Row
            writer.RenderEndTag();
        }

        private void RenderButtons(HtmlTextWriter writer)
        {
            // render table
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            if (this.ShowAddButton)
            {
                this.RenderButton("Add", writer);
            }

            if (this.ShowAddAllButton)
            {
                this.RenderButton("AddAll", writer);
            }

            // Begin Button Row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // Begin Button Cell
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.Write("&nbsp;");

            // End of Button Cell
            writer.RenderEndTag();

            // Render end of Button Row
            writer.RenderEndTag();

            if (this.ShowRemoveButton)
            {
                this.RenderButton("Remove", writer);
            }

            if (this.ShowRemoveAllButton)
            {
                this.RenderButton("RemoveAll", writer);
            }

            // Render end of table
            writer.RenderEndTag();
        }

        private void RenderListBox(string listType, object dataSource, Style style, HtmlTextWriter writer)
        {
            if (dataSource != null)
            {
                NameValueCollection list = this.GetList(listType, dataSource);
                if (list != null)
                {
                    if (style != null)
                    {
                        style.AddAttributesToRender(writer);
                    }

                    // Render ListBox
                    writer.AddAttribute(HtmlTextWriterAttribute.Multiple, "multiple");
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_" + listType);
                    writer.RenderBeginTag(HtmlTextWriterTag.Select);
                    for (int i = 0; i <= list.Count - 1; i++)
                    {
                        // Render option tags for each item
                        writer.AddAttribute(HtmlTextWriterAttribute.Value, list.Get(i));
                        writer.RenderBeginTag(HtmlTextWriterTag.Option);
                        writer.Write(list.GetKey(i));
                        writer.RenderEndTag();
                    }

                    // Render ListBox end
                    writer.RenderEndTag();
                }
            }
        }

        private void RenderHeader(HtmlTextWriter writer)
        {
            // render Header row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (this.HeaderStyle != null)
            {
                this.HeaderStyle.AddAttributesToRender(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(Localization.GetString(this.ID + "_Available", this.LocalResourceFile));
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.RenderEndTag();
            if (this.HeaderStyle != null)
            {
                this.HeaderStyle.AddAttributesToRender(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(Localization.GetString(this.ID + "_Selected", this.LocalResourceFile));
            writer.RenderEndTag();

            // Render end of Header Row
            writer.RenderEndTag();
        }

        private void RenderListBoxes(HtmlTextWriter writer)
        {
            // render List Boxes row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderListBox("Available", this.AvailableDataSource, this.AvailableListBoxStyle, writer);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderButtons(writer);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderListBox("Selected", this.SelectedDataSource, this.SelectedListBoxStyle, writer);
            writer.RenderEndTag();

            // Render end of List Boxes Row
            writer.RenderEndTag();
        }
    }
}
