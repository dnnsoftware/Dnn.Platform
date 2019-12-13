// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Services.Exceptions
{
    public class SearchIndexEmptyException : Exception
    {
        public SearchIndexEmptyException(string message) : base(message)
        {
        }
    }
}
