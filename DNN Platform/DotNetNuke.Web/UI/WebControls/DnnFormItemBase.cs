// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Collections;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    public abstract class DnnFormItemBase : WebControl, INamingContainer
    {
        private object _value;
        private string _requiredMessageSuffix = ".Required";
        private string _validationMessageSuffix = ".RegExError";

        protected DnnFormItemBase()
        {
            this.FormMode = DnnFormMode.Inherit;
            this.IsValid = true;

            this.Validators = new List<IValidator>();
        }

        public object Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        public string DataField { get; set; }

        public string DataMember { get; set; }

        public DnnFormMode FormMode { get; set; }

        public bool IsValid { get; private set; }

        public string OnClientClicked { get; set; }

        public string LocalResourceFile { get; set; }

        public bool Required { get; set; }

        public string ResourceKey { get; set; }

        public string RequiredMessageSuffix
        {
            get
            {
                return this._requiredMessageSuffix;
            }

            set
            {
                this._requiredMessageSuffix = value;
            }
        }

        public string ValidationMessageSuffix
        {
            get
            {
                return this._validationMessageSuffix;
            }

            set
            {
                this._validationMessageSuffix = value;
            }
        }

        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<IValidator> Validators { get; private set; }

        public string ValidationExpression { get; set; }

        internal object DataSource { get; set; }

        protected PropertyInfo ChildProperty
        {
            get
            {
                Type type = this.Property.PropertyType;
                IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
                return props.SingleOrDefault(p => p.Name == this.DataField);
            }
        }

        protected PortalSettings PortalSettings
        {
            get { return PortalController.Instance.GetCurrentPortalSettings(); }
        }

        protected PropertyInfo Property
        {
            get
            {
                Type type = this.DataSource.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
                return !string.IsNullOrEmpty(this.DataMember)
                    ? props.SingleOrDefault(p => p.Name == this.DataMember)
                    : props.SingleOrDefault(p => p.Name == this.DataField);
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        public void CheckIsValid()
        {
            this.IsValid = true;
            foreach (BaseValidator validator in this.Validators)
            {
                validator.Validate();
                if (!validator.IsValid)
                {
                    this.IsValid = false;
                    break;
                }
            }
        }

        public void DataBindItem(bool useDataSource)
        {
            if (useDataSource)
            {
                this.OnDataBinding(EventArgs.Empty);
                this.Controls.Clear();
                this.ClearChildViewState();
                this.TrackViewState();

                this.DataBindInternal();

                this.CreateControlHierarchy();
                this.ChildControlsCreated = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(this.DataField))
                {
                    this.UpdateDataSourceInternal(null, this._value, this.DataField);
                }
            }
        }

        protected virtual void CreateControlHierarchy()
        {
            // Load Item Style
            this.CssClass = "dnnFormItem";
            this.CssClass += (this.FormMode == DnnFormMode.Long) ? string.Empty : " dnnFormShort";

            if (string.IsNullOrEmpty(this.ResourceKey))
            {
                this.ResourceKey = this.DataField;
            }

            // Add Label
            var label = new DnnFormLabel
            {
                LocalResourceFile = this.LocalResourceFile,
                ResourceKey = this.ResourceKey + ".Text",
                ToolTipKey = this.ResourceKey + ".Help",
                ViewStateMode = ViewStateMode.Disabled,
            };

            if (this.Required)
            {
                label.RequiredField = true;
            }

            this.Controls.Add(label);

            WebControl inputControl = this.CreateControlInternal(this);
            label.AssociatedControlID = inputControl.ID;
            this.AddValidators(inputControl.ID);
        }

        /// <summary>
        /// Use container to add custom control hierarchy to.
        /// </summary>
        /// <param name="container"></param>
        /// <returns>An "input" control that can be used for attaching validators.</returns>
        protected virtual WebControl CreateControlInternal(Control container)
        {
            return null;
        }

        protected override void CreateChildControls()
        {
            // CreateChildControls re-creates the children (the items)
            // using the saved view state.
            // First clear any existing child controls.
            this.Controls.Clear();

            this.CreateControlHierarchy();
        }

        protected void DataBindInternal(string dataField, ref object value)
        {
            var dictionary = this.DataSource as IDictionary;
            if (dictionary != null)
            {
                if (!string.IsNullOrEmpty(dataField) && dictionary.Contains(dataField))
                {
                    value = dictionary[dataField];
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(dataField))
                {
                    if (string.IsNullOrEmpty(this.DataMember))
                    {
                        if (this.Property != null && this.Property.GetValue(this.DataSource, null) != null)
                        {
                            // ReSharper disable PossibleNullReferenceException
                            value = this.Property.GetValue(this.DataSource, null);

                            // ReSharper restore PossibleNullReferenceException
                        }
                    }
                    else
                    {
                        if (this.Property != null && this.Property.GetValue(this.DataSource, null) != null)
                        {
                            // ReSharper disable PossibleNullReferenceException
                            object parentValue = this.Property.GetValue(this.DataSource, null);
                            if (this.ChildProperty != null && this.ChildProperty.GetValue(parentValue, null) != null)
                            {
                                value = this.ChildProperty.GetValue(parentValue, null);
                            }

                            // ReSharper restore PossibleNullReferenceException
                        }
                    }
                }
            }
        }

        protected virtual void DataBindInternal()
        {
            this.DataBindInternal(this.DataField, ref this._value);
        }

        protected void UpdateDataSource(object oldValue, object newValue, string dataField)
        {
            this.CheckIsValid();

            this._value = newValue;

            this.UpdateDataSourceInternal(oldValue, newValue, dataField);
        }

        protected override void LoadControlState(object state)
        {
            this._value = state;
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        protected override void OnInit(EventArgs e)
        {
            this.Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            return this._value;
        }

        private void AddValidators(string controlId)
        {
            var value = this.Value as string;
            this.Validators.Clear();

            // Add Validators
            if (this.Required)
            {
                var requiredValidator = new RequiredFieldValidator
                {
                    ID = this.ID + "_Required",
                    ErrorMessage = this.ResourceKey + this.RequiredMessageSuffix,
                };
                this.Validators.Add(requiredValidator);
            }

            if (!string.IsNullOrEmpty(this.ValidationExpression))
            {
                var regexValidator = new RegularExpressionValidator
                {
                    ID = this.ID + "_RegEx",
                    ErrorMessage = this.ResourceKey + this.ValidationMessageSuffix,
                    ValidationExpression = this.ValidationExpression,
                };
                if (!string.IsNullOrEmpty(value))
                {
                    regexValidator.IsValid = Regex.IsMatch(value, this.ValidationExpression);
                    this.IsValid = regexValidator.IsValid;
                }

                this.Validators.Add(regexValidator);
            }

            if (this.Validators.Count > 0)
            {
                foreach (BaseValidator validator in this.Validators)
                {
                    validator.ControlToValidate = controlId;
                    validator.Display = ValidatorDisplay.Dynamic;
                    validator.ErrorMessage = this.LocalizeString(validator.ErrorMessage);
                    validator.CssClass = "dnnFormMessage dnnFormError";
                    this.Controls.Add(validator);
                }
            }
        }

        private void UpdateDataSourceInternal(object oldValue, object newValue, string dataField)
        {
            if (this.DataSource != null)
            {
                if (this.DataSource is IDictionary<string, string>)
                {
                    var dictionary = this.DataSource as IDictionary<string, string>;
                    if (dictionary.ContainsKey(dataField) && !ReferenceEquals(newValue, oldValue))
                    {
                        dictionary[dataField] = newValue as string;
                    }
                }
                else if (this.DataSource is IIndexable)
                {
                    var indexer = this.DataSource as IIndexable;
                    indexer[dataField] = newValue;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.DataMember))
                    {
                        if (this.Property != null)
                        {
                            if (!ReferenceEquals(newValue, oldValue))
                            {
                                if (this.Property.PropertyType.IsEnum)
                                {
                                    this.Property.SetValue(this.DataSource, Enum.Parse(this.Property.PropertyType, newValue.ToString()), null);
                                }
                                else
                                {
                                    this.Property.SetValue(this.DataSource, Convert.ChangeType(newValue, this.Property.PropertyType), null);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.Property != null)
                        {
                            object parentValue = this.Property.GetValue(this.DataSource, null);
                            if (parentValue != null)
                            {
                                if (parentValue is IDictionary<string, string>)
                                {
                                    var dictionary = parentValue as IDictionary<string, string>;
                                    if (dictionary.ContainsKey(dataField) && !ReferenceEquals(newValue, oldValue))
                                    {
                                        dictionary[dataField] = newValue as string;
                                    }
                                }
                                else if (parentValue is IIndexable)
                                {
                                    var indexer = parentValue as IIndexable;
                                    indexer[dataField] = newValue;
                                }
                                else if (this.ChildProperty != null)
                                {
                                    if (this.Property.PropertyType.IsEnum)
                                    {
                                        this.ChildProperty.SetValue(parentValue, Enum.Parse(this.ChildProperty.PropertyType, newValue.ToString()), null);
                                    }
                                    else
                                    {
                                        this.ChildProperty.SetValue(parentValue, Convert.ChangeType(newValue, this.ChildProperty.PropertyType), null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
