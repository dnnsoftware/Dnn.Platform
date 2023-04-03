// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor.Helpers
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class UrlHelper
    {
        private readonly ModuleInstanceContext context;

        /// <summary>Initializes a new instance of the <see cref="UrlHelper"/> class.</summary>
        /// <param name="context">The module context.</param>
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public UrlHelper(ModuleInstanceContext context)
        {
            this.context = context;
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected INavigationManager NavigationManager { get; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public string NavigateToControl()
        {
            return this.NavigationManager.NavigateURL(this.context.TabId);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public string NavigateToControl(string controlKey)
        {
            return this.NavigationManager.NavigateURL(this.context.TabId, controlKey, "mid=" + this.context.ModuleId);
        }
    }
}
