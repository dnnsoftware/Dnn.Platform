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

using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

// ReSharper disable ConvertPropertyToExpressionBody

namespace DotNetNuke.UI.Modules.Html5
{
    public class ModuleContextPropertyAccess : IPropertyAccess
    {
        private readonly ModuleInstanceContext _moduleContext;

        public ModuleContextPropertyAccess(ModuleInstanceContext moduleContext)
        {
            _moduleContext = moduleContext;
        }

        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLower())
            {
                case "moduleid":
                    return _moduleContext.ModuleId.ToString();
                case "tabmoduleid":
                    return _moduleContext.TabModuleId.ToString();
                case "tabid":
                    return _moduleContext.TabId.ToString();
                case "portalid":
                    return _moduleContext.Configuration.OwnerPortalID.ToString();
                case "issuperuser":
                    return _moduleContext.PortalSettings.UserInfo.IsSuperUser.ToString();
                case "editmode":
                    return _moduleContext.EditMode.ToString();
                default:
                    if (_moduleContext.Settings.ContainsKey(propertyName))
                    {
                        return (string)_moduleContext.Settings[propertyName];
                    }
                    break;
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
