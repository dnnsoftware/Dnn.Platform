// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// Used to page long lists of data to the client
    /// </summary>
    public interface IPagingInfo
    {
        /// <summary>
        /// Current page nr
        /// </summary>
        int PageNo { get; set; }
        /// <summary>
        /// Page size
        /// </summary>
        int PageSize { get; set; }
        /// <summary>
        /// Total nr of pages
        /// </summary>
        int TotalPages { get; set; }
        /// <summary>
        /// Total nr of records
        /// </summary>
        int TotalRecords { get; set; }
    }
}
