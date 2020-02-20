// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Framework;

namespace DotNetNuke.Web.Mvc.Common
{
    public class AntiForgery : ServiceLocator<IAntiForgery, AntiForgery>
    {
        protected override Func<IAntiForgery> GetFactory()
        {
            return () => new AntiForgeryImpl();
        }
    }
}
