// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Users

// ReSharper restore CheckNamespace
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Xml.Serialization;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Services.FileSystem;

    /// <summary>
    /// The UserProfile class provides a Business Layer entity for the Users Profile.
    /// </summary>
    [Serializable]
    public class UserProfile : IIndexable
    {
        // Name properties
        public const string USERPROFILE_FirstName = "FirstName";
        public const string USERPROFILE_LastName = "LastName";
        public const string USERPROFILE_Title = "Title";

        // Address Properties
        public const string USERPROFILE_Unit = "Unit";
        public const string USERPROFILE_Street = "Street";
        public const string USERPROFILE_City = "City";
        public const string USERPROFILE_Country = "Country";
        public const string USERPROFILE_Region = "Region";
        public const string USERPROFILE_PostalCode = "PostalCode";

        // Phone contact
        public const string USERPROFILE_Telephone = "Telephone";
        public const string USERPROFILE_Cell = "Cell";
        public const string USERPROFILE_Fax = "Fax";

        // Online contact
        public const string USERPROFILE_Website = "Website";
        public const string USERPROFILE_IM = "IM";

        // Preferences
        public const string USERPROFILE_Photo = "Photo";
        public const string USERPROFILE_TimeZone = "TimeZone";
        public const string USERPROFILE_PreferredLocale = "PreferredLocale";
        public const string USERPROFILE_PreferredTimeZone = "PreferredTimeZone";
        public const string USERPROFILE_Biography = "Biography";
        private bool _IsDirty;

        private UserInfo _user;

        // collection to store all profile properties.
        private ProfilePropertyDefinitionCollection _profileProperties;

        public UserProfile()
        {
        }

        public UserProfile(UserInfo user)
        {
            this._user = user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Full Name.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string FullName
        {
            get
            {
                return this.FirstName + " " + this.LastName;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets or sets whether the property has been changed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsDirty
        {
            get
            {
                return this._IsDirty;
            }
        }

        /// <summary>
        /// Gets property will return a URL for the photourl - if the path contains invalid url characters it will return a fileticket.
        /// </summary>
        public string PhotoURL
        {
            get
            {
                string photoURL = Globals.ApplicationPath + "/images/no_avatar.gif";
                ProfilePropertyDefinition photoProperty = this.GetProperty(USERPROFILE_Photo);
                if (photoProperty != null)
                {
                    UserInfo user = UserController.Instance.GetCurrentUserInfo();
                    PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();

                    bool isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, this._user);
                    if (!string.IsNullOrEmpty(photoProperty.PropertyValue) && isVisible)
                    {
                        var fileInfo = FileManager.Instance.GetFile(int.Parse(photoProperty.PropertyValue));
                        if (fileInfo != null)
                        {
                            photoURL = FileManager.Instance.GetUrl(fileInfo);
                        }
                    }
                }

                return photoURL;
            }
        }

        /// <summary>
        /// Gets property will return the file path of the photo url (designed to be used when files are loaded via the filesystem e.g for caching).
        /// </summary>
        [Obsolete("Obsolete in 7.2.2, Use PhotoUrl instead of it.. Scheduled removal in v10.0.0.")]
        public string PhotoURLFile
        {
            get
            {
                string photoURLFile = Globals.ApplicationPath + "/images/no_avatar.gif";
                ProfilePropertyDefinition photoProperty = this.GetProperty(USERPROFILE_Photo);
                if (photoProperty != null)
                {
                    UserInfo user = UserController.Instance.GetCurrentUserInfo();
                    PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();

                    bool isVisible = user.UserID == this._user.UserID;
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
                        if (fileInfo != null)
                        {
                            string rootFolder = string.Empty;
                            if (fileInfo.PortalId == Null.NullInteger)
                            {
                                // Host
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
        /// Gets and sets the Collection of Profile Properties.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection ProfileProperties
        {
            get { return this._profileProperties ?? (this._profileProperties = new ProfilePropertyDefinitionCollection()); }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Cell/Mobile Phone.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Cell
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Cell);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Cell, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the City part of the Address.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string City
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_City);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_City, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Country part of the Address.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Country
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Country);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Country, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Fax Phone.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Fax
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Fax);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Fax, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the First Name.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string FirstName
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_FirstName);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_FirstName, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Instant Messenger Handle.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string IM
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_IM);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_IM, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Last Name.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string LastName
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_LastName);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_LastName, value);
            }
        }

        public string Photo
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Photo);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Photo, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the PostalCode part of the Address.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PostalCode
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_PostalCode);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_PostalCode, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Preferred Locale.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PreferredLocale
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_PreferredLocale);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_PreferredLocale, value);
            }
        }

        [XmlIgnore]
        public TimeZoneInfo PreferredTimeZone
        {
            get
            {
                // First set to Server
                TimeZoneInfo _TimeZone = TimeZoneInfo.Local;

                // Next check if there is a Property Setting
                string _TimeZoneId = this.GetPropertyValue(USERPROFILE_PreferredTimeZone);
                if (!string.IsNullOrEmpty(_TimeZoneId))
                {
                    _TimeZone = TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId);
                }

                // Next check if there is a Portal Setting
                else
                {
                    PortalSettings _PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    if (_PortalSettings != null)
                    {
                        _TimeZone = _PortalSettings.TimeZone;
                    }
                }

                // still we can't find it or it's somehow set to null
                return _TimeZone ?? TimeZoneInfo.Local;
            }

            set
            {
                if (value != null)
                {
                    this.SetProfileProperty(USERPROFILE_PreferredTimeZone, value.Id);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Region part of the Address.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Region
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Region);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Region, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Street part of the Address.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Street
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Street);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Street, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Telephone.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Telephone
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Telephone);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Telephone, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Title.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Title
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Title);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Title, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Unit part of the Address.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Unit
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Unit);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Unit, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Website.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Website
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Website);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Website, value);
            }
        }

        public string Biography
        {
            get
            {
                return this.GetPropertyValue(USERPROFILE_Biography);
            }

            set
            {
                this.SetProfileProperty(USERPROFILE_Biography, value);
            }
        }

        public object this[string name]
        {
            get
            {
                return this.GetPropertyValue(name);
            }

            set
            {
                string stringValue;
                if (value is DateTime)
                {
                    var dateValue = (DateTime)value;
                    stringValue = dateValue.ToString(CultureInfo.InvariantCulture);
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

                this.SetProfileProperty(name, stringValue);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clears the IsDirty Flag.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ClearIsDirty()
        {
            this._IsDirty = false;
            foreach (ProfilePropertyDefinition profProperty in this.ProfileProperties)
            {
                profProperty?.ClearIsDirty();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Profile Property from the Profile.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="propName">The name of the property to retrieve.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition GetProperty(string propName)
        {
            return this.ProfileProperties[propName];
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Profile Property Value from the Profile.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="propName">The name of the propoerty to retrieve.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public string GetPropertyValue(string propName)
        {
            string propValue = Null.NullString;
            ProfilePropertyDefinition profileProp = this.GetProperty(propName);
            if (profileProp != null)
            {
                propValue = profileProp.PropertyValue;

                if (profileProp.DataType > -1)
                {
                    var controller = new ListController();
                    var dataType = controller.GetListEntryInfo("DataType", profileProp.DataType);
                    if (dataType.Value == "Country" || dataType.Value == "Region")
                    {
                        propValue = this.GetListValue(dataType.Value, propValue);
                    }
                }
            }

            return propValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialises the Profile with an empty collection of profile properties.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The name of the property to retrieve.</param>
        /// -----------------------------------------------------------------------------
        public void InitialiseProfile(int portalId)
        {
            this.InitialiseProfile(portalId, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialises the Profile with an empty collection of profile properties.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The name of the property to retrieve.</param>
        /// <param name="useDefaults">A flag that indicates whether the profile default values should be
        /// copied to the Profile.</param>
        /// -----------------------------------------------------------------------------
        public void InitialiseProfile(int portalId, bool useDefaults)
        {
            this._profileProperties = ProfileController.GetPropertyDefinitionsByPortal(portalId, true, false);
            if (useDefaults)
            {
                foreach (ProfilePropertyDefinition ProfileProperty in this._profileProperties)
                {
                    ProfileProperty.PropertyValue = ProfileProperty.DefaultValue;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets a Profile Property Value in the Profile.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="propName">The name of the propoerty to set.</param>
        /// <param name="propValue">The value of the propoerty to set.</param>
        /// -----------------------------------------------------------------------------
        public void SetProfileProperty(string propName, string propValue)
        {
            ProfilePropertyDefinition profileProp = this.GetProperty(propName);
            if (profileProp != null)
            {
                profileProp.PropertyValue = propValue;

                // Set the IsDirty flag
                if (profileProp.IsDirty)
                {
                    this._IsDirty = true;
                }
            }
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
    }
}
