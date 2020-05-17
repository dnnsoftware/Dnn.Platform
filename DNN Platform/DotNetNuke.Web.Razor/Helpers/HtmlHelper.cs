﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using System;
using System.Web;

#endregion

namespace DotNetNuke.Web.Razor.Helpers
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class HtmlHelper
    {
        private readonly string _resourceFile;
        private ModuleInstanceContext _context;

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public HtmlHelper(ModuleInstanceContext context, string resourcefile)
        {
            _context = context;
            _resourceFile = resourcefile;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public object GetLocalizedString(string key)
        {
            return Localization.GetString(key, _resourceFile);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public object GetLocalizedString(string key, string culture)
        {
            return Localization.GetString(key, _resourceFile, culture);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public HtmlString Raw(string text)
        {
            return new HtmlString(text);
        }
    }
}
