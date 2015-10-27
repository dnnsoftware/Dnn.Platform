using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Dnn.Modules.DynamicContentManager.Components
{
    /// <summary>
    /// Font Property Access.
    /// </summary>
    public class FontPropertyAccess : IPropertyAccess
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FontPropertyAccess()
        {
        }

        /// <summary>
        /// Token Cacheability.
        /// </summary>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        /// <summary>
        /// Get Font Property.
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
            const string rootName = "dnni dnni-{0}";

            return string.Format(rootName, propertyName.ToLower());
        }
    }
}