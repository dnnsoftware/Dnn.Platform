// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System.Web;

    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// Provides methods to resolve tab and module information from an HTTP request.
    /// </summary>
    public interface ITabAndModuleInfoProvider
    {
        /// <summary>
        /// Attempts to find the tab identifier for the specified request.
        /// </summary>
        /// <param name="request">The current HTTP request.</param>
        /// <param name="tabId">When this method returns, contains the resolved tab identifier if found; otherwise, a default value.</param>
        /// <returns><c>true</c> if the tab identifier was found; otherwise, <c>false</c>.</returns>
        bool TryFindTabId(HttpRequestBase request, out int tabId);

        /// <summary>
        /// Attempts to find the module identifier for the specified request.
        /// </summary>
        /// <param name="request">The current HTTP request.</param>
        /// <param name="moduleId">When this method returns, contains the resolved module identifier if found; otherwise, a default value.</param>
        /// <returns><c>true</c> if the module identifier was found; otherwise, <c>false</c>.</returns>
        bool TryFindModuleId(HttpRequestBase request, out int moduleId);

        /// <summary>
        /// Attempts to find module information for the specified request.
        /// </summary>
        /// <param name="request">The current HTTP request.</param>
        /// <param name="moduleInfo">When this method returns, contains the resolved module information if found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the module information was found; otherwise, <c>false</c>.</returns>
        bool TryFindModuleInfo(HttpRequestBase request, out ModuleInfo moduleInfo);
    }
}
