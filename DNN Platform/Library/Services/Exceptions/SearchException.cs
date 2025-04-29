// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions;

using System;

using DotNetNuke.Services.Search;

#pragma warning disable 0618
public class SearchException : BasePortalException
{
    private readonly SearchItemInfo searchItem;

    // default constructor

    /// <summary>Initializes a new instance of the <see cref="SearchException"/> class.</summary>
    public SearchException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SearchException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    /// <param name="searchItem"></param>
    public SearchException(string message, Exception inner, SearchItemInfo searchItem)
        : base(message, inner)
    {
        this.searchItem = searchItem;
    }

    public SearchItemInfo SearchItem
    {
        get
        {
            return this.searchItem;
        }
    }
}
#pragma warning restore 0618
