// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;

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
    /// -----------------------------------------------------------------------------
    [ValidationPropertyAttribute("Value")]
    public abstract class EditControl : WebControl, IPostBackDataHandler
    {
        private object[] _CustomAttributes;

        public event PropertyChangedEventHandler ItemAdded;

        public event PropertyChangedEventHandler ItemDeleted;

        public event PropertyChangedEventHandler ValueChanged;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether returns whether the.
        /// </summary>
        /// <value>A boolean.</value>
        /// -----------------------------------------------------------------------------
        public virtual bool IsValid
        {
            get
            {
                return true;
            }
        }

        public virtual string EditControlClientId
        {
            get
            {
                return this.ClientID;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Custom Attributes for this Control.
        /// </summary>
        /// <value>An array of Attributes.</value>
        /// -----------------------------------------------------------------------------
        public object[] CustomAttributes
        {
            get
            {
                return this._CustomAttributes;
            }

            set
            {
                this._CustomAttributes = value;
                if ((this._CustomAttributes != null) && this._CustomAttributes.Length > 0)
                {
                    this.OnAttributesChanged();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Edit Mode of the Editor.
        /// </summary>
        /// <value>A boolean.</value>
        /// -----------------------------------------------------------------------------
        public PropertyEditorMode EditMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Local Resource File for the Control.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets name is the name of the field as a string.
        /// </summary>
        /// <value>A string representing the Name of the property.</value>
        /// -----------------------------------------------------------------------------
        public string Name { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Category to which this edit control belongs.
        /// </summary>
        /// <value>A string representing the Category of the property.</value>
        /// -----------------------------------------------------------------------------
        public string Category { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets oldValue is the initial value of the field.
        /// </summary>
        /// <value>The initial Value of the property.</value>
        /// -----------------------------------------------------------------------------
        public object OldValue { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Property is required.
        /// </summary>
        /// <value>The initial Value of the property.</value>
        /// -----------------------------------------------------------------------------
        public bool Required { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets systemType is the System Data Type for the property.
        /// </summary>
        /// <value>A string representing the Type of the property.</value>
        /// -----------------------------------------------------------------------------
        public string SystemType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets value is the value of the control.
        /// </summary>
        /// <value>The Value of the property.</value>
        /// -----------------------------------------------------------------------------
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets set Data Field of the control.
        /// </summary>
        public string DataField { get; set; }

        public UserInfo User { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets stringValue is the value of the control expressed as a String.
        /// </summary>
        /// <value>A string representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected abstract string StringValue { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadPostData loads the Post Back Data and determines whether the value has change.
        /// </summary>
        /// <param name="postDataKey">A key to the PostBack Data to load.</param>
        /// <param name="postCollection">A name value collection of postback data.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = postCollection[postDataKey];
            if (!presentValue.Equals(postedValue))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaisePostDataChangedEvent runs when the PostBackData has changed.  It triggers
        /// a ValueChanged Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void RaisePostDataChangedEvent()
        {
            this.OnDataChanged(EventArgs.Empty);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected abstract void OnDataChanged(EventArgs e);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnAttributesChanged runs when the CustomAttributes property has changed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnAttributesChanged()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is added to a collection type property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemAdded(PropertyEditorEventArgs e)
        {
            if (this.ItemAdded != null)
            {
                this.ItemAdded(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is deleted from a collection type property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemDeleted(PropertyEditorEventArgs e)
        {
            if (this.ItemDeleted != null)
            {
                this.ItemDeleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnValueChanged runs when the Value has changed.  It raises the ValueChanged
        /// Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnValueChanged(PropertyEditorEventArgs e)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void RenderViewMode(HtmlTextWriter writer)
        {
            string propValue = this.Page.Server.HtmlDecode(Convert.ToString(this.Value));

            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            var security = PortalSecurity.Instance;
            writer.Write(security.InputFilter(propValue, PortalSecurity.FilterFlag.NoScripting));
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void RenderEditMode(HtmlTextWriter writer)
        {
            string propValue = Convert.ToString(this.Value);

            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, propValue);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Render is called by the .NET framework to render the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void Render(HtmlTextWriter writer)
        {
            var strOldValue = this.OldValue as string;
            if (this.EditMode == PropertyEditorMode.Edit || (this.Required && string.IsNullOrEmpty(strOldValue)))
            {
                this.RenderEditMode(writer);
            }
            else
            {
                this.RenderViewMode(writer);
            }
        }
    }
}
