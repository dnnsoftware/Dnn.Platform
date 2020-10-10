// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Application
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// test.
    /// </summary>
    public interface IDnnStartupConfigure
    {
        /// <summary>
        /// Configure.
        /// </summary>
        /// <param name="app">app.</param>
        /// <param name="env">env.</param>
        void Configure(IApplicationBuilder app, IApplicationInfo env);
    }
}
