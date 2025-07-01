// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>Used to page long lists of data to the client.</summary>
    public interface IPagingInfo
    {
        /// <summary>Gets or sets the current page number.</summary>
        int PageNo { get; set; }

        /// <summary>Gets or sets the Page size.</summary>
        int PageSize { get; set; }

        /// <summary>Gets or sets the total number of pages.</summary>
        int TotalPages { get; set; }

        /// <summary>Gets or sets the total number of records.</summary>
        int TotalRecords { get; set; }
    }
}
