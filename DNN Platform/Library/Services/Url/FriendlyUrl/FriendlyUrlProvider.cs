#region Copyright
// 
// DotNetNuke® - https://www.dnnsoftware.com
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

using DotNetNuke.Abstractions.Portals;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.Services.Url.FriendlyUrl
{
    public abstract class FriendlyUrlProvider
    {
		#region "Shared/Static Methods"

        //return the provider
        public static FriendlyUrlProvider Instance()
        {
            return ComponentFactory.GetComponent<FriendlyUrlProvider>();
        }
		
		#endregion

		#region "Abstract Methods"

        public abstract string FriendlyUrl(TabInfo tab, string path);

        public abstract string FriendlyUrl(TabInfo tab, string path, string pageName);

        [Obsolete("Deprecated in Platform 9.4.3. Scheduled for removal in v11.0.0. Use the IPortalSettings overload")]
        public virtual string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return this.FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
        }
        
        public abstract string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings);

        public abstract string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias);
		
		#endregion
    }
}