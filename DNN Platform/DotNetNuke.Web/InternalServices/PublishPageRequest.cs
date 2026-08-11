// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

/// <summary>A data transfer object with information about a request to publish the current page.</summary>
public class PublishPageRequest
{
    /// <summary>Gets or sets a value indicating whether to publish.</summary>
    public bool Publish { get; set; }
}
