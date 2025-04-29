// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Exceptions;

using System;

public class SearchIndexEmptyException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="SearchIndexEmptyException"/> class.</summary>
    /// <param name="message"></param>
    public SearchIndexEmptyException(string message)
        : base(message)
    {
    }
}
