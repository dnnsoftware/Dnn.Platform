// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
