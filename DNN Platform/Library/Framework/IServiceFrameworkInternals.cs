// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web.UI;

namespace DotNetNuke.Framework
{
    internal interface IServiceFrameworkInternals
    {
        bool IsAjaxAntiForgerySupportRequired { get; }

        void RegisterAjaxAntiForgery(Page page);

        bool IsAjaxScriptSupportRequired { get; }

        void RegisterAjaxScript(Page page);
    }
}
