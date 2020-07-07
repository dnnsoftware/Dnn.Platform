// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;

    /// <summary>
    /// Implementing this interface on an Auth filter will allow the filter to override the default
    /// Host level auth provided by DnnController.
    /// </summary>
    public interface IOverrideDefaultAuthLevel
    {
        // no need for methods the mere presence of this interface acts as a flag to OverridableHostAuthFilter
    }
}
