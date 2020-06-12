// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
    /// <summary>
    /// Business Layer to manage Search.
    /// </summary>
    public class SearchController : ServiceLocator<ISearchController, SearchController>
    {
        protected override Func<ISearchController> GetFactory()
        {
            return () => new SearchControllerImpl();
        }
    }
}
