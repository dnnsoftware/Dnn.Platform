// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
