// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Web.Client.Controls;

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    public class DnnJsExclude : ClientResourceExclude
    {
        public DnnJsExclude()
        {
            DependencyType = ClientDependency.Core.ClientDependencyType.Javascript;
        }
    }
}
