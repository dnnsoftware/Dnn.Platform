// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using DotNetNuke.Internal.SourceGenerators;

/// <summary>A data transfer object with information about a request to publish the current page.</summary>
[DnnDeprecated(10, 3, 3, "Use PublishPageRequest")]
public partial class PublishPageDto
{
    /// <summary>Gets or sets a value indicating whether to publish.</summary>
    public bool Publish { get; set; }

    /// <summary>Converts this instance to a <see cref="PublishPageRequest"/>.</summary>
    /// <returns>A <see cref="PublishPageRequest"/> instance.</returns>
    public PublishPageRequest ToPublishPageRequest()
    {
        return new PublishPageRequest
        {
            Publish = this.Publish,
        };
    }
}
