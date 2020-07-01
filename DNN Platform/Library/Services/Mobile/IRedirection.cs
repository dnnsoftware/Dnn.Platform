// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;

    public interface IRedirection
    {
        /// <summary>
        /// Gets primary ID.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets portal Id.
        /// </summary>
        int PortalId { get; set; }

        /// <summary>
        /// Gets or sets redirection name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets if redirect by visit the whole portal, this value should be -1;
        /// otherwise should be the exactly page id for redirection.
        /// </summary>
        int SourceTabId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this value will be available when SourceTabId have a specific value, in that way when this value is true, page will rediect
        /// to target when request source tab and all child tabs under source tab.
        /// </summary>
        bool IncludeChildTabs { get; set; }

        /// <summary>
        /// Gets or sets the redirection type: should be Mobile, Tablet, Both of mobile and tablet, and all other unknown devices.
        /// if this value is Other, should use MatchRules to match the special request need to redirect.
        /// </summary>
        RedirectionType Type { get; set; }

        /// <summary>
        /// Gets or sets request match rules.
        /// </summary>
        IList<IMatchRule> MatchRules { get; set; }

        /// <summary>
        /// Gets or sets redirection target type.
        /// </summary>
        TargetType TargetType { get; set; }

        /// <summary>
        /// Gets or sets redirection target value, can a portal id, tab id or a specific url.
        /// </summary>
        object TargetValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled the Redirection.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets redirection's Order.
        /// </summary>
        int SortOrder { get; set; }
    }
}
