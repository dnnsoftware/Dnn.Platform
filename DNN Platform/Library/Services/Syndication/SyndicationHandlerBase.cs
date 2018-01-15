#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
#region Usings

using System;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Services.Syndication
{
    public class SyndicationHandlerBase : GenericRssHttpHandlerBase
    {
        private int _moduleId = Null.NullInteger;

        private int _tabId = Null.NullInteger;

        public PortalSettings Settings
        {
            get
            {
                return Globals.GetPortalSettings();
            }
        }

        public int TabId
        {
            get
            {
                if (_tabId == Null.NullInteger && Request.QueryString["tabid"] != null)
                {
                    if (! Int32.TryParse(Request.QueryString["tabid"], out _tabId))
                    {
                        _tabId = Null.NullInteger;
                    }
                }
                return _tabId;
            }
        }

        public int ModuleId
        {
            get
            {
                if (_moduleId == Null.NullInteger && Request.QueryString["moduleid"] != null)
                {
                    if (! Int32.TryParse(Request.QueryString["moduleid"], out _moduleId))
                    {
                        _moduleId = Null.NullInteger;
                    }
                }
                return _moduleId;
            }
        }

        public HttpRequest Request
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }
    }
}