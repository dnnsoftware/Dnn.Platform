using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using System.Globalization;
using DotNetNuke.Common.Utilities;

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
            var settings = SettingsManager.Instance.Get(_portalId, _moduleId);

            string pageSize = Null.NullString;

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
                    propertyNotFound = true;
                    break;
            }

            return pageSize;
        }
    }


}