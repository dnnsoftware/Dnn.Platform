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

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Users
// ReSharper restore CheckNamespace
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserProfile
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserProfile class provides a Business Layer entity for the Users Profile
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///     [cnurse]	01/31/2006	documented
    ///     [cnurse]    02/10/2006  updated with extensible profile enhancment
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserProfile: IIndexable
    {
		#region Private Constants

        //Name properties
        private const string cFirstName = "FirstName";
        private const string cLastName = "LastName";
        private const string cTitle = "Title";

        //Address Properties
        private const string cUnit = "Unit";
        private const string cStreet = "Street";
        private const string cCity = "City";
        private const string cRegion = "Region";
        private const string cCountry = "Country";
        private const string cPostalCode = "PostalCode";

        //Phone contact
        private const string cTelephone = "Telephone";
        private const string cCell = "Cell";
        private const string cFax = "Fax";

        //Online contact
        private const string cWebsite = "Website";
        private const string cIM = "IM";

        //Preferences
        private const string cPhoto = "Photo";
        private const string cTimeZone = "TimeZone";
        private const string cPreferredLocale = "PreferredLocale";
        private const string cPreferredTimeZone = "PreferredTimeZone";
        private const string cBiography = "Biography";

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
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Cell
        {
            get
            {
                return GetPropertyValue(cCell);
            }
            set
            {
                SetProfileProperty(cCell, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the City part of the Address
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string City
        {
            get
            {
                return GetPropertyValue(cCity);
            }
            set
            {
                SetProfileProperty(cCity, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Country part of the Address
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Country
        {
            get
            {
                return GetPropertyValue(cCountry);
            }
            set
            {
                SetProfileProperty(cCountry, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Fax Phone
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Fax
        {
            get
            {
                return GetPropertyValue(cFax);
            }
            set
            {
                SetProfileProperty(cFax, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the First Name
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string FirstName
        {
            get
            {
                return GetPropertyValue(cFirstName);
            }
            set
            {
                SetProfileProperty(cFirstName, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Full Name
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
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
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string IM
        {
            get
            {
                return GetPropertyValue(cIM);
            }
            set
            {
                SetProfileProperty(cIM, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether the property has been changed
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	created
        /// </history>
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
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string LastName
        {
            get
            {
                return GetPropertyValue(cLastName);
            }
            set
            {
                SetProfileProperty(cLastName, value);
            }
        }

        public string Photo
        {
            get
            {
                return GetPropertyValue(cPhoto);
            }
            set
            {
                SetProfileProperty(cPhoto, value);
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
                ProfilePropertyDefinition photoProperty = GetProperty(cPhoto);
                if ((photoProperty != null))
                {
                    UserInfo user = UserController.GetCurrentUserInfo();
                    PortalSettings settings = PortalController.GetCurrentPortalSettings();

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
                ProfilePropertyDefinition photoProperty = GetProperty(cPhoto);
                if ((photoProperty != null))
                {
                    UserInfo user = UserController.GetCurrentUserInfo();
                    PortalSettings settings = PortalController.GetCurrentPortalSettings();

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
                            string rootFolder="";
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
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string PostalCode
        {
            get
            {
                return GetPropertyValue(cPostalCode);
            }
            set
            {
                SetProfileProperty(cPostalCode, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Preferred Locale
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string PreferredLocale
        {
            get
            {
                return GetPropertyValue(cPreferredLocale);
            }
            set
            {
                SetProfileProperty(cPreferredLocale, value);
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
                string _TimeZoneId = GetPropertyValue(cPreferredTimeZone);
                if (!string.IsNullOrEmpty(_TimeZoneId))
                {
                    _TimeZone = TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId);
                }
                //Next check if there is a Portal Setting
                else
                {
                    PortalSettings _PortalSettings = PortalController.GetCurrentPortalSettings();
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
                    SetProfileProperty(cPreferredTimeZone, value.Id);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Collection of Profile Properties
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        ///     [cnurse]    03/28/2006  Converted to a ProfilePropertyDefinitionCollection
        /// </history>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection ProfileProperties
        {
            get { return _profileProperties ?? (_profileProperties = new ProfilePropertyDefinitionCollection()); }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Region part of the Address
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Region
        {
            get
            {
                return GetPropertyValue(cRegion);
            }
            set
            {
                SetProfileProperty(cRegion, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Street part of the Address
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Street
        {
            get
            {
                return GetPropertyValue(cStreet);
            }
            set
            {
                SetProfileProperty(cStreet, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Telephone
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Telephone
        {
            get
            {
                return GetPropertyValue(cTelephone);
            }
            set
            {
                SetProfileProperty(cTelephone, value);
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
                return GetPropertyValue(cTitle);
            }
            set
            {
                SetProfileProperty(cTitle, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Unit part of the Address
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Unit
        {
            get
            {
                return GetPropertyValue(cUnit);
            }
            set
            {
                SetProfileProperty(cUnit, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Website
        /// </summary>
        /// <history>
        ///     [cnurse]	02/10/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Website
        {
            get
            {
                return GetPropertyValue(cWebsite);
            }
            set
            {
                SetProfileProperty(cWebsite, value);
            }
        }
		
        #endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clears the IsDirty Flag
        /// </summary>
        /// <history>
        ///     [cnurse]	02/29/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void ClearIsDirty()
        {
            _IsDirty = false;
            foreach (ProfilePropertyDefinition profProperty in ProfileProperties)
            {
                profProperty.ClearIsDirty();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Profile Property from the Profile
        /// </summary>
        /// <remarks></remarks>
        /// <param name="propName">The name of the property to retrieve.</param>
        /// <history>
        /// 	[cnurse]	02/13/2006	Created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	02/10/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string GetPropertyValue(string propName)
        {
            string propValue = Null.NullString;
            ProfilePropertyDefinition profileProp = GetProperty(propName);
            if (profileProp != null)
            {
                propValue = profileProp.PropertyValue;
            }
            return propValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialises the Profile with an empty collection of profile properties
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The name of the property to retrieve.</param>
        /// <history>
        /// 	[cnurse]	05/18/2006	Created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	08/04/2006	Created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	02/10/2006	Created
        /// </history>
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

        #region Obsolete

        [Obsolete("Deprecated in DNN 6.0. Replaced by PreferredTimeZone.")]
        [Browsable(false)]    
        public int TimeZone
        {
            get
            {
                Int32 retValue = Null.NullInteger;
                string propValue = GetPropertyValue(cTimeZone);
                if (!string.IsNullOrEmpty(propValue))
                {
                    retValue = int.Parse(propValue);
                }
                return retValue;
            }
            set
            {
                SetProfileProperty(cTimeZone, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public string Biography
        {
            get
            {
                return GetPropertyValue(cBiography);
            }
            set
            {
                SetProfileProperty(cBiography, value);
            }
        }

        #endregion

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
                    stringValue = dateValue.ToString(CultureInfo.InvariantCulture);
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
