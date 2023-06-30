// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    /// <summary>Removes items in the specified section of the Web.config file that match a search term.</summary>
    internal interface IRemoveItemFromCollectionStep : IXmlStep
    {
        /// <summary>Gets or sets the XPath expression of the target collection.</summary>
        string CollectionPath { get; set; }

        /// <summary>
        /// Gets or sets the text that items must contain for them to get removed from the collection
        /// (case insensitive).
        /// </summary>
        string SearchTerm { get; set; }

        /// <summary>Gets or sets the attribute names to include in the search.</summary>
        string AttributeNamesToIncludeInSearch { get; set; }
    }
}
