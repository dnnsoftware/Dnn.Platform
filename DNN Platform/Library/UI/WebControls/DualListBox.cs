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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public class DualListBox : WebControl, IPostBackEventHandler, IPostBackDataHandler
    {
		#region Private Members

        private readonly Style _AvailableListBoxStyle = new Style();
        private readonly Style _ButtonStyle = new Style();
        private readonly TableStyle _ContainerStyle = new TableStyle();
        private readonly Style _HeaderStyle = new Style();
        private readonly Style _SelectedListBoxStyle = new Style();
        private List<string> _AddValues;
        private List<string> _RemoveValues;

		#endregion

        public DualListBox()
        {
            ShowAddButton = true;
            ShowAddAllButton = true;
            ShowRemoveButton = true;
            ShowRemoveAllButton = true;
        }

		#region Public Properties

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

		#region Style Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Available List Box Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty),
         TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Available List Box.")]
        public Style AvailableListBoxStyle
        {
            get
            {
                return _AvailableListBoxStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Button Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty),
         TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Button.")]
        public Style ButtonStyle
        {
            get
            {
                return _ButtonStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Container Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty),
         TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Container.")]
        public TableStyle ContainerStyle
        {
            get
            {
                return _ContainerStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Header Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty),
         TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Header.")]
        public Style HeaderStyle
        {
            get
            {
                return _HeaderStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Selected List Box Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty),
         TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Selected List Box.")]
        public Style SelectedListBoxStyle
        {
            get
            {
                return _SelectedListBoxStyle;
            }
        }
		#endregion
		
		#endregion

        #region IPostBackDataHandler Members

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool retValue = Null.NullBoolean;
            string addItems = postCollection[postDataKey + "_Available"];
            if (!string.IsNullOrEmpty(addItems))
            {
                _AddValues = new List<string>();
                foreach (string addItem in addItems.Split(','))
                {
                    _AddValues.Add(addItem);
                }
                retValue = true;
            }
            string removeItems = postCollection[postDataKey + "_Selected"];
            if (!string.IsNullOrEmpty(removeItems))
            {
                _RemoveValues = new List<string>();
                foreach (string removeItem in removeItems.Split(','))
                {
                    _RemoveValues.Add(removeItem);
                }
                retValue = true;
            }
            return retValue;
        }

        public void RaisePostDataChangedEvent()
        {
        }

        #endregion

        #region IPostBackEventHandler Members

        public void RaisePostBackEvent(string eventArgument)
        {
            switch (eventArgument)
            {
                case "Add":
                    OnAddButtonClick(new DualListBoxEventArgs(_AddValues));
                    break;
                case "AddAll":
                    OnAddAllButtonClick(new EventArgs());
                    break;
                case "Remove":
                    OnRemoveButtonClick(new DualListBoxEventArgs(_RemoveValues));
                    break;
                case "RemoveAll":
                    OnRemoveAllButtonClick(new EventArgs());
                    break;
            }
        }

        #endregion
		
		#region Events

        public event DualListBoxEventHandler AddButtonClick;
        public event EventHandler AddAllButtonClick;
        public event DualListBoxEventHandler RemoveButtonClick;
        public event EventHandler RemoveAllButtonClick;
		
		#endregion

		#region Private Methods

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
                    PropertyInfo objTextProperty = item.GetType().GetProperty(DataTextField, bindings);
                    PropertyInfo objValueProperty = item.GetType().GetProperty(DataValueField, bindings);
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

            //Begin Button Row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            //Begin Button Cell
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            switch (buttonType)
            {
                case "Add":
                    buttonText = string.IsNullOrEmpty(AddKey) 
                                    ? AddText 
                                    : Localization.GetString(AddKey, LocalResourceFile);
                    imageURL = AddImageURL;
                    break;
                case "AddAll":
                    buttonText = string.IsNullOrEmpty(AddAllKey) 
                                    ? AddAllText 
                                    : Localization.GetString(AddAllKey, LocalResourceFile);
                    imageURL = AddAllImageURL;
                    break;
                case "Remove":
                    buttonText = string.IsNullOrEmpty(RemoveKey) 
                                    ? RemoveText 
                                    : Localization.GetString(RemoveKey, LocalResourceFile);
                    imageURL = RemoveImageURL;
                    break;
                case "RemoveAll":
                    buttonText = string.IsNullOrEmpty(RemoveAllKey) 
                                    ? RemoveAllText 
                                    : Localization.GetString(RemoveAllKey, LocalResourceFile);
                    imageURL = RemoveAllImageURL;
                    break;
            }
			
            //Render Hyperlink
            writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackEventReference(GetPostBackOptions(buttonType)));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, buttonText);
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            //Render Image
            if (!string.IsNullOrEmpty(imageURL))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, ResolveClientUrl(imageURL));
                writer.AddAttribute(HtmlTextWriterAttribute.Title, buttonText);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }
            else
            {
                writer.Write(buttonText);
            }
			
            //End of Hyperlink
            writer.RenderEndTag();

            //End of Button Cell
            writer.RenderEndTag();

            //Render end of Button Row
            writer.RenderEndTag();
        }

        private void RenderButtons(HtmlTextWriter writer)
        {
			//render table
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            if (ShowAddButton)
            {
                RenderButton("Add", writer);
            }
            if (ShowAddAllButton)
            {
                RenderButton("AddAll", writer);
            }

            //Begin Button Row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            //Begin Button Cell
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.Write("&nbsp;");

        	//End of Button Cell
            writer.RenderEndTag();

            //Render end of Button Row
            writer.RenderEndTag();

            if (ShowRemoveButton)
            {
                RenderButton("Remove", writer);
            }
            if (ShowRemoveAllButton)
            {
                RenderButton("RemoveAll", writer);
            }

            //Render end of table
            writer.RenderEndTag();
        }

        private void RenderListBox(string listType, object dataSource, Style style, HtmlTextWriter writer)
        {
            if (dataSource != null)
            {
                NameValueCollection list = GetList(listType, dataSource);
                if (list != null)
                {
                    if (style != null)
                    {
                        style.AddAttributesToRender(writer);
                    }
					
                    //Render ListBox
                    writer.AddAttribute(HtmlTextWriterAttribute.Multiple, "multiple");
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + "_" + listType);
                    writer.RenderBeginTag(HtmlTextWriterTag.Select);
                    for (int i = 0; i <= list.Count - 1; i++)
                    {
						//Render option tags for each item
                        writer.AddAttribute(HtmlTextWriterAttribute.Value, list.Get(i));
                        writer.RenderBeginTag(HtmlTextWriterTag.Option);
                        writer.Write(list.GetKey(i));
                        writer.RenderEndTag();
                    }
					
                    //Render ListBox end
                    writer.RenderEndTag();
                }
            }
        }

        private void RenderHeader(HtmlTextWriter writer)
        {
			//render Header row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (HeaderStyle != null)
            {
                HeaderStyle.AddAttributesToRender(writer);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(Localization.GetString(ID + "_Available", LocalResourceFile));
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.RenderEndTag();
            if (HeaderStyle != null)
            {
                HeaderStyle.AddAttributesToRender(writer);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(Localization.GetString(ID + "_Selected", LocalResourceFile));
            writer.RenderEndTag();

            //Render end of Header Row
            writer.RenderEndTag();
        }

        private void RenderListBoxes(HtmlTextWriter writer)
        {
			//render List Boxes row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderListBox("Available", AvailableDataSource, AvailableListBoxStyle, writer);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderButtons(writer);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderListBox("Selected", SelectedDataSource, SelectedListBoxStyle, writer);
            writer.RenderEndTag();

            //Render end of List Boxes Row
            writer.RenderEndTag();
        }

        protected virtual PostBackOptions GetPostBackOptions(string argument)
        {
            var postBackOptions = new PostBackOptions(this, argument) {RequiresJavaScriptProtocol = true};

            if (this.CausesValidation && this.Page.GetValidators(this.ValidationGroup).Count > 0)
            {
                postBackOptions.PerformValidation = true;
                postBackOptions.ValidationGroup = this.ValidationGroup;
            }
            return postBackOptions;
        }
		
		#endregion

		#region Protected Methods

        protected void OnAddButtonClick(DualListBoxEventArgs e)
        {
            if (AddButtonClick != null)
            {
                AddButtonClick(this, e);
            }
        }

        protected void OnAddAllButtonClick(EventArgs e)
        {
            if (AddAllButtonClick != null)
            {
                AddAllButtonClick(this, e);
            }
        }

        protected void OnRemoveButtonClick(DualListBoxEventArgs e)
        {
            if (RemoveButtonClick != null)
            {
                RemoveButtonClick(this, e);
            }
        }

        protected void OnRemoveAllButtonClick(EventArgs e)
        {
            if (RemoveAllButtonClick != null)
            {
                RemoveAllButtonClick(this, e);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Page != null)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
			//render table
            if (ContainerStyle != null)
            {
                ContainerStyle.AddAttributesToRender(writer);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            //Render Header Row
            RenderHeader(writer);

            //Render ListBox row
            RenderListBoxes(writer);

            //Render end of table
            writer.RenderEndTag();
        }
		
		#endregion
    }
}
