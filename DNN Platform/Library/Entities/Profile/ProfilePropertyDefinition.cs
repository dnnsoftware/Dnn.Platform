// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Profile
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfilePropertyDefinition
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfilePropertyDefinition class provides a Business Layer entity for
    /// property Definitions.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [XmlRoot("profiledefinition", IsNullable = false)]
    [Serializable]
    public class ProfilePropertyDefinition : BaseEntityInfo
    {
        private int _dataType = Null.NullInteger;
        private string _defaultValue;
        private UserVisibilityMode _defaultVisibility = UserVisibilityMode.AdminOnly;
        private bool _deleted;
        private int _length;
        private int _moduleDefId = Null.NullInteger;
        private int _portalId;

        private ProfileVisibility _profileVisibility = new ProfileVisibility
        {
            VisibilityMode = UserVisibilityMode.AdminOnly,
        };

        private string _propertyCategory;
        private string _propertyName;
        private string _propertyValue;
        private bool _readOnly;
        private bool _required;
        private string _ValidationExpression;
        private int _viewOrder;
        private bool _visible;

        public ProfilePropertyDefinition()
        {
            this.PropertyDefinitionId = Null.NullInteger;

            // Get the default PortalSettings
            PortalSettings _Settings = PortalController.Instance.GetCurrentPortalSettings();
            this.PortalId = _Settings.PortalId;
        }

        public ProfilePropertyDefinition(int portalId)
        {
            this.PropertyDefinitionId = Null.NullInteger;
            this.PortalId = portalId;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Data Type of the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Editor("DotNetNuke.UI.WebControls.DNNListEditControl, DotNetNuke", typeof(EditControl))]
        [List("DataType", "", ListBoundField.Id, ListBoundField.Value)]
        [IsReadOnly(true)]
        [Required(true)]
        [SortOrder(1)]
        [XmlIgnore]
        public int DataType
        {
            get
            {
                return this._dataType;
            }

            set
            {
                if (this._dataType != value)
                {
                    this.IsDirty = true;
                }

                this._dataType = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Default Value of the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(4)]
        [XmlIgnore]
        public string DefaultValue
        {
            get
            {
                return this._defaultValue;
            }

            set
            {
                if (this._defaultValue != value)
                {
                    this.IsDirty = true;
                }

                this._defaultValue = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets and sets the Default Visibility of the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(10)]
        [XmlIgnore]
        public UserVisibilityMode DefaultVisibility
        {
            get
            {
                return this._defaultVisibility;
            }

            set
            {
                if (this._defaultVisibility != value)
                {
                    this.IsDirty = true;
                }

                this._defaultVisibility = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets the Deleted.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public bool Deleted
        {
            get
            {
                return this._deleted;
            }

            set
            {
                this._deleted = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the Definition has been modified since it has been retrieved.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public bool IsDirty { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Length of the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(3)]
        [XmlElement("length")]
        public int Length
        {
            get
            {
                return this._length;
            }

            set
            {
                if (this._length != value)
                {
                    this.IsDirty = true;
                }

                this._length = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the ModuleDefId.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public int ModuleDefId
        {
            get
            {
                return this._moduleDefId;
            }

            set
            {
                this._moduleDefId = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the PortalId.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public int PortalId
        {
            get
            {
                return this._portalId;
            }

            set
            {
                this._portalId = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Category of the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Required(true)]
        [SortOrder(2)]
        [XmlElement("propertycategory")]
        public string PropertyCategory
        {
            get
            {
                return this._propertyCategory;
            }

            set
            {
                if (this._propertyCategory != value)
                {
                    this.IsDirty = true;
                }

                this._propertyCategory = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Id of the ProfilePropertyDefinition.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public int PropertyDefinitionId { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Name of the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Required(true)]
        [IsReadOnly(true)]
        [SortOrder(0)]
        [RegularExpressionValidator("^[a-zA-Z0-9._%\\-+']+$")]
        [XmlElement("propertyname")]
        public string PropertyName
        {
            get
            {
                return this._propertyName;
            }

            set
            {
                if (this._propertyName != value)
                {
                    this.IsDirty = true;
                }

                this._propertyName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Value of the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public string PropertyValue
        {
            get
            {
                return this._propertyValue;
            }

            set
            {
                if (this._propertyValue != value)
                {
                    this.IsDirty = true;
                }

                this._propertyValue = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the property is read only.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(7)]
        [XmlIgnore]
        public bool ReadOnly
        {
            get
            {
                return this._readOnly;
            }

            set
            {
                if (this._readOnly != value)
                {
                    this.IsDirty = true;
                }

                this._readOnly = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the property is required.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(6)]
        [XmlIgnore]
        public bool Required
        {
            get
            {
                return this._required;
            }

            set
            {
                if (this._required != value)
                {
                    this.IsDirty = true;
                }

                this._required = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets a Validation Expression (RegEx) for the Profile Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(5)]
        [XmlIgnore]
        public string ValidationExpression
        {
            get
            {
                return this._ValidationExpression;
            }

            set
            {
                if (this._ValidationExpression != value)
                {
                    this.IsDirty = true;
                }

                this._ValidationExpression = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the View Order of the Property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [IsReadOnly(true)]
        [SortOrder(9)]
        [XmlIgnore]
        public int ViewOrder
        {
            get
            {
                return this._viewOrder;
            }

            set
            {
                if (this._viewOrder != value)
                {
                    this.IsDirty = true;
                }

                this._viewOrder = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the property is visible.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(8)]
        [XmlIgnore]
        public bool Visible
        {
            get
            {
                return this._visible;
            }

            set
            {
                if (this._visible != value)
                {
                    this.IsDirty = true;
                }

                this._visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets whether the property is visible.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public ProfileVisibility ProfileVisibility
        {
            get
            {
                return this._profileVisibility;
            }

            set
            {
                if (this._profileVisibility != value)
                {
                    this.IsDirty = true;
                }

                this._profileVisibility = value;
            }
        }

        [Obsolete("Deprecated in 6.2 as profile visibility has been extended, keep for compatible with upgrade.. Scheduled removal in v10.0.0.")]
        [Browsable(false)]
        [XmlIgnore]
        public UserVisibilityMode Visibility
        {
            get
            {
                return this.ProfileVisibility.VisibilityMode;
            }

            set
            {
                if (this.ProfileVisibility.VisibilityMode != value)
                {
                    this.IsDirty = true;
                }

                this.ProfileVisibility.VisibilityMode = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clears the IsDirty Flag.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ClearIsDirty()
        {
            this.IsDirty = false;
        }

        /// <summary>
        /// Clone a ProfilePropertyDefinition.
        /// </summary>
        /// <returns>A ProfilePropertyDefinition.</returns>
        public ProfilePropertyDefinition Clone()
        {
            var clone = new ProfilePropertyDefinition(this.PortalId)
            {
                DataType = this.DataType,
                DefaultValue = this.DefaultValue,
                Length = this.Length,
                ModuleDefId = this.ModuleDefId,
                PropertyCategory = this.PropertyCategory,
                PropertyDefinitionId = this.PropertyDefinitionId,
                PropertyName = this.PropertyName,
                PropertyValue = this.PropertyValue,
                ReadOnly = this.ReadOnly,
                Required = this.Required,
                ValidationExpression = this.ValidationExpression,
                ViewOrder = this.ViewOrder,
                DefaultVisibility = this.DefaultVisibility,
                ProfileVisibility = this.ProfileVisibility.Clone(),
                Visible = this.Visible,
                Deleted = this.Deleted,
            };
            clone.ClearIsDirty();
            return clone;
        }
    }
}
