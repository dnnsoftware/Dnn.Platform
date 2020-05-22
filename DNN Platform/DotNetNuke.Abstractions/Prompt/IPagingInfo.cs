// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

namespace DotNetNuke.Abstractions.Prompt
{
    public interface IPagingInfo
    {
        int PageNo { get; set; }
        int PageSize { get; set; }
        int TotalPages { get; set; }
    }
}