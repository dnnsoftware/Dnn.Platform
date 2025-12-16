// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Globalization;
    using System.Xml.Serialization;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using Newtonsoft.Json;

    /// <summary>The UserProfile class provides a Business Layer entity for the Users Profile.</summary>
    [Serializable]
    public class UserProfile : IIndexable
    {
#pragma warning disable SA1310 // Field names should not contain underscore
#pragma warning disable SA1600 // Elements should be documented
        // Name properties
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_FirstName = UserProfileFirstName;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_LastName = UserProfileLastName;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Title = UserProfileTitle;

        // Address Properties
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Unit = UserProfileUnit;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Street = UserProfileStreet;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_City = UserProfileCity;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Country = UserProfileCountry;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Region = UserProfileRegion;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_PostalCode = UserProfilePostalCode;

        // Phone contact
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Telephone = UserProfileTelephone;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Cell = UserProfileCell;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Fax = UserProfileFax;

        // Online contact
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Website = UserProfileWebsite;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_IM = UserProfileIM;

        // Preferences
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Photo = UserProfilePhoto;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_TimeZone = UserProfileTimeZone;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_PreferredLocale = UserProfilePreferredLocale;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_PreferredTimeZone = UserProfilePreferredTimeZone;
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use the properties on this class instead. Scheduled for removal in v11.0.0.")]
        public const string USERPROFILE_Biography = UserProfileBiography;
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore SA1600 // Elements should be documented

        private const string UserProfileFirstName = "FirstName";
        private const string UserProfileLastName = "LastName";
        private const string UserProfileTitle = "Title";
        private const string UserProfileUnit = "Unit";
        private const string UserProfileStreet = "Street";
        private const string UserProfileCity = "City";
        private const string UserProfileCountry = "Country";
        private const string UserProfileRegion = "Region";
        private const string UserProfilePostalCode = "PostalCode";

        // Phone contact
        private const string UserProfileTelephone = "Telephone";
        private const string UserProfileCell = "Cell";
        private const string UserProfileFax = "Fax";

        // Online contact
        private const string UserProfileWebsite = "Website";
        private const string UserProfileIM = "IM";

        // Preferences
        private const string UserProfilePhoto = "Photo";
        private const string UserProfileTimeZone = "TimeZone";
        private const string UserProfilePreferredLocale = "PreferredLocale";
        private const string UserProfilePreferredTimeZone = "PreferredTimeZone";
        private const string UserProfileBiography = "Biography";
        private bool isDirty;

        private UserInfo user;

        // collection to store all profile properties.
        private ProfilePropertyDefinitionCollection profileProperties;

        // Address Properties

        /// <summary>Initializes a new instance of the <see cref="UserProfile"/> class.</summary>
        public UserProfile()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UserProfile"/> class with a proficed existing user.</summary>
        /// <param name="user">The user this profile belongs to.</param>
        public UserProfile(UserInfo user)
        {
            this.user = user;
        }

        /// <summary>Gets the full name by concatenating the first and last name with a space.</summary>
        public string FullName
        {
            get
            {
                return this.FirstName + " " + this.LastName;
            }
        }

        /// <summary>Gets a value indicating whether a property has been changed.</summary>
        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }

        /// <summary>Gets a URL for the profile picture - if the path contains invalid url characters it will return a fileticket.</summary>
        public string PhotoURL
        {
            get
            {
                string photoURL = Globals.ApplicationPath + "/images/no_avatar.gif";
                ProfilePropertyDefinition photoProperty = this.GetProperty(UserProfilePhoto);
                if (photoProperty != null)
                {
                    UserInfo user = UserController.Instance.GetCurrentUserInfo();
                    var settings = PortalController.Instance.GetCurrentSettings();

                    bool isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, this.user);
                    if (!string.IsNullOrEmpty(photoProperty.PropertyValue) && isVisible)
                    {
                        var fileInfo = FileManager.Instance.GetFile(int.Parse(photoProperty.PropertyValue, CultureInfo.InvariantCulture));
                        if (fileInfo != null)
                        {
                            photoURL = FileManager.Instance.GetUrl(fileInfo);
                        }
                    }
                }

                return photoURL;
            }
        }

        /// <summary>Gets the Collection of Profile Properties.</summary>
        public ProfilePropertyDefinitionCollection ProfileProperties
        {
            get { return this.profileProperties ?? (this.profileProperties = new ProfilePropertyDefinitionCollection()); }
        }

        /// <summary>Gets or sets the Cell/Mobile Phone.</summary>
        public string Cell
        {
            get
            {
                return this.GetPropertyValue(UserProfileCell);
            }

            set
            {
                this.SetProfileProperty(UserProfileCell, value);
            }
        }

        /// <summary>Gets or sets the City part of the Address.</summary>
        public string City
        {
            get
            {
                return this.GetPropertyValue(UserProfileCity);
            }

            set
            {
                this.SetProfileProperty(UserProfileCity, value);
            }
        }

        /// <summary>Gets or sets the Country part of the Address.</summary>
        public string Country
        {
            get
            {
                return this.GetPropertyValue(UserProfileCountry);
            }

            set
            {
                this.SetProfileProperty(UserProfileCountry, value);
            }
        }

        /// <summary>Gets or sets the Fax Phone.</summary>
        public string Fax
        {
            get
            {
                return this.GetPropertyValue(UserProfileFax);
            }

            set
            {
                this.SetProfileProperty(UserProfileFax, value);
            }
        }

        /// <summary>Gets or sets the First Name.</summary>
        public string FirstName
        {
            get
            {
                return this.GetPropertyValue(UserProfileFirstName);
            }

            set
            {
                this.SetProfileProperty(UserProfileFirstName, value);
            }
        }

        /// <summary>Gets or sets the Instant Messenger Handle.</summary>
        public string IM
        {
            get
            {
                return this.GetPropertyValue(UserProfileIM);
            }

            set
            {
                this.SetProfileProperty(UserProfileIM, value);
            }
        }

        /// <summary>Gets or sets the Last Name.</summary>
        public string LastName
        {
            get
            {
                return this.GetPropertyValue(UserProfileLastName);
            }

            set
            {
                this.SetProfileProperty(UserProfileLastName, value);
            }
        }

        /// <summary>Gets or sets the path to the profile picture.</summary>
        public string Photo
        {
            get
            {
                return this.GetPropertyValue(UserProfilePhoto);
            }

            set
            {
                this.SetProfileProperty(UserProfilePhoto, value);
            }
        }

        /// <summary>Gets or sets the PostalCode part of the Address.</summary>
        public string PostalCode
        {
            get
            {
                return this.GetPropertyValue(UserProfilePostalCode);
            }

            set
            {
                this.SetProfileProperty(UserProfilePostalCode, value);
            }
        }

        /// <summary>Gets or sets the Preferred Locale.</summary>
        public string PreferredLocale
        {
            get
            {
                return this.GetPropertyValue(UserProfilePreferredLocale);
            }

            set
            {
                this.SetProfileProperty(UserProfilePreferredLocale, value);
            }
        }

        /// <summary>Gets or sets the preferred time zone.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public TimeZoneInfo PreferredTimeZone
        {
            get
            {
                // First set to Server
                TimeZoneInfo timeZone = TimeZoneInfo.Local;

                // Next check if there is a Property Setting
                string timeZoneId = this.GetPropertyValue(UserProfilePreferredTimeZone);
                if (!string.IsNullOrEmpty(timeZoneId))
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }

                // Next check if there is a Portal Setting
                else
                {
                    var portalSettings = PortalController.Instance.GetCurrentSettings();
                    if (portalSettings != null)
                    {
                        timeZone = portalSettings.TimeZone;
                    }
                }

                // still we can't find it or it's somehow set to null
                return timeZone ?? TimeZoneInfo.Local;
            }

            set
            {
                if (value != null)
                {
                    this.SetProfileProperty(UserProfilePreferredTimeZone, value.Id);
                }
            }
        }

        /// <summary>Gets or sets the Region part of the Address.</summary>
        public string Region
        {
            get
            {
                return this.GetPropertyValue(UserProfileRegion);
            }

            set
            {
                this.SetProfileProperty(UserProfileRegion, value);
            }
        }

        /// <summary>Gets or sets the Street part of the Address.</summary>
        public string Street
        {
            get
            {
                return this.GetPropertyValue(UserProfileStreet);
            }

            set
            {
                this.SetProfileProperty(UserProfileStreet, value);
            }
        }

        /// <summary>Gets or sets the telephone number.</summary>
        public string Telephone
        {
            get
            {
                return this.GetPropertyValue(UserProfileTelephone);
            }

            set
            {
                this.SetProfileProperty(UserProfileTelephone, value);
            }
        }

        /// <summary>Gets or sets the Title.</summary>
        public string Title
        {
            get
            {
                return this.GetPropertyValue(UserProfileTitle);
            }

            set
            {
                this.SetProfileProperty(UserProfileTitle, value);
            }
        }

        /// <summary>Gets or sets the Unit part of the Address.</summary>
        public string Unit
        {
            get
            {
                return this.GetPropertyValue(UserProfileUnit);
            }

            set
            {
                this.SetProfileProperty(UserProfileUnit, value);
            }
        }

        /// <summary>Gets or sets the Website.</summary>
        public string Website
        {
            get
            {
                return this.GetPropertyValue(UserProfileWebsite);
            }

            set
            {
                this.SetProfileProperty(UserProfileWebsite, value);
            }
        }

        /// <summary>Gets or sets the biography.</summary>
        public string Biography
        {
            get
            {
                return this.GetPropertyValue(UserProfileBiography);
            }

            set
            {
                this.SetProfileProperty(UserProfileBiography, value);
            }
        }

        /// <inheritdoc/>
        public object this[string name]
        {
            get
            {
                return this.GetPropertyValue(name);
            }

            set
            {
                string stringValue;
                if (value is DateTime dateValue)
                {
                    stringValue = dateValue.ToString(CultureInfo.InvariantCulture);
                }
                else if (value is TimeZoneInfo timezoneValue)
                {
                    stringValue = timezoneValue.Id;
                }
                else
                {
                    stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                }

                this.SetProfileProperty(name, stringValue);
            }
        }

        /// <summary>Clears the IsDirty Flag.</summary>
        public void ClearIsDirty()
        {
            this.isDirty = false;
            foreach (ProfilePropertyDefinition profProperty in this.ProfileProperties)
            {
                profProperty?.ClearIsDirty();
            }
        }

        /// <summary>Gets a Profile Property from the Profile.</summary>
        /// <remarks>
        /// Used mainly for custom profile properties, many default properties are already exposed in this class.
        /// </remarks>
        /// <param name="propName">The name of the property to retrieve.</param>
        /// <returns>A profile property definition, <see cref="ProfilePropertyDefinition"/>.</returns>
        public ProfilePropertyDefinition GetProperty(string propName)
        {
            return this.ProfileProperties[propName];
        }

        /// <summary>Gets a Profile Property Value from the Profile.</summary>
        /// <remarks>
        /// Used mainly for custom profile properties, many default properties are already exposed in this class.
        /// </remarks>
        /// <param name="propName">The name of the property to retrieve.</param>
        /// <returns>A string representing the property value.</returns>
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
                    if (dataType == null)
                    {
                        LoggerSource.Instance.GetLogger(typeof(UserProfile)).ErrorFormat(CultureInfo.InvariantCulture, "Invalid data type {0} for profile property {1}", profileProp.DataType, profileProp.PropertyName);
                        return propValue;
                    }

                    if (dataType.Value is "Country" or "Region")
                    {
                        propValue = GetListValue(dataType.Value, propValue);
                    }
                }
            }

            return propValue;
        }

        /// <summary>Initialises the Profile with a collection of profile properties and their default values.</summary>
        /// <param name="portalId">The id of the portal this profile belongs to.</param>
        public void InitialiseProfile(int portalId)
        {
            this.InitialiseProfile(portalId, true);
        }

        /// <summary>Initialises the Profile with an empty collection of profile properties or default values.</summary>
        /// <param name="portalId">the id of the portal this profile belongs to.</param>
        /// <param name="useDefaults">A flag that indicates whether the profile default values should be
        /// copied to the Profile.</param>
        public void InitialiseProfile(int portalId, bool useDefaults)
        {
            this.profileProperties = ProfileController.GetPropertyDefinitionsByPortal(portalId, true, false);
            if (useDefaults)
            {
                foreach (ProfilePropertyDefinition profileProperty in this.profileProperties)
                {
                    profileProperty.PropertyValue = profileProperty.DefaultValue;
                }
            }
        }

        /// <summary>Sets a Profile Property Value in the Profile.</summary>
        /// <param name="propName">The name of the propoerty to set.</param>
        /// <param name="propValue">The value of the propoerty to set.</param>
        public void SetProfileProperty(string propName, string propValue)
        {
            ProfilePropertyDefinition profileProp = this.GetProperty(propName);
            if (profileProp != null)
            {
                profileProp.PropertyValue = propValue;

                // Set the IsDirty flag
                if (profileProp.IsDirty)
                {
                    this.isDirty = true;
                }
            }
        }

        private static string GetListValue(string listName, string value)
        {
            ListController listController = new ListController();
            int entryId;
            if (int.TryParse(value, out entryId))
            {
                ListEntryInfo item = listController.GetListEntryInfo(listName, entryId);
                if (item != null)
                {
                    return item.Text;
                }
            }

            return value;
        }
    }
}
