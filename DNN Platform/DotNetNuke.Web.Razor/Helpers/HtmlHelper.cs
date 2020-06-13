// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor.Helpers
{
    using System;
    using System.Web;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class HtmlHelper
    {
        private readonly string _resourceFile;
        private ModuleInstanceContext _context;

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public HtmlHelper(ModuleInstanceContext context, string resourcefile)
        {
            this._context = context;
            this._resourceFile = resourcefile;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public object GetLocalizedString(string key)
        {
            return Localization.GetString(key, this._resourceFile);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public object GetLocalizedString(string key, string culture)
        {
            return Localization.GetString(key, this._resourceFile, culture);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public HtmlString Raw(string text)
        {
            return new HtmlString(text);
        }
    }
}
