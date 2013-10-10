#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNListEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNListEditControl control provides a standard UI component for selecting
    /// from Lists
    /// </summary>
    /// <history>
    ///     [cnurse]	05/04/2006	created
    /// </history>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DNNListEditControl runat=server></{0}:DNNListEditControl>")]
    public class DNNListEditControl : EditControl, IPostBackEventHandler
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (DNNListEditControl));
        private List<ListEntryInfo> _listEntries;
        private string _listName = "";

        public DNNListEditControl()
        {
            ValueField = ListBoundField.Value;
            TextField = ListBoundField.Text;
            ParentKey = "";
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the List Auto Posts Back
        /// </summary>
        /// <history>
        ///     [cnurse]	05/04/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool AutoPostBack { get; set; }

		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IntegerValue returns the Integer representation of the Value
        /// </summary>
        /// <value>An integer representing the Value</value>
        /// <history>
        ///     [cnurse]	06/14/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected int IntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                try
                {
                    intValue = Convert.ToInt32(Value);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                }
                return intValue;
            }
        }

        /// <summary>
        /// List gets the List associated with the control
        /// </summary>
        [Obsolete("Obsoleted in 6.0.1 use ListEntries instead"), EditorBrowsable(EditorBrowsableState.Never)]
        protected ListEntryInfoCollection List
        {
            get
            {
                var listController = new ListController();
                return listController.GetListEntryInfoCollection(ListName, ParentKey, PortalId);
            }
        }

        /// <summary>
        /// Gets the ListEntryInfo objects associated witht the control
        /// </summary>
        protected IEnumerable<ListEntryInfo> ListEntries
        {
            get
            {
                if(_listEntries == null)
                {
                    var listController = new ListController();
                    _listEntries = listController.GetListEntryInfoItems(ListName, ParentKey, PortalId).ToList();
                }

                return _listEntries;
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ListName is the name of the List to display
        /// </summary>
        /// <history>
        ///     [cnurse]	05/04/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual string ListName
        {
            get
            {
                if (_listName == Null.NullString)
                {
                    _listName = Name;
                }
                return _listName;
            }
            set
            {
                _listName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldIntegerValue returns the Integer representation of the OldValue
        /// </summary>
        /// <value>An integer representing the OldValue</value>
        /// <history>
        ///     [cnurse]	06/14/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected int OldIntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                try
                {
					//Try and cast the value to an Integer
                    intValue = Convert.ToInt32(OldValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                }
                return intValue;
            }
        }

        /// <summary>
        /// The parent key of the List to display
        /// </summary>
        protected virtual string ParentKey { get; set; }

        protected int PortalId
        {
            get
            {
                return PortalController.GetEffectivePortalId(PortalSettings.Current.PortalId);
            }
        }

        /// <summary>
        /// The field to display in the combo
        /// </summary>
        protected virtual ListBoundField TextField { get; set; }

        /// <summary>
        /// The field to use as the combo item values
        /// </summary>
        protected virtual ListBoundField ValueField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldStringValue returns the Boolean representation of the OldValue
        /// </summary>
        /// <value>A String representing the OldValue</value>
        /// <history>
        ///     [cnurse]	06/14/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string OldStringValue
        {
            get
            {
                return Convert.ToString(OldValue);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StringValue is the value of the control expressed as a String
        /// </summary>
        /// <value>A string representing the Value</value>
        /// <history>
        ///     [cnurse]	05/04/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                return Convert.ToString(Value);
            }
            set
            {
                if (ValueField == ListBoundField.Id)
                {
					//Integer type field
                    Value = Int32.Parse(value);
                }
                else
                {
					//String type Field
                    Value = value;
                }
            }
        }
		
		#endregion

        #region IPostBackEventHandler Members

        public void RaisePostBackEvent(string eventArgument)
        {
            if (AutoPostBack)
            {
                OnItemChanged(GetEventArgs());
            }
        }

        #endregion

        public event PropertyChangedEventHandler ItemChanged;
		
		#region "Private Methods"

        private PropertyEditorEventArgs GetEventArgs()
        {
            var args = new PropertyEditorEventArgs(Name);
            if (ValueField == ListBoundField.Id)
            {
				//This is an Integer Value
                args.Value = IntegerValue;
                args.OldValue = OldIntegerValue;
            }
            else
            {
				//This is a String Value
                args.Value = StringValue;
                args.OldValue = OldStringValue;
            }
            args.StringValue = StringValue;
            return args;
        }
		
		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAttributesChanged runs when the CustomAttributes property has changed.
        /// </summary>
        /// <history>
        ///     [cnurse]	06/08/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnAttributesChanged()
        {
			//Get the List settings out of the "Attributes"
            if ((CustomAttributes != null))
            {
                foreach (Attribute attribute in CustomAttributes)
                {
                    if (attribute is ListAttribute)
                    {
                        var listAtt = (ListAttribute) attribute;
                        ListName = listAtt.ListName;
                        ParentKey = listAtt.ParentKey;
                        TextField = listAtt.TextField;
                        ValueField = listAtt.ValueField;
                        break;
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// <history>
        ///     [cnurse]	05/04/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            base.OnValueChanged(GetEventArgs());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnItemChanged runs when the Item has changed
        /// </summary>
        /// <history>
        ///     [cnurse]	05/04/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemChanged(PropertyEditorEventArgs e)
        {
            if (ItemChanged != null)
            {
                ItemChanged(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// <history>
        ///     [cnurse]	05/04/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            var objListController = new ListController();
            ListEntryInfo entry = null;
            string entryText = Null.NullString;
            switch (ValueField)
            {
                case ListBoundField.Id:
                    entry = objListController.GetListEntryInfo(Convert.ToInt32(Value));
                    break;
                case ListBoundField.Text:
                    entryText = StringValue;
                    break;
                case ListBoundField.Value:
                    entry = objListController.GetListEntryInfo(ListName, StringValue);
                    break;
            }
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (entry != null)
            {
                switch (TextField)
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
			
			//Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// <history>
        ///     [cnurse]	05/04/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            //Render the Select Tag
            ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            if (AutoPostBack)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, Page.ClientScript.GetPostBackEventReference(this, ID));
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            //Add the Not Specified Option
            if (ValueField == ListBoundField.Text)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
            }
            if (StringValue == Null.NullString)
            {
				//Add the Selected Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write("<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">");
            writer.RenderEndTag();
            
            foreach (ListEntryInfo item in ListEntries)
            {
                string itemValue = Null.NullString;
                
				//Add the Value Attribute
				switch (ValueField)
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
                if (StringValue == itemValue)
                {
					//Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }
                
				//Render Option Tag
				writer.RenderBeginTag(HtmlTextWriterTag.Option);
                switch (TextField)
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
            
			//Close Select Tag
			writer.RenderEndTag();
        }
		
		#endregion
    }
}