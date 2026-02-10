// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A base class for a control that is an item in a form.</summary>
    public abstract class DnnFormItemBase : WebControl, INamingContainer
    {
        private object value;

        /// <summary>Initializes a new instance of the <see cref="DnnFormItemBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        protected DnnFormItemBase()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormItemBase"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        protected DnnFormItemBase(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.AppStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.EventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();

            this.FormMode = DnnFormMode.Inherit;
            this.IsValid = true;

            this.Validators = [];
        }

        /// <summary>Gets or sets the value.</summary>
        public object Value
        {
            get => this.value;
            set => this.value = value;
        }

        /// <summary>Gets or sets the data field.</summary>
        public string DataField { get; set; }

        /// <summary>Gets or sets the data member.</summary>
        public string DataMember { get; set; }

        /// <summary>Gets or sets the form mode.</summary>
        public DnnFormMode FormMode { get; set; }

        /// <summary>Gets a value indicating whether the item is valid.</summary>
        public bool IsValid { get; private set; }

        /// <summary>Gets or sets the client click event handler.</summary>
        public string OnClientClicked { get; set; }

        /// <summary>Gets or sets the path to the local resource file.</summary>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets or sets a value indicating whether the item is required.</summary>
        public bool Required { get; set; }

        /// <summary>Gets or sets the resource key for the label.</summary>
        public string ResourceKey { get; set; }

        /// <summary>Gets or sets the suffix to the message indicating the field is required.</summary>
        public string RequiredMessageSuffix { get; set; } = ".Required";

        /// <summary>Gets or sets the suffix to the validation message.</summary>
        public string ValidationMessageSuffix { get; set; } = ".RegExError";

        /// <summary>Gets the validators.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<IValidator> Validators { get; private set; }

        /// <summary>Gets or sets the validation expression.</summary>
        public string ValidationExpression { get; set; }

        /// <summary>Gets or sets the data source.</summary>
        internal object DataSource { get; set; }

        /// <summary>Gets the application status.</summary>
        protected IApplicationStatusInfo AppStatus { get; }

        /// <summary>Gets the event logger.</summary>
        protected IEventLogger EventLogger { get; }

        /// <summary>Gets the property of <see cref="Property"/> specified by <see cref="DataField"/>.</summary>
        protected PropertyInfo ChildProperty
        {
            get
            {
                Type type = this.Property.PropertyType;
                IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
                return props.SingleOrDefault(p => p.Name == this.DataField);
            }
        }

        /// <summary>Gets the current portal settings.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings PortalSettings => PortalSettings.Current;

        /// <summary>Gets the property from the <see cref="DataSource"/> matching the <see cref="DataMember"/> (or falling back to <see cref="DataField"/>).</summary>
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

        /// <inheritdoc />
        protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.Div;

        /// <summary>Checks whether this item is valid, setting <see cref="IsValid"/>.</summary>
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

        /// <summary>Data-binds the item.</summary>
        /// <param name="useDataSource">Whether to use the data source.</param>
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
                    this.UpdateDataSourceInternal(null, this.value, this.DataField);
                }
            }
        }

        /// <summary>Creates the control hierarchy.</summary>
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
            var label = new DnnFormLabel(this.AppStatus, this.EventLogger)
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

        /// <summary>Use container to add custom control hierarchy to.</summary>
        /// <param name="container">The container in which to render this control.</param>
        /// <returns>An "input" control that can be used for attaching validators.</returns>
        protected virtual WebControl CreateControlInternal(Control container)
        {
            return null;
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            // CreateChildControls re-creates the children (the items)
            // using the saved view state.
            // First clear any existing child controls.
            this.Controls.Clear();

            this.CreateControlHierarchy();
        }

        /// <summary>Does the data binding.</summary>
        /// <param name="dataField">The data field.</param>
        /// <param name="value">The value.</param>
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

        /// <summary>Does the data binding.</summary>
        protected virtual void DataBindInternal()
        {
            this.DataBindInternal(this.DataField, ref this.value);
        }

        /// <summary>Updates the data source.</summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="dataField">The data field.</param>
        protected void UpdateDataSource(object oldValue, object newValue, string dataField)
        {
            this.CheckIsValid();

            this.value = newValue;

            this.UpdateDataSourceInternal(oldValue, newValue, dataField);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        protected override void LoadControlState(object state)
        {
            this.value = state;
        }

        /// <summary>Gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized text.</returns>
        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            this.Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        /// <inheritdoc />
        protected override object SaveControlState()
        {
            return this.value;
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
                if (this.DataSource is IDictionary<string, string> dict)
                {
                    if (dict.ContainsKey(dataField) && !ReferenceEquals(newValue, oldValue))
                    {
                        dict[dataField] = newValue as string;
                    }
                }
                else if (this.DataSource is IIndexable indexer)
                {
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
                                    this.Property.SetValue(this.DataSource, Convert.ChangeType(newValue, this.Property.PropertyType, CultureInfo.InvariantCulture), null);
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
                                if (parentValue is IDictionary<string, string> parentDict)
                                {
                                    if (parentDict.ContainsKey(dataField) && !ReferenceEquals(newValue, oldValue))
                                    {
                                        parentDict[dataField] = newValue as string;
                                    }
                                }
                                else if (parentValue is IIndexable parentIndexer)
                                {
                                    parentIndexer[dataField] = newValue;
                                }
                                else if (this.ChildProperty != null)
                                {
                                    if (this.Property.PropertyType.IsEnum)
                                    {
                                        this.ChildProperty.SetValue(parentValue, Enum.Parse(this.ChildProperty.PropertyType, newValue.ToString()), null);
                                    }
                                    else
                                    {
                                        this.ChildProperty.SetValue(parentValue, Convert.ChangeType(newValue, this.ChildProperty.PropertyType, CultureInfo.InvariantCulture), null);
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
