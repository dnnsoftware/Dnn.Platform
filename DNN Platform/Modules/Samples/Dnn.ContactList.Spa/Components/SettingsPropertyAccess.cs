// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Text.RegularExpressions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

namespace Dnn.ContactList.Spa.Components
{
    /// <summary>
    /// Implements the Interface IPropertyAccess. This mechanism is used by the Token Replace
    /// Engine implemented to be used in SPA modules. The method 'GetProperty' allows to access to 
    /// information that will be available as properties in the custom token.
    /// </summary>
    public class SettingsPropertyAccess : IPropertyAccess
    {
        private readonly ISettingsService _service;
        private readonly int _moduleId;
        private readonly int _tabId;

        /// <summary>
        /// Default Constructor constructs a new PreloadedDataPropertyAccess
        /// </summary>
        public SettingsPropertyAccess(int moduleId, int tabId) : this(moduleId, tabId, SettingsService.Instance)
        {

        }

        /// <summary>
        /// Constructor constructs a new PreloadedDataPropertyAccess with a passed in service
        /// </summary>
        public SettingsPropertyAccess(int moduleId, int tabId, ISettingsService service)
        {
            Requires.NotNull(service);

            _service = service;
            _tabId = tabId;
            _moduleId = moduleId;
        }

        /// <summary>
        /// Token Cacheability.
        /// </summary>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <summary>
        /// Get Setting Property.
        /// </summary>
        /// <param name="propertyName">property name.</param>
        /// <param name="format">format.</param>
        /// <param name="formatProvider">format provider.</param>
        /// <param name="accessingUser">accessing user.</param>
        /// <param name="accessLevel">access level.</param>
        /// <param name="propertyNotFound">Whether found the property value.</param>
        /// <returns></returns>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            string propertyValue = "";

            switch (propertyName)
            {
                case "IsFormEnabled":
                    propertyValue = _service.IsFormEnabled(_moduleId, _tabId).ToString().ToLower();
                    break;
                case "EmailRegex":
                    propertyValue = Regex.Escape(Globals.glbEmailRegEx);
                    break;
                default:
                    propertyNotFound = true;
                    break;
            }

            return propertyValue;
        }
    }
}
