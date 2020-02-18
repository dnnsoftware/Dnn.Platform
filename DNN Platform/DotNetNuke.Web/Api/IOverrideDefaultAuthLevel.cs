// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Web.Api
{
    /// <summary>
    /// Implementing this interface on an Auth filter will allow the filter to override the default
    /// Host level auth provided by DnnController
    /// </summary>
    public interface IOverrideDefaultAuthLevel
    {
        //no need for methods the mere presence of this interface acts as a flag to OverridableHostAuthFilter
    }
}
