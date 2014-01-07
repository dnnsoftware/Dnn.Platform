#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Users;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      EditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditControl control provides a standard UI component for editing 
    /// properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///     [cnurse]	02/14/2006	created
    /// </history>
    /// -----------------------------------------------------------------------------
    [ValidationPropertyAttribute("Value")]
    public abstract class EditControl : WebControl, IPostBackDataHandler
    {
		#region "Private Members"

        private object[] _CustomAttributes;

		#endregion

		#region "Data Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Custom Attributes for this Control
        /// </summary>
        /// <value>An array of Attributes</value>
        /// <history>
        ///     [cnurse]	05/08/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public object[] CustomAttributes
        {
            get
            {
                return _CustomAttributes;
            }
            set
            {
                _CustomAttributes = value;
                if ((_CustomAttributes != null) && _CustomAttributes.Length > 0)
                {
                    OnAttributesChanged();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Edit Mode of the Editor
        /// </summary>
        /// <value>A boolean</value>
        /// <history>
        ///     [cnurse]	02/27/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public PropertyEditorMode EditMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns whether the
        /// </summary>
        /// <value>A boolean</value>
        /// <history>
        ///     [cnurse]	02/27/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual bool IsValid
        {
            get
            {
                return true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Local Resource File for the Control
        /// </summary>
        /// <value>A String</value>
        /// <history>
        ///     [cnurse]	05/17/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Name is the name of the field as a string
        /// </summary>
        /// <value>A string representing the Name of the property</value>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Name { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldValue is the initial value of the field
        /// </summary>
        /// <value>The initial Value of the property</value>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public object OldValue { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// gets and sets whether the Property is required
        /// </summary>
        /// <value>The initial Value of the property</value>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool Required { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SystemType is the System Data Type for the property
        /// </summary>
        /// <value>A string representing the Type of the property</value>
        /// <history>
        ///     [cnurse]	02/22/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string SystemType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Value is the value of the control
        /// </summary>
        /// <value>The Value of the property</value>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public object Value { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StringValue is the value of the control expressed as a String
        /// </summary>
        /// <value>A string representing the Value</value>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected abstract string StringValue { get; set; }

        public UserInfo User { get; set; }

        #endregion

        #region IPostBackDataHandler Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadPostData loads the Post Back Data and determines whether the value has change
        /// </summary>
        /// <param name="postDataKey">A key to the PostBack Data to load</param>
        /// <param name="postCollection">A name value collection of postback data</param>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = StringValue;
            string postedValue = postCollection[postDataKey];
            if (!presentValue.Equals(postedValue))
            {
                Value = postedValue;
                dataChanged = true;
            }
            return dataChanged;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaisePostDataChangedEvent runs when the PostBackData has changed.  It triggers
        /// a ValueChanged Event
        /// </summary>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void RaisePostDataChangedEvent()
        {
            OnDataChanged(EventArgs.Empty);
        }

        #endregion
		
		#region "Events"

        public event PropertyChangedEventHandler ItemAdded;
        public event PropertyChangedEventHandler ItemDeleted;
        public event PropertyChangedEventHandler ValueChanged;

		#endregion

		#region "Abstract Members"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected abstract void OnDataChanged(EventArgs e);

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
        protected virtual void OnAttributesChanged()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is added to a collection type property
        /// </summary>
        /// <history>
        ///     [cnurse]	02/05/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemAdded(PropertyEditorEventArgs e)
        {
            if (ItemAdded != null)
            {
                ItemAdded(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is deleted from a collection type property
        /// </summary>
        /// <history>
        ///     [cnurse]	02/05/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemDeleted(PropertyEditorEventArgs e)
        {
            if (ItemDeleted != null)
            {
                ItemDeleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnValueChanged runs when the Value has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// <history>
        ///     [cnurse]	02/21/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void OnValueChanged(PropertyEditorEventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// <history>
        ///     [cnurse]	02/27/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void RenderViewMode(HtmlTextWriter writer)
        {
            string propValue = Page.Server.HtmlDecode(Convert.ToString(Value));

            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            var security = new PortalSecurity();
            writer.Write(security.InputFilter(propValue, PortalSecurity.FilterFlag.NoScripting));
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// <history>
        ///     [cnurse]	02/27/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void RenderEditMode(HtmlTextWriter writer)
        {
            string propValue = Convert.ToString(Value);

            ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, propValue);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Render is called by the .NET framework to render the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// <history>
        ///     [cnurse]	02/27/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void Render(HtmlTextWriter writer)
        {
            var strOldValue = OldValue as string;
            if (EditMode == PropertyEditorMode.Edit || (Required && string.IsNullOrEmpty(strOldValue)))
            {
                RenderEditMode(writer);
            }
            else
            {
                RenderViewMode(writer);
            }
        }
		
		#endregion
    }
}
