// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.UI.Modules;
using System;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace DotNetNuke.Web.Razor.Helpers
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class UrlHelper
    {
        private readonly ModuleInstanceContext _context;
        protected INavigationManager NavigationManager { get; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public UrlHelper(ModuleInstanceContext context)
        {
            _context = context;
            NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public string NavigateToControl()
        {
            return NavigationManager.NavigateURL(_context.TabId);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public string NavigateToControl(string controlKey)
        {
            return NavigationManager.NavigateURL(_context.TabId, controlKey, "mid=" + _context.ModuleId);
        }
    }
}
