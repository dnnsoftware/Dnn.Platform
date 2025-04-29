// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices.Views.Search;

using System.Collections.Generic;

/// <summary>Detailed Search Result View.</summary>
public class BasicView
{
    /// <summary>Initializes a new instance of the <see cref="BasicView"/> class.</summary>
    public BasicView()
    {
        this.Attributes = new Dictionary<string, string>();
    }

    /// <summary>Gets or sets document's Title.</summary>
    public string Title { get; set; }

    /// <summary>Gets or sets highlighted snippet from document.</summary>
    public string Snippet { get; set; }

    /// <summary>Gets or sets description from document.</summary>
    public string Description { get; set; }

    /// <summary>Gets or sets link to the Document.</summary>
    public string DocumentUrl { get; set; }

    /// <summary>Gets or sets display Name of the Document Type.</summary>
    public string DocumentTypeName { get; set; }

    /// <summary>Gets or sets custom Attributes of the document.</summary>
    public IDictionary<string, string> Attributes { get; set; }
}
