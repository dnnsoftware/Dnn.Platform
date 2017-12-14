#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
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
		
		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a SkinsEditControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public SkinsEditControl()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a SkinsEditControl
        /// </summary>
        /// <param name="type">The type of the property</param>
        /// -----------------------------------------------------------------------------
        public SkinsEditControl(string type)
        {
            SystemType = type;
        }
		
		#endregion

		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DictionaryValue returns the Dictionary(Of Integer, String) representation of the Value
        /// </summary>
        /// <value>A Dictionary(Of Integer, String) representing the Value</value>
        /// -----------------------------------------------------------------------------
        protected Dictionary<int, string> DictionaryValue
        {
            get
            {
                return Value as Dictionary<int, string>;
            }
            set
            {
                Value = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldDictionaryValue returns the Dictionary(Of Integer, String) representation of the OldValue
        /// </summary>
        /// <value>A Dictionary(Of Integer, String) representing the OldValue</value>
        /// -----------------------------------------------------------------------------
        protected Dictionary<int, string> OldDictionaryValue
        {
            get
            {
                return OldValue as Dictionary<int, string>;
            }
            set
            {
                OldValue = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldStringValue returns the String representation of the OldValue
        /// </summary>
        /// <value>A String representing the OldValue</value>
        /// -----------------------------------------------------------------------------
        protected string OldStringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (OldDictionaryValue != null)
                {
                    foreach (string Skin in OldDictionaryValue.Values)
                    {
                        strValue += Skin + ",";
                    }
                }
                return strValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StringValue is the value of the control expressed as a String
        /// </summary>
        /// <value>A string representing the Value</value>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (DictionaryValue != null)
                {
                    foreach (string Skin in DictionaryValue.Values)
                    {
                        strValue += Skin + ",";
                    }
                }
                return strValue;
            }
            set
            {
                Value = value;
            }
        }

        protected string AddedItem
        {
            get
            {
                return _AddedItem;
            }
            set
            {
                _AddedItem = value;
            }
        }

        #region IPostBackEventHandler Members

        public void RaisePostBackEvent(string eventArgument)
        {
            PropertyEditorEventArgs args;
            switch (eventArgument.Substring(0, 3))
            {
                case "Del":
                    args = new PropertyEditorEventArgs(Name);
                    args.Value = DictionaryValue;
                    args.OldValue = OldDictionaryValue;
                    args.Key = int.Parse(eventArgument.Substring(7));
                    args.Changed = true;
                    base.OnItemDeleted(args);
                    break;
                case "Add":
                    args = new PropertyEditorEventArgs(Name);
                    args.Value = AddedItem;
                    args.StringValue = AddedItem;
                    args.Changed = true;
                    base.OnItemAdded(args);
                    break;
            }
        }

        #endregion
		
		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(Name);
            args.Value = DictionaryValue;
            args.OldValue = OldDictionaryValue;
            args.StringValue = "";
            args.Changed = true;
            base.OnValueChanged(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs just before the control is due to be rendered
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //Register control for PostBack
            Page.RegisterRequiresPostBack(this);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            int length = Null.NullInteger;
            if ((CustomAttributes != null))
            {
                foreach (Attribute attribute in CustomAttributes)
                {
                    if (attribute is MaxLengthAttribute)
                    {
                        var lengthAtt = (MaxLengthAttribute) attribute;
                        length = lengthAtt.Length;
                        break;
                    }
                }
            }
            if (DictionaryValue != null)
            {
                foreach (KeyValuePair<int, string> kvp in DictionaryValue)
                {
					//Render Hyperlink
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, "Delete_" + kvp.Key, false));
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:return confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteItem")) + "');");
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("cmdDelete", LocalResourceFile));
                    writer.RenderBeginTag(HtmlTextWriterTag.A);

                    //Render Image
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Delete"));
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);

                    //Render end of Image
                    writer.RenderEndTag();

                    //Render end of Hyperlink
                    writer.RenderEndTag();

                    ControlStyle.AddAttributesToRender(writer);
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, kvp.Value);
                    if (length > Null.NullInteger)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString());
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + "_skin" + kvp.Key);
                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag();

                    writer.WriteBreak();
                }
                writer.WriteBreak();

                //Create Add Row
                //Render Hyperlink
                writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, "Add", false));
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("cmdAdd", LocalResourceFile));
                writer.RenderBeginTag(HtmlTextWriterTag.A);

                //Render Image
                writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Add"));
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);

                //Render end of Image
                writer.RenderEndTag();

                //Render end of Hyperlink
                writer.RenderEndTag();

                ControlStyle.AddAttributesToRender(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
                if (length > Null.NullInteger)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString());
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + "_skinnew");
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
                writer.WriteBreak();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            if (DictionaryValue != null)
            {
                foreach (KeyValuePair<int, string> kvp in DictionaryValue)
                {
                    ControlStyle.AddAttributesToRender(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(kvp.Value);
                    writer.RenderEndTag();
                    writer.WriteBreak();
                }
            }
        }
		
		#endregion

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string postedValue;
            var newDictionaryValue = new Dictionary<int, string>();
            foreach (KeyValuePair<int, string> kvp in DictionaryValue)
            {
                postedValue = postCollection[UniqueID + "_skin" + kvp.Key];
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
            postedValue = postCollection[UniqueID + "_skinnew"];
            if (!string.IsNullOrEmpty(postedValue))
            {
                AddedItem = postedValue;
            }
            DictionaryValue = newDictionaryValue;
            return dataChanged;
        }
    }
}
