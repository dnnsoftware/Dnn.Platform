#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System;
using System.Web;

namespace DotNetNuke.Common.Internal
{
    
    public interface IGlobals
    {
        /// <summary>
        /// Gets the application path.
        /// </summary>
        string ApplicationPath { get; }

        /// <summary>
        /// Gets or sets the host map path.
        /// </summary>
        /// <value>ApplicationMapPath + "Portals\_default\"</value>
        string HostMapPath { get; }

        /// <summary>
        /// Returns the folder path under the root for the portal 
        /// </summary>
        /// <param name="fileNamePath">The folder the absolute path</param>
        /// <param name="portalId">Portal Id.</param>
        string GetSubFolderPath(string fileNamePath, int portalId);

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <returns>Formatted url.</returns>
        string LinkClick(string link, int tabId, int moduleId);

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="Link">The link</param>
        /// <param name="TabID">The Tab ID</param>
        /// <param name="ModuleID">The Module ID</param>
        /// <param name="TrackClicks">Check whether it has to track clicks</param>
        /// <param name="ForceDownload">Check whether it has to force the download</param>
        /// <param name="PortalId">The Portal ID</param>
        /// <param name="EnableUrlLanguage">Check whether the portal has enabled  ulr languages</param>
        /// <param name="portalGuid">The Portal GUID</param>
        /// <returns></returns>
        string LinkClick(string Link, int TabID, int ModuleID, bool TrackClicks, bool ForceDownload, int PortalId,
                         bool EnableUrlLanguage, string portalGuid);

        /// <summary>
        /// Generates the correctly formatted url
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="url">The url to format.</param>
        /// <returns>The formatted (resolved) url</returns>
        string ResolveUrl(string url);

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <param name="requestedUri">The requested Uri</param>
        /// <returns>domain name</returns>
        string GetDomainName(Uri requestedUri);

        /// <summary>
        /// returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost )
        /// </summary>
        /// <param name="requestedUri">The requested Uri</param>
        /// <param name="parsePortNumber">if set to <c>true</c> [parse port number].</param>
        /// <returns>domain name</returns>
        string GetDomainName(Uri requestedUri, bool parsePortNumber);
    }
}