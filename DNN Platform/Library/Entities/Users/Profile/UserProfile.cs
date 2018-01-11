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


using DotNetNuke.Common.Internal;

#region Usings

using System;
using System.ComponentModel;
using System.Globalization;

using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Services.FileSystem;

using System.Xml.Serialization;
using DotNetNuke.Common.Lists;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Users
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The UserProfile class provides a Business Layer entity for the Users Profile
    /// </summary>
    [Serializable]
    public class UserProfile : IIndexable
    {
        #region Public Constants

        //Name properties
        public const string USERPROFILE_FirstName = "FirstName";
        public const string USERPROFILE_LastName = "LastName";
        public const string USERPROFILE_Title = "Title";

        //Address Properties
        public const string USERPROFILE_Unit = "Unit";
        public const string USERPROFILE_Street = "Street";
        public const string USERPROFILE_City = "City";
        public const string USERPROFILE_Region = "Region";
        public const string USERPROFILE_Country = "Country";
        public const string USERPROFILE_PostalCode = "PostalCode";

        //Phone contact
        public const string USERPROFILE_Telephone = "Telephone";
        public const string USERPROFILE_Cell = "Cell";
        public const string USERPROFILE_Fax = "Fax";

        //Online contact
        public const string USERPROFILE_Website = "Website";
        public const string USERPROFILE_IM = "IM";

        //Preferences
        public const string USERPROFILE_Photo = "Photo";
        public const string USERPROFILE_TimeZone = "TimeZone";
        public const string USERPROFILE_PreferredLocale = "PreferredLocale";
        public const string USERPROFILE_PreferredTimeZone = "PreferredTimeZone";
        public const string USERPROFILE_Biography = "Biography";

        #endregion

        #region Private Members

        private bool _IsDirty;

        private UserInfo _user;

        //collection to store all profile properties.
        private ProfilePropertyDefinitionCollection _profileProperties;

        #endregion

        public UserProfile()
        {
        }

        public UserProfile(UserInfo user)
        {
            _user = user;
        }

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Cell/Mobile Phone
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Cell
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Cell);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Cell, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the City part of the Address
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string City
        {
            get
            {
                return GetPropertyValue(USERPROFILE_City);
            }
            set
            {
                SetProfileProperty(USERPROFILE_City, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Country part of the Address
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Country
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Country);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Country, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Fax Phone
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Fax
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Fax);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Fax, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the First Name
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string FirstName
        {
            get
            {
                return GetPropertyValue(USERPROFILE_FirstName);
            }
            set
            {
                SetProfileProperty(USERPROFILE_FirstName, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Full Name
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Instant Messenger Handle
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string IM
        {
            get
            {
                return GetPropertyValue(USERPROFILE_IM);
            }
            set
            {
                SetProfileProperty(USERPROFILE_IM, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether the property has been changed
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsDirty
        {
            get
            {
                return _IsDirty;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Last Name
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string LastName
        {
            get
            {
                return GetPropertyValue(USERPROFILE_LastName);
            }
            set
            {
                SetProfileProperty(USERPROFILE_LastName, value);
            }
        }

        public string Photo
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Photo);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Photo, value);
            }
        }

        /// <summary>
        /// property will return a URL for the photourl - if the path contains invalid url characters it will return a fileticket
        /// </summary>
        public string PhotoURL
        {
            get
            {
                string photoURL = Globals.ApplicationPath + "/images/no_avatar.gif";
                ProfilePropertyDefinition photoProperty = GetProperty(USERPROFILE_Photo);
                if ((photoProperty != null))
                {
                    UserInfo user = UserController.Instance.GetCurrentUserInfo();
                    PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();

	                bool isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, _user);
                    if (!string.IsNullOrEmpty(photoProperty.PropertyValue) && isVisible)
                    {
                        var fileInfo = FileManager.Instance.GetFile(int.Parse(photoProperty.PropertyValue));
                        if ((fileInfo != null))
                        {
                            photoURL = FileManager.Instance.GetUrl(fileInfo);
                        }
                    }
                }
                return photoURL;
            }
        }

        /// <summary>
        /// property will return the file path of the photo url (designed to be used when files are loaded via the filesystem e.g for caching)
        /// </summary>
        [Obsolete("Obsolete in 7.2.2, Use PhotoUrl instead of it.")]
        public string PhotoURLFile
        {
            get
            {
                string photoURLFile = Globals.ApplicationPath + "/images/no_avatar.gif";
                ProfilePropertyDefinition photoProperty = GetProperty(USERPROFILE_Photo);
                if ((photoProperty != null))
                {
                    UserInfo user = UserController.Instance.GetCurrentUserInfo();
                    PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();

                    bool isVisible = (user.UserID == _user.UserID);
                    if (!isVisible)
                    {
                        switch (photoProperty.ProfileVisibility.VisibilityMode)
                        {
                            case UserVisibilityMode.AllUsers:
                                isVisible = true;
                                break;
                            case UserVisibilityMode.MembersOnly:
                                isVisible = user.UserID > 0;
                                break;
                            case UserVisibilityMode.AdminOnly:
                                isVisible = user.IsInRole(settings.AdministratorRoleName);
                                break;
                            case UserVisibilityMode.FriendsAndGroups:
                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(photoProperty.PropertyValue) && isVisible)
                    {
                        var fileInfo = FileManager.Instance.GetFile(int.Parse(photoProperty.PropertyValue));
                        if ((fileInfo != null))
                        {
                            string rootFolder = "";
                            if (fileInfo.PortalId == Null.NullInteger)
                            {
                                //Host
                                rootFolder = Globals.HostPath;
                            }
                            else
                            {
                                rootFolder = settings.HomeDirectory;
                            }
                            photoURLFile = TestableGlobals.Instance.ResolveUrl(rootFolder + fileInfo.Folder + fileInfo.FileName);
                        }
                    }
                }
                return photoURLFile;
            }
        }
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the PostalCode part of the Address
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PostalCode
        {
            get
            {
                return GetPropertyValue(USERPROFILE_PostalCode);
            }
            set
            {
                SetProfileProperty(USERPROFILE_PostalCode, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Preferred Locale
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PreferredLocale
        {
            get
            {
                return GetPropertyValue(USERPROFILE_PreferredLocale);
            }
            set
            {
                SetProfileProperty(USERPROFILE_PreferredLocale, value);
            }
        }

        [XmlIgnore]
        public TimeZoneInfo PreferredTimeZone
        {
            get
            {
                //First set to Server
                TimeZoneInfo _TimeZone = TimeZoneInfo.Local;

                //Next check if there is a Property Setting
                string _TimeZoneId = GetPropertyValue(USERPROFILE_PreferredTimeZone);
                if (!string.IsNullOrEmpty(_TimeZoneId))
                {
                    _TimeZone = TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId);
                }
                //Next check if there is a Portal Setting
                else
                {
                    PortalSettings _PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    if (_PortalSettings != null)
                    {
                        _TimeZone = _PortalSettings.TimeZone;
                    }
                }

                //still we can't find it or it's somehow set to null
                return _TimeZone ?? (TimeZoneInfo.Local);
            }
            set
            {
                if (value != null)
                {
                    SetProfileProperty(USERPROFILE_PreferredTimeZone, value.Id);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Collection of Profile Properties
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection ProfileProperties
        {
            get { return _profileProperties ?? (_profileProperties = new ProfilePropertyDefinitionCollection()); }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Region part of the Address
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Region
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Region);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Region, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Street part of the Address
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Street
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Street);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Street, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Telephone
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Telephone
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Telephone);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Telephone, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Title
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Title
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Title);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Title, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Unit part of the Address
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Unit
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Unit);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Unit, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Website
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Website
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Website);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Website, value);
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
            _IsDirty = false;
            foreach (ProfilePropertyDefinition profProperty in ProfileProperties)
            {
                profProperty?.ClearIsDirty();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Profile Property from the Profile
        /// </summary>
        /// <remarks></remarks>
        /// <param name="propName">The name of the property to retrieve.</param>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition GetProperty(string propName)
        {
            return ProfileProperties[propName];
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Profile Property Value from the Profile
        /// </summary>
        /// <remarks></remarks>
        /// <param name="propName">The name of the propoerty to retrieve.</param>
        /// -----------------------------------------------------------------------------
        public string GetPropertyValue(string propName)
        {
            string propValue = Null.NullString;
            ProfilePropertyDefinition profileProp = GetProperty(propName);
            if (profileProp != null)
            {
                propValue = profileProp.PropertyValue;

                if (profileProp.DataType > -1)
                {
                    var controller = new ListController();
                    var dataType = controller.GetListEntryInfo("DataType", profileProp.DataType);
                    if (dataType.Value == "Country" || dataType.Value == "Region")
                    {
                        propValue = GetListValue(dataType.Value, propValue);
                    }
                }
            }
            return propValue;
        }

        private string GetListValue(string listName, string value)
        {
            ListController lc = new ListController();
            int entryId;
            if (int.TryParse(value, out entryId))
            {
                ListEntryInfo item = lc.GetListEntryInfo(listName, entryId);
                if (item != null)
                {
                    return item.Text;
                }
            }
            return value;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialises the Profile with an empty collection of profile properties
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The name of the property to retrieve.</param>
        /// -----------------------------------------------------------------------------
        public void InitialiseProfile(int portalId)
        {
            InitialiseProfile(portalId, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialises the Profile with an empty collection of profile properties
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The name of the property to retrieve.</param>
        /// <param name="useDefaults">A flag that indicates whether the profile default values should be
        /// copied to the Profile.</param>
        /// -----------------------------------------------------------------------------
        public void InitialiseProfile(int portalId, bool useDefaults)
        {
            _profileProperties = ProfileController.GetPropertyDefinitionsByPortal(portalId, true, false);
            if (useDefaults)
            {
                foreach (ProfilePropertyDefinition ProfileProperty in _profileProperties)
                {
                    ProfileProperty.PropertyValue = ProfileProperty.DefaultValue;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets a Profile Property Value in the Profile
        /// </summary>
        /// <remarks></remarks>
        /// <param name="propName">The name of the propoerty to set.</param>
        /// <param name="propValue">The value of the propoerty to set.</param>
        /// -----------------------------------------------------------------------------
        public void SetProfileProperty(string propName, string propValue)
        {
            ProfilePropertyDefinition profileProp = GetProperty(propName);
            if (profileProp != null)
            {
                profileProp.PropertyValue = propValue;

                //Set the IsDirty flag
                if (profileProp.IsDirty)
                {
                    _IsDirty = true;
                }
            }
        }

        #endregion

        public string Biography
        {
            get
            {
                return GetPropertyValue(USERPROFILE_Biography);
            }
            set
            {
                SetProfileProperty(USERPROFILE_Biography, value);
            }
        }

        #region Implement IIndexable

        public object this[string name]
        {
            get
            {
                return GetPropertyValue(name);
            }
            set
            {
                string stringValue;
                if (value is DateTime)
                {
                    var dateValue = (DateTime)value;
                    stringValue = dateValue.ToString(DateUtils.StandardDateTimeFormat);
                }
				else if (value is TimeZoneInfo)
				{
					var timezoneValue = (TimeZoneInfo)value;
					stringValue = timezoneValue.Id;
				}
                else
                {
                    stringValue = Convert.ToString(value);
                }
                SetProfileProperty(name, stringValue);
            }
        }

        #endregion
    }
}
