// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

/// <summary>Marker interface for stylesheet resources.</summary>
public interface IStylesheetResource : ILinkResource
{
    /// <summary>
    /// Gets or sets a value indicating whether the disabled attribute should be added to the link element.
    /// Indicates whether the described stylesheet should be loaded and applied to the document.
    /// If disabled is specified in the HTML when it is loaded, the stylesheet will not be loaded during page load.
    /// Instead, the stylesheet will be loaded on-demand, if and when the disabled attribute is changed to false or removed.
    /// </summary>
    public bool Disabled { get; set; }
}
