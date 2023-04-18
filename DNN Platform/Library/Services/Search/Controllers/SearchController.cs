// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Business Layer to manage Search.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.0.0. Please resolve ISearchController via Dependency Injection. Scheduled removal in v12.0.0.")]
    public class SearchController : ServiceLocator<ISearchController, SearchController>
    {
        /// <inheritdoc/>
        protected override Func<ISearchController> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<ISearchController>;
        }
    }
}
