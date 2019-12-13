// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
	/// Internal Search Controller. This is an Internal class and should not be used outside of Core
	/// </summary>
    public class SearchHelper : ServiceLocator<ISearchHelper, SearchHelper>
    {
        protected override Func<ISearchHelper> GetFactory()
        {
            return () => new SearchHelperImpl();
        }
    }
}
