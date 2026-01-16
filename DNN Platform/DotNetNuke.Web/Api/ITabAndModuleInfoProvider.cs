// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#nullable enable
namespace DotNetNuke.Web.Api
{
    using System.Net.Http;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>A contract specifying the ability to get tab and module information from a web API request.</summary>
    public interface ITabAndModuleInfoProvider
    {
        /// <summary>Try to get the tab ID associated with the request.</summary>
        /// <param name="request">The web API request.</param>
        /// <param name="tabId">The tab ID if it was found, or <see cref="Null.NullInteger"/>.</param>
        /// <returns>Whether the tab ID was found successfully.</returns>
        bool TryFindTabId(HttpRequestMessage request, out int tabId);

        /// <summary>Try to get the module ID associated with the request.</summary>
        /// <param name="request">The web API request.</param>
        /// <param name="moduleId">The module ID if it was found, or <see cref="Null.NullInteger"/>.</param>
        /// <returns>Whether the module ID was found successfully.</returns>
        bool TryFindModuleId(HttpRequestMessage request, out int moduleId);

        /// <summary>Try to get the module info associated with the request.</summary>
        /// <param name="request">The web API request.</param>
        /// <param name="moduleInfo">The module info if it was found, or <see langword="null" />.</param>
        /// <returns>Whether the module info was found successfully.</returns>
        bool TryFindModuleInfo(HttpRequestMessage request, out ModuleInfo? moduleInfo);
    }
}
