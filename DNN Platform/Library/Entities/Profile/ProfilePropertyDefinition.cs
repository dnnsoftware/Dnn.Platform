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
    using Newtonsoft.Json;

    /// <summary>
    /// The ProfilePropertyDefinition class provides a Business Layer entity for
    /// property Definitions.
    /// </summary>
    [XmlRoot("profiledefinition", IsNullable = false)]
    [Serializable]
    public class ProfilePropertyDefinition : BaseEntityInfo
    {
        private int dataType = Null.NullInteger;
        private string defaultValue;
        private UserVisibilityMode defaultVisibility = UserVisibilityMode.AdminOnly;
        private bool deleted;
        private int length;
        private int moduleDefId = Null.NullInteger;
        private int portalId;

        private ProfileVisibility profileVisibility = new ProfileVisibility
        {
            VisibilityMode = UserVisibilityMode.AdminOnly,
        };

        private string propertyCategory;
        private string propertyName;
        private string propertyValue;
        private bool readOnly;
        private bool required;
        private string validationExpression;
        private int viewOrder;
        private bool visible;

        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinition"/> class.</summary>
        public ProfilePropertyDefinition()
        {
            this.PropertyDefinitionId = Null.NullInteger;

            // Get the default PortalSettings
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            this.PortalId = settings.PortalId;
        }

        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinition"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        public ProfilePropertyDefinition(int portalId)
        {
            this.PropertyDefinitionId = Null.NullInteger;
            this.PortalId = portalId;
        }

        /// <summary>Gets or sets the Data Type of the Profile Property.</summary>
        [Editor("DotNetNuke.UI.WebControls.DNNListEditControl, DotNetNuke", typeof(EditControl))]
        [List("DataType", "", ListBoundField.Id, ListBoundField.Value)]
        [IsReadOnly(true)]
        [Required(true)]
        [SortOrder(1)]
        [XmlIgnore]
        [JsonIgnore]
        public int DataType
        {
            get
            {
                return this.dataType;
            }

            set
            {
                if (this.dataType != value)
                {
                    this.IsDirty = true;
                }

                this.dataType = value;
            }
        }

        /// <summary>Gets or sets the Default Value of the Profile Property.</summary>
        [SortOrder(4)]
        [XmlIgnore]
        [JsonIgnore]
        public string DefaultValue
        {
            get
            {
                return this.defaultValue;
            }

            set
            {
                if (this.defaultValue != value)
                {
                    this.IsDirty = true;
                }

                this.defaultValue = value;
            }
        }

        /// <summary>  Gets or sets and sets the Default Visibility of the Profile Property.</summary>
        [SortOrder(10)]
        [XmlIgnore]
        [JsonIgnore]
        public UserVisibilityMode DefaultVisibility
        {
            get
            {
                return this.defaultVisibility;
            }

            set
            {
                if (this.defaultVisibility != value)
                {
                    this.IsDirty = true;
                }

                this.defaultVisibility = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the property is Deleted.</summary>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public bool Deleted
        {
            get
            {
                return this.deleted;
            }

            set
            {
                this.deleted = value;
            }
        }

        /// <summary>Gets a value indicating whether the Definition has been modified since it has been retrieved.</summary>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public bool IsDirty { get; private set; }

        /// <summary>Gets or sets the Length of the Profile Property.</summary>
        [SortOrder(3)]
        [XmlElement("length")]
        public int Length
        {
            get
            {
                return this.length;
            }

            set
            {
                if (this.length != value)
                {
                    this.IsDirty = true;
                }

                this.length = value;
            }
        }

        /// <summary>Gets or sets the ModuleDefId.</summary>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public int ModuleDefId
        {
            get
            {
                return this.moduleDefId;
            }

            set
            {
                this.moduleDefId = value;
            }
        }

        /// <summary>Gets or sets the PortalId.</summary>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public int PortalId
        {
            get
            {
                return this.portalId;
            }

            set
            {
                this.portalId = value;
            }
        }

        /// <summary>Gets or sets the Category of the Profile Property.</summary>
        [Required(true)]
        [SortOrder(2)]
        [XmlElement("propertycategory")]
        public string PropertyCategory
        {
            get
            {
                return this.propertyCategory;
            }

            set
            {
                if (this.propertyCategory != value)
                {
                    this.IsDirty = true;
                }

                this.propertyCategory = value;
            }
        }

        /// <summary>Gets or sets the Id of the ProfilePropertyDefinition.</summary>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public int PropertyDefinitionId { get; set; }

        /// <summary>Gets or sets the Name of the Profile Property.</summary>
        [Required(true)]
        [IsReadOnly(true)]
        [SortOrder(0)]
        [RegularExpressionValidator("^[a-zA-Z0-9._%\\-+']+$")]
        [XmlElement("propertyname")]
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }

            set
            {
                if (this.propertyName != value)
                {
                    this.IsDirty = true;
                }

                this.propertyName = value;
            }
        }

        /// <summary>Gets or sets the Value of the Profile Property.</summary>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public string PropertyValue
        {
            get
            {
                return this.propertyValue;
            }

            set
            {
                if (this.propertyValue != value)
                {
                    this.IsDirty = true;
                }

                this.propertyValue = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the property is read only.</summary>
        [SortOrder(7)]
        [XmlIgnore]
        [JsonIgnore]
        public bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }

            set
            {
                if (this.readOnly != value)
                {
                    this.IsDirty = true;
                }

                this.readOnly = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the property is required.</summary>
        [SortOrder(6)]
        [XmlIgnore]
        [JsonIgnore]
        public bool Required
        {
            get
            {
                return this.required;
            }

            set
            {
                if (this.required != value)
                {
                    this.IsDirty = true;
                }

                this.required = value;
            }
        }

        /// <summary>Gets or sets a Validation Expression (RegEx) for the Profile Property.</summary>
        [SortOrder(5)]
        [XmlIgnore]
        [JsonIgnore]
        public string ValidationExpression
        {
            get
            {
                return this.validationExpression;
            }

            set
            {
                if (this.validationExpression != value)
                {
                    this.IsDirty = true;
                }

                this.validationExpression = value;
            }
        }

        /// <summary>Gets or sets the View Order of the Property.</summary>
        [IsReadOnly(true)]
        [SortOrder(9)]
        [XmlIgnore]
        [JsonIgnore]
        public int ViewOrder
        {
            get
            {
                return this.viewOrder;
            }

            set
            {
                if (this.viewOrder != value)
                {
                    this.IsDirty = true;
                }

                this.viewOrder = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the property is visible.</summary>
        [SortOrder(8)]
        [XmlIgnore]
        [JsonIgnore]
        public bool Visible
        {
            get
            {
                return this.visible;
            }

            set
            {
                if (this.visible != value)
                {
                    this.IsDirty = true;
                }

                this.visible = value;
            }
        }

        /// <summary>Gets or sets whether the property is visible.</summary>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public ProfileVisibility ProfileVisibility
        {
            get
            {
                return this.profileVisibility;
            }

            set
            {
                if (this.profileVisibility != value)
                {
                    this.IsDirty = true;
                }

                this.profileVisibility = value;
            }
        }

        /// <summary>Clears the IsDirty Flag.</summary>
        public void ClearIsDirty()
        {
            this.IsDirty = false;
        }

        /// <summary>Clone a ProfilePropertyDefinition.</summary>
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
