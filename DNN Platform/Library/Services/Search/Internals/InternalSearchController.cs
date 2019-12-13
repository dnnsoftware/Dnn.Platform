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
    public class InternalSearchController : ServiceLocator<IInternalSearchController, InternalSearchController>
    {
        protected override Func<IInternalSearchController> GetFactory()
        {
            return () => new InternalSearchControllerImpl();
        }
    }
}
