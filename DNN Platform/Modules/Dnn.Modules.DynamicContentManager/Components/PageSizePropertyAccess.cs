using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Dnn.Modules.DynamicContentManager.Components.Entities;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Personalization;

namespace Dnn.Modules.DynamicContentManager.Components
{
    public class PageSizePropertyAccess : IPropertyAccess
    {
        private readonly int _portalId;
        private readonly int _moduleId;
        public PageSizePropertyAccess(int portalId, int moduleId)
        {
            _portalId = portalId;
            _moduleId = moduleId;
        }

        /// <summary>
        /// Token Cacheability.
        /// </summary>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            //var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            //var personalization = personalizationController.LoadProfile(_userId, _portalId);
            //string profileData = Convert.ToString(personalization.Profile["DCCContentTypePageSize:" + _portalId]);
            var settings = (DCCSettings)Personalization.GetProfile("DCC", "UserSettings" + _portalId + _moduleId);

            var pageSize = "10";

            switch (propertyName)
            {
                case "ContentType":
                    pageSize = settings.ContentTypePageSize.ToString();
                    break;
                case "DataType":
                    pageSize = settings.DataTypePageSize.ToString();
                    break;
                case "Template":
                    pageSize = settings.TemplatePageSize.ToString();
                    break;
                default:
                    pageSize = "10";
                    break;
            }

            return pageSize;
        }
    }


}