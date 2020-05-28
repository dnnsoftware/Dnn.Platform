// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
