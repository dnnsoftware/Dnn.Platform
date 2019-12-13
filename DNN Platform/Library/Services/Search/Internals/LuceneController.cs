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
	/// Low-level APIs to manage Lucene Layer. This is an Internal class and should not be used outside of Core
	/// </summary>
    internal class LuceneController : ServiceLocator<ILuceneController, LuceneController>
    {
        protected override Func<ILuceneController> GetFactory()
        {
            return () => new LuceneControllerImpl();
        }
    }
}
