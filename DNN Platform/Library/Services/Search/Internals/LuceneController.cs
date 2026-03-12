// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Low-level APIs to manage Lucene Layer. This is an Internal class and should not be used outside of Core.</summary>
    internal class LuceneController : ServiceLocator<ILuceneController, LuceneController>
    {
        /// <inheritdoc />
        protected override Func<ILuceneController> GetFactory()
        {
            return () => ActivatorUtilities.GetServiceOrCreateInstance<LuceneControllerImpl>(Globals.DependencyProvider);
        }
    }
}
