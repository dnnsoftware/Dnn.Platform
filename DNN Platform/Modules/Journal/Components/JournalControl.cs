// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DotNetNuke.Services.Tokens;

public class JournalControl : IPropertyAccess
{
    /// <inheritdoc/>
    public CacheLevel Cacheability
    {
        get
        {
            return CacheLevel.fullyCacheable;
        }
    }

    public string CommentLink { get; set; }

    public string LikeLink { get; set; }

    public string LikeList { get; set; }

    public string CommentArea { get; set; }

    public string AuthorNameLink { get; set; }

    /// <inheritdoc/>
    public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
    {
        string outputFormat = string.Empty;
        if (format == string.Empty)
        {
            outputFormat = "g";
        }
        else
        {
            outputFormat = format;
        }

        propertyName = propertyName.ToLowerInvariant();
        switch (propertyName)
        {
            case "commentlink":
                return this.CommentLink;
            case "likelink":
                return this.LikeLink;
            case "likelist":
                return this.LikeList;
            case "commentarea":
                return this.CommentArea;
            case "authornamelink":
                return this.AuthorNameLink;
        }

        propertyNotFound = true;
        return string.Empty;
    }
}
