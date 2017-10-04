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

using System;
using System.ComponentModel;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Entities.Profile
{
	/// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfilePropertyDefinition
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfilePropertyDefinition class provides a Business Layer entity for 
    /// property Definitions
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [XmlRoot("profiledefinition", IsNullable = false)]
    [Serializable]
    public class ProfilePropertyDefinition : BaseEntityInfo
    {
        #region Private Members

        private int _dataType = Null.NullInteger;
        private string _defaultValue;
        private UserVisibilityMode _defaultVisibility = UserVisibilityMode.AdminOnly;
        private bool _deleted;
	    private int _length;
        private int _moduleDefId = Null.NullInteger;
        private int _portalId;
        private ProfileVisibility _profileVisibility = new ProfileVisibility
                                                            {
                                                                VisibilityMode = UserVisibilityMode.AdminOnly
                                                            };
        private string _propertyCategory;
	    private string _propertyName;
        private string _propertyValue;
        private bool _readOnly;
        private bool _required;
        private string _ValidationExpression;
        private int _viewOrder;
        private bool _visible;

        #endregion

        #region Constructors

        public ProfilePropertyDefinition()
        {
            PropertyDefinitionId = Null.NullInteger;
            //Get the default PortalSettings
            PortalSettings _Settings = PortalController.Instance.GetCurrentPortalSettings();
            PortalId = _Settings.PortalId;
        }

        public ProfilePropertyDefinition(int portalId)
        {
            PropertyDefinitionId = Null.NullInteger;
            PortalId = portalId;
        }

	    #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Data Type of the Profile Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Editor("DotNetNuke.UI.WebControls.DNNListEditControl, DotNetNuke", typeof (EditControl))]
        [List("DataType", "", ListBoundField.Id, ListBoundField.Value)]
        [IsReadOnly(true)]
        [Required(true)]
        [SortOrder(1)]
        [XmlIgnore]
        public int DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                if (_dataType != value)
                {
                    IsDirty = true;
                }
                _dataType = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Default Value of the Profile Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(4)]
        [XmlIgnore]
        public string DefaultValue
        {
            get
            {
                return _defaultValue;
            }
            set
            {
                if (_defaultValue != value)
                {
                    IsDirty = true;
                }
                _defaultValue = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets and sets the Default Visibility of the Profile Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(10)]
        [XmlIgnore]
        public UserVisibilityMode DefaultVisibility
        {
            get
            {
                return _defaultVisibility;
            }
            set
            {
                if (_defaultVisibility != value)
                {
                    IsDirty = true;
                }
                _defaultVisibility = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Deleted
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public bool Deleted
        {
            get
            {
                return _deleted;
            }
            set
            {
                _deleted = value;
            }
        }

	    /// -----------------------------------------------------------------------------
	    /// <summary>
	    /// Gets whether the Definition has been modified since it has been retrieved
	    /// </summary>
	    /// -----------------------------------------------------------------------------
	    [Browsable(false)]
        [XmlIgnore]
	    public bool IsDirty { get; private set; }

	    /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Length of the Profile Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(3)]
        [XmlElement("length")]
        public int Length
        {
            get
            {
                return _length;
            }
            set
            {
                if (_length != value)
                {
                    IsDirty = true;
                }
                _length = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ModuleDefId
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public int ModuleDefId
        {
            get
            {
                return _moduleDefId;
            }
            set
            {
                _moduleDefId = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the PortalId
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public int PortalId
        {
            get
            {
                return _portalId;
            }
            set
            {
                _portalId = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Category of the Profile Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Required(true)]
        [SortOrder(2)]
        [XmlElement("propertycategory")]
        public string PropertyCategory
        {
            get
            {
                return _propertyCategory;
            }
            set
            {
                if (_propertyCategory != value)
                {
                    IsDirty = true;
                }
                _propertyCategory = value;
            }
        }

	    /// -----------------------------------------------------------------------------
	    /// <summary>
	    /// Gets and sets the Id of the ProfilePropertyDefinition
	    /// </summary>
	    /// -----------------------------------------------------------------------------
	    [Browsable(false)]
        [XmlIgnore]
	    public int PropertyDefinitionId { get; set; }

	    /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Name of the Profile Property
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
                return _propertyName;
            }
            set
            {
                if (_propertyName != value)
                {
                    IsDirty = true;
                }
                _propertyName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Value of the Profile Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public string PropertyValue
        {
            get
            {
                return _propertyValue;
            }
            set
            {
                if (_propertyValue != value)
                {
                    IsDirty = true;
                }
                _propertyValue = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the property is read only
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(7)]
        [XmlIgnore]
        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                if (_readOnly != value)
                {
                    IsDirty = true;
                }
                _readOnly = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the property is required
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(6)]
        [XmlIgnore]
        public bool Required
        {
            get
            {
                return _required;
            }
            set
            {
                if (_required != value)
                {
                    IsDirty = true;
                }
                _required = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets a Validation Expression (RegEx) for the Profile Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(5)]
        [XmlIgnore]
        public string ValidationExpression
        {
            get
            {
                return _ValidationExpression;
            }
            set
            {
                if (_ValidationExpression != value)
                {
                    IsDirty = true;
                }
                _ValidationExpression = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the View Order of the Property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [IsReadOnly(true)]
        [SortOrder(9)]
        [XmlIgnore]
        public int ViewOrder
        {
            get
            {
                return _viewOrder;
            }
            set
            {
                if (_viewOrder != value)
                {
                    IsDirty = true;
                }
                _viewOrder = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the property is visible
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(8)]
        [XmlIgnore]
        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (_visible != value)
                {
                    IsDirty = true;
                }
                _visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the property is visible
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public ProfileVisibility ProfileVisibility
	    {
            get
            {
                return _profileVisibility;
            }
            set
            {
                if (_profileVisibility != value)
                {
                    IsDirty = true;
                }
                _profileVisibility = value;
            }	        
	    }

        #endregion

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clears the IsDirty Flag
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ClearIsDirty()
        {
            IsDirty = false;
        }

        /// <summary>
        /// Clone a ProfilePropertyDefinition
        /// </summary>
        /// <returns>A ProfilePropertyDefinition</returns>
        public ProfilePropertyDefinition Clone()
        {
            var clone = new ProfilePropertyDefinition(PortalId)
                            {
                                DataType = DataType,
                                DefaultValue = DefaultValue,
                                Length = Length,
                                ModuleDefId = ModuleDefId,
                                PropertyCategory = PropertyCategory,
                                PropertyDefinitionId = PropertyDefinitionId,
                                PropertyName = PropertyName,
                                PropertyValue = PropertyValue,
                                ReadOnly = ReadOnly,
                                Required = Required,
                                ValidationExpression = ValidationExpression,
                                ViewOrder = ViewOrder,
                                DefaultVisibility = DefaultVisibility,
                                ProfileVisibility = ProfileVisibility.Clone(),
                                Visible = Visible,
                                Deleted = Deleted
                            };
            clone.ClearIsDirty();
            return clone;
        }

        #endregion

        #region Obsolete

        [Obsolete("Deprecated in 6.2 as profile visibility has been extended, keep for compatible with upgrade.")]
        [Browsable(false)]
        [XmlIgnore]
        public UserVisibilityMode Visibility
        {
            get
            {
                return ProfileVisibility.VisibilityMode;
            }
            set
            {
                if (ProfileVisibility.VisibilityMode != value)
                {
                    IsDirty = true;
                }
                ProfileVisibility.VisibilityMode = value;
            }
        }

        #endregion
    }
}
