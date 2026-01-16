// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;

    /// <summary>The EditControl control provides a standard UI component for editing properties.</summary>
    [ValidationPropertyAttribute("Value")]
    public abstract class EditControl : WebControl, IPostBackDataHandler
    {
        private object[] customAttributes;

        public event PropertyChangedEventHandler ItemAdded;

        public event PropertyChangedEventHandler ItemDeleted;

        public event PropertyChangedEventHandler ValueChanged;

        /// <summary>Gets a value indicating whether the control is valid.</summary>
        /// <value>A boolean.</value>
        public virtual bool IsValid => true;

        public virtual string EditControlClientId => this.ClientID;

        /// <summary>Gets or sets the Custom Attributes for this Control.</summary>
        /// <value>An array of Attributes.</value>
        public object[] CustomAttributes
        {
            get
            {
                return this.customAttributes;
            }

            set
            {
                this.customAttributes = value;
                if ((this.customAttributes != null) && this.customAttributes.Length > 0)
                {
                    this.OnAttributesChanged();
                }
            }
        }

        /// <summary>Gets or sets the Edit Mode of the Editor.</summary>
        /// <value>A boolean.</value>
        public PropertyEditorMode EditMode { get; set; }

        /// <summary>Gets or sets the Local Resource File for the Control.</summary>
        /// <value>A String.</value>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets or sets name is the name of the field as a string.</summary>
        /// <value>A string representing the Name of the property.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the Category to which this edit control belongs.</summary>
        /// <value>A string representing the Category of the property.</value>
        public string Category { get; set; }

        /// <summary>Gets or sets oldValue is the initial value of the field.</summary>
        /// <value>The initial Value of the property.</value>
        public object OldValue { get; set; }

        /// <summary>Gets or sets a value indicating whether the Property is required.</summary>
        /// <value>The initial Value of the property.</value>
        public bool Required { get; set; }

        /// <summary>Gets or sets systemType is the System Data Type for the property.</summary>
        /// <value>A string representing the Type of the property.</value>
        public string SystemType { get; set; }

        /// <summary>Gets or sets value is the value of the control.</summary>
        /// <value>The Value of the property.</value>
        public object Value { get; set; }

        /// <summary>Gets or sets set Data Field of the control.</summary>
        public string DataField { get; set; }

        public UserInfo User { get; set; }

        /// <summary>Gets or sets stringValue is the value of the control expressed as a String.</summary>
        /// <value>A string representing the Value.</value>
        protected abstract string StringValue { get; set; }

        /// <summary>LoadPostData loads the Post Back Data and determines whether the value has change.</summary>
        /// <param name="postDataKey">A key to the PostBack Data to load.</param>
        /// <param name="postCollection">A name value collection of postback data.</param>
        /// <returns><see langword="true"/> if the value has changed, otherwise <see langword="false"/>.</returns>
        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = postCollection[postDataKey];
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        /// <summary>RaisePostDataChangedEvent runs when the PostBackData has changed.  It triggers a <see cref="ValueChanged"/> Event.</summary>
        public void RaisePostDataChangedEvent()
        {
            this.OnDataChanged(EventArgs.Empty);
        }

        /// <summary>OnDataChanged runs when the PostbackData has changed.  It raises the <see cref="ValueChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected abstract void OnDataChanged(EventArgs e);

        /// <summary>OnAttributesChanged runs when the CustomAttributes property has changed.</summary>
        protected virtual void OnAttributesChanged()
        {
        }

        /// <summary>Runs when an item is added to a collection type property. It raises the <see cref="ItemAdded"/> event.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnItemAdded(PropertyEditorEventArgs e)
        {
            if (this.ItemAdded != null)
            {
                this.ItemAdded(this, e);
            }
        }

        /// <summary>Runs when an item is deleted from a collection type property. It raises the <see cref="ItemDeleted"/> event.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnItemDeleted(PropertyEditorEventArgs e)
        {
            if (this.ItemDeleted != null)
            {
                this.ItemDeleted(this, e);
            }
        }

        /// <summary>OnValueChanged runs when the Value has changed.  It raises the <see cref="ValueChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnValueChanged(PropertyEditorEventArgs e)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, e);
            }
        }

        /// <summary>RenderViewMode renders the View (readonly) mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected virtual void RenderViewMode(HtmlTextWriter writer)
        {
            string propValue = this.Page.Server.HtmlDecode(Convert.ToString(this.Value, CultureInfo.InvariantCulture));

            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            var security = PortalSecurity.Instance;
            writer.Write(security.InputFilter(propValue, PortalSecurity.FilterFlag.NoScripting));
            writer.RenderEndTag();
        }

        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected virtual void RenderEditMode(HtmlTextWriter writer)
        {
            string propValue = Convert.ToString(this.Value, CultureInfo.InvariantCulture);

            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, propValue);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        /// <inheritdoc />
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
