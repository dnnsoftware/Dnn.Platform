﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;

    using DotNetNuke.Framework;

    /// <summary>
    /// Business Layer to manage Search.
    /// </summary>
    public class SearchController : ServiceLocator<ISearchController, SearchController>
    {
        /// <inheritdoc/>
        protected override Func<ISearchController> GetFactory()
        {
            return () => new SearchControllerImpl();
        }
    }
}
