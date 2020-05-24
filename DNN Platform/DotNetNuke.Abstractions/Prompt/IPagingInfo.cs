// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

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