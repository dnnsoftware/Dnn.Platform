// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNListEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNListEditControl control provides a standard UI component for selecting
    /// from Lists.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DNNListEditControl runat=server></{0}:DNNListEditControl>")]
    public class DNNListEditControl : EditControl, IPostBackEventHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNListEditControl));
        private List<ListEntryInfo> _listEntries;
        private string _listName = string.Empty;

        public DNNListEditControl()
        {
            this.ValueField = ListBoundField.Value;
            this.TextField = ListBoundField.Text;
            this.ParentKey = string.Empty;
            this.SortAlphabetically = false;
        }

        public event PropertyChangedEventHandler ItemChanged;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets integerValue returns the Integer representation of the Value.
        /// </summary>
        /// <value>An integer representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected int IntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                if (this.Value == null || string.IsNullOrEmpty((string)this.Value))
                {
                    return intValue;
                }

                try
                {
                    intValue = Convert.ToInt32(this.Value);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return intValue;
            }
        }

        /// <summary>
        /// Gets list gets the List associated with the control.
        /// </summary>
        [Obsolete("Obsoleted in 6.0.1 use ListEntries instead. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected ListEntryInfoCollection List
        {
            get
            {
                var listController = new ListController();
                return listController.GetListEntryInfoCollection(this.ListName, this.ParentKey, this.PortalId);
            }
        }

        /// <summary>
        /// Gets the ListEntryInfo objects associated witht the control.
        /// </summary>
        protected IEnumerable<ListEntryInfo> ListEntries
        {
            get
            {
                if (this._listEntries == null)
                {
                    var listController = new ListController();
                    if (this.SortAlphabetically)
                    {
                        this._listEntries = listController.GetListEntryInfoItems(this.ListName, this.ParentKey, this.PortalId).OrderBy(s => s.SortOrder).ThenBy(s => s.Text).ToList();
                    }
                    else
                    {
                        this._listEntries = listController.GetListEntryInfoItems(this.ListName, this.ParentKey, this.PortalId).ToList();
                    }
                }

                return this._listEntries;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets oldIntegerValue returns the Integer representation of the OldValue.
        /// </summary>
        /// <value>An integer representing the OldValue.</value>
        /// -----------------------------------------------------------------------------
        protected int OldIntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                if (this.OldValue == null || string.IsNullOrEmpty(this.OldValue.ToString()))
                {
                    return intValue;
                }

                try
                {
                    // Try and cast the value to an Integer
                    intValue = Convert.ToInt32(this.OldValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return intValue;
            }
        }

        protected int PortalId
        {
            get
            {
                return PortalController.GetEffectivePortalId(PortalSettings.Current.PortalId);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets oldStringValue returns the Boolean representation of the OldValue.
        /// </summary>
        /// <value>A String representing the OldValue.</value>
        /// -----------------------------------------------------------------------------
        protected string OldStringValue
        {
            get
            {
                return Convert.ToString(this.OldValue);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether determines whether the List Auto Posts Back.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected bool AutoPostBack { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether if true the list will be sorted on the value of Text before rendering.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected bool SortAlphabetically { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets listName is the name of the List to display.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual string ListName
        {
            get
            {
                if (this._listName == Null.NullString)
                {
                    this._listName = this.DataField;
                }

                return this._listName;
            }

            set
            {
                this._listName = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent key of the List to display.
        /// </summary>
        protected virtual string ParentKey { get; set; }

        /// <summary>
        /// Gets or sets the field to display in the combo.
        /// </summary>
        protected virtual ListBoundField TextField { get; set; }

        /// <summary>
        /// Gets or sets the field to use as the combo item values.
        /// </summary>
        protected virtual ListBoundField ValueField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets stringValue is the value of the control expressed as a String.
        /// </summary>
        /// <value>A string representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                return Convert.ToString(this.Value);
            }

            set
            {
                if (this.ValueField == ListBoundField.Id)
                {
                    // Integer type field
                    this.Value = int.Parse(value);
                }
                else
                {
                    // String type Field
                    this.Value = value;
                }
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            if (this.AutoPostBack)
            {
                this.OnItemChanged(this.GetEventArgs());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAttributesChanged runs when the CustomAttributes property has changed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnAttributesChanged()
        {
            // Get the List settings out of the "Attributes"
            if (this.CustomAttributes != null)
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    if (attribute is ListAttribute)
                    {
                        var listAtt = (ListAttribute)attribute;
                        this.ListName = listAtt.ListName;
                        this.ParentKey = listAtt.ParentKey;
                        this.TextField = listAtt.TextField;
                        this.ValueField = listAtt.ValueField;
                        break;
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            this.OnValueChanged(this.GetEventArgs());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnItemChanged runs when the Item has changed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemChanged(PropertyEditorEventArgs e)
        {
            if (this.ItemChanged != null)
            {
                this.ItemChanged(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            var objListController = new ListController();
            ListEntryInfo entry = null;
            string entryText = Null.NullString;
            switch (this.ValueField)
            {
                case ListBoundField.Id:
                    entry = objListController.GetListEntryInfo(this.ListName, Convert.ToInt32(this.Value));
                    break;
                case ListBoundField.Text:
                    entryText = this.StringValue;
                    break;
                case ListBoundField.Value:
                    entry = objListController.GetListEntryInfo(this.ListName, this.StringValue);
                    break;
            }

            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (entry != null)
            {
                switch (this.TextField)
                {
                    case ListBoundField.Id:
                        writer.Write(entry.EntryID.ToString());
                        break;
                    case ListBoundField.Text:
                        writer.Write(entry.Text);
                        break;
                    case ListBoundField.Value:
                        writer.Write(entry.Value);
                        break;
                }
            }
            else
            {
                writer.Write(entryText);
            }

            // Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            // Render the Select Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute("data-name", this.Name);
            writer.AddAttribute("data-list", this.ListName);
            writer.AddAttribute("data-category", this.Category);
            writer.AddAttribute("data-editor", "DNNListEditControl");
            if (this.AutoPostBack)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, this.Page.ClientScript.GetPostBackEventReference(this, this.ID));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            // Add the Not Specified Option
            if (this.ValueField == ListBoundField.Text)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
            }

            if (this.StringValue == Null.NullString)
            {
                // Add the Selected Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }

            var defaultText = HttpUtility.HtmlEncode("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">");
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write(defaultText);
            writer.RenderEndTag();

            foreach (ListEntryInfo item in this.ListEntries)
            {
                string itemValue = Null.NullString;

                // Add the Value Attribute
                switch (this.ValueField)
                {
                    case ListBoundField.Id:
                        itemValue = item.EntryID.ToString();
                        break;
                    case ListBoundField.Text:
                        itemValue = item.Text;
                        break;
                    case ListBoundField.Value:
                        itemValue = item.Value;
                        break;
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Value, itemValue);
                if (this.StringValue == itemValue)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                switch (this.TextField)
                {
                    case ListBoundField.Id:
                        writer.Write(item.EntryID.ToString());
                        break;
                    case ListBoundField.Text:
                        writer.Write(item.Text);
                        break;
                    case ListBoundField.Value:
                        writer.Write(item.Value.Trim());
                        break;
                }

                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
        }

        private PropertyEditorEventArgs GetEventArgs()
        {
            var args = new PropertyEditorEventArgs(this.Name);
            if (this.ValueField == ListBoundField.Id)
            {
                // This is an Integer Value
                args.Value = this.IntegerValue;
                args.OldValue = this.OldIntegerValue;
            }
            else
            {
                // This is a String Value
                args.Value = this.StringValue;
                args.OldValue = this.OldStringValue;
            }

            args.StringValue = this.StringValue;
            return args;
        }
    }
}
