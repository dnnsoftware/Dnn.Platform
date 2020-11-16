﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using DotNetNuke.Web.Client.Controls;

    public class DnnCssExclude : ClientResourceExclude
    {
        public DnnCssExclude()
        {
            this.DependencyType = ClientDependency.Core.ClientDependencyType.Css;
        }
    }
}
