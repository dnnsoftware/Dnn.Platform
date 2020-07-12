// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;
    using System.Web.UI;

    internal interface IServiceFrameworkInternals
    {
        bool IsAjaxAntiForgerySupportRequired { get; }

        bool IsAjaxScriptSupportRequired { get; }

        void RegisterAjaxAntiForgery(Page page);

        void RegisterAjaxScript(Page page);
    }
}
