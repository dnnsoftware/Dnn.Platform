// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.Skins.Controls
    /// Class:      SkinsEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinsEditControl control provides a standard UI component for editing
    /// skins.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:SkinsEditControl runat=server></{0}:SkinsEditControl>")]
    public class SkinsEditControl : EditControl, IPostBackEventHandler
    {
        private string _AddedItem = Null.NullString;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SkinsEditControl"/> class.
        /// Constructs a SkinsEditControl.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public SkinsEditControl()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SkinsEditControl"/> class.
        /// Constructs a SkinsEditControl.
        /// </summary>
        /// <param name="type">The type of the property.</param>
        /// -----------------------------------------------------------------------------
        public SkinsEditControl(string type)
        {
            this.SystemType = type;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets oldStringValue returns the String representation of the OldValue.
        /// </summary>
        /// <value>A String representing the OldValue.</value>
        /// -----------------------------------------------------------------------------
        protected string OldStringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (this.OldDictionaryValue != null)
                {
                    foreach (string Skin in this.OldDictionaryValue.Values)
                    {
                        strValue += Skin + ",";
                    }
                }

                return strValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets dictionaryValue returns the Dictionary(Of Integer, String) representation of the Value.
        /// </summary>
        /// <value>A Dictionary(Of Integer, String) representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected Dictionary<int, string> DictionaryValue
        {
            get
            {
                return this.Value as Dictionary<int, string>;
            }

            set
            {
                this.Value = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets oldDictionaryValue returns the Dictionary(Of Integer, String) representation of the OldValue.
        /// </summary>
        /// <value>A Dictionary(Of Integer, String) representing the OldValue.</value>
        /// -----------------------------------------------------------------------------
        protected Dictionary<int, string> OldDictionaryValue
        {
            get
            {
                return this.OldValue as Dictionary<int, string>;
            }

            set
            {
                this.OldValue = value;
            }
        }

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
                string strValue = Null.NullString;
                if (this.DictionaryValue != null)
                {
                    foreach (string Skin in this.DictionaryValue.Values)
                    {
                        strValue += Skin + ",";
                    }
                }

                return strValue;
            }

            set
            {
                this.Value = value;
            }
        }

        protected string AddedItem
        {
            get
            {
                return this._AddedItem;
            }

            set
            {
                this._AddedItem = value;
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            PropertyEditorEventArgs args;
            switch (eventArgument.Substring(0, 3))
            {
                case "Del":
                    args = new PropertyEditorEventArgs(this.Name);
                    args.Value = this.DictionaryValue;
                    args.OldValue = this.OldDictionaryValue;
                    args.Key = int.Parse(eventArgument.Substring(7));
                    args.Changed = true;
                    this.OnItemDeleted(args);
                    break;
                case "Add":
                    args = new PropertyEditorEventArgs(this.Name);
                    args.Value = this.AddedItem;
                    args.StringValue = this.AddedItem;
                    args.Changed = true;
                    this.OnItemAdded(args);
                    break;
            }
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string postedValue;
            var newDictionaryValue = new Dictionary<int, string>();
            foreach (KeyValuePair<int, string> kvp in this.DictionaryValue)
            {
                postedValue = postCollection[this.UniqueID + "_skin" + kvp.Key];
                if (kvp.Value.Equals(postedValue))
                {
                    newDictionaryValue[kvp.Key] = kvp.Value;
                }
                else
                {
                    newDictionaryValue[kvp.Key] = postedValue;
                    dataChanged = true;
                }
            }

            postedValue = postCollection[this.UniqueID + "_skinnew"];
            if (!string.IsNullOrEmpty(postedValue))
            {
                this.AddedItem = postedValue;
            }

            this.DictionaryValue = newDictionaryValue;
            return dataChanged;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.DictionaryValue;
            args.OldValue = this.OldDictionaryValue;
            args.StringValue = string.Empty;
            args.Changed = true;
            this.OnValueChanged(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs just before the control is due to be rendered.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Register control for PostBack
            this.Page.RegisterRequiresPostBack(this);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            int length = Null.NullInteger;
            if (this.CustomAttributes != null)
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    if (attribute is MaxLengthAttribute)
                    {
                        var lengthAtt = (MaxLengthAttribute)attribute;
                        length = lengthAtt.Length;
                        break;
                    }
                }
            }

            if (this.DictionaryValue != null)
            {
                foreach (KeyValuePair<int, string> kvp in this.DictionaryValue)
                {
                    // Render Hyperlink
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackClientHyperlink(this, "Delete_" + kvp.Key, false));
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:return confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteItem")) + "');");
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("cmdDelete", this.LocalResourceFile));
                    writer.RenderBeginTag(HtmlTextWriterTag.A);

                    // Render Image
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Delete"));
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);

                    // Render end of Image
                    writer.RenderEndTag();

                    // Render end of Hyperlink
                    writer.RenderEndTag();

                    this.ControlStyle.AddAttributesToRender(writer);
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, kvp.Value);
                    if (length > Null.NullInteger)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString());
                    }

                    writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_skin" + kvp.Key);
                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag();

                    writer.WriteBreak();
                }

                writer.WriteBreak();

                // Create Add Row
                // Render Hyperlink
                writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackClientHyperlink(this, "Add", false));
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("cmdAdd", this.LocalResourceFile));
                writer.RenderBeginTag(HtmlTextWriterTag.A);

                // Render Image
                writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Add"));
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);

                // Render end of Image
                writer.RenderEndTag();

                // Render end of Hyperlink
                writer.RenderEndTag();

                this.ControlStyle.AddAttributesToRender(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
                if (length > Null.NullInteger)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString());
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_skinnew");
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
                writer.WriteBreak();
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
            if (this.DictionaryValue != null)
            {
                foreach (KeyValuePair<int, string> kvp in this.DictionaryValue)
                {
                    this.ControlStyle.AddAttributesToRender(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(kvp.Value);
                    writer.RenderEndTag();
                    writer.WriteBreak();
                }
            }
        }
    }
}
