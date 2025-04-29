// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Internal.SourceGenerators;

/// Namespace:  DotNetNuke.Services.Search
/// Project:    DotNetNuke
/// Class:      SearchItemInfo
/// <summary>The SearchItemInfo represents a Search Item.</summary>
[DnnDeprecated(7, 1, 0, "No longer used in the Search infrastructure", RemovalVersion = 10)]
[Serializable]
public partial class SearchItemInfo
{
    private int author;
    private string content;
    private string description;
    private string guid;
    private int hitCount;
    private int imageFileId;
    private int moduleId;
    private DateTime pubDate;
    private int searchItemId;
    private string searchKey;
    private int tabId;
    private string title;

    /// <summary>Initializes a new instance of the <see cref="SearchItemInfo"/> class.</summary>
    public SearchItemInfo()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SearchItemInfo"/> class.</summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="pubDate"></param>
    /// <param name="moduleID"></param>
    /// <param name="searchKey"></param>
    /// <param name="content"></param>
    public SearchItemInfo(string title, string description, int author, DateTime pubDate, int moduleID, string searchKey, string content)
        : this(title, description, author, pubDate, moduleID, searchKey, content, string.Empty, Null.NullInteger)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SearchItemInfo"/> class.</summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="pubDate"></param>
    /// <param name="moduleID"></param>
    /// <param name="searchKey"></param>
    /// <param name="content"></param>
    /// <param name="guid"></param>
    public SearchItemInfo(string title, string description, int author, DateTime pubDate, int moduleID, string searchKey, string content, string guid)
        : this(title, description, author, pubDate, moduleID, searchKey, content, guid, Null.NullInteger)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SearchItemInfo"/> class.</summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="pubDate"></param>
    /// <param name="moduleID"></param>
    /// <param name="searchKey"></param>
    /// <param name="content"></param>
    /// <param name="image"></param>
    public SearchItemInfo(string title, string description, int author, DateTime pubDate, int moduleID, string searchKey, string content, int image)
        : this(title, description, author, pubDate, moduleID, searchKey, content, string.Empty, image)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SearchItemInfo"/> class.</summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="pubDate"></param>
    /// <param name="moduleID"></param>
    /// <param name="searchKey"></param>
    /// <param name="content"></param>
    /// <param name="guid"></param>
    /// <param name="image"></param>
    public SearchItemInfo(string title, string description, int author, DateTime pubDate, int moduleID, string searchKey, string content, string guid, int image)
    {
        this.title = title;
        this.description = description;
        this.author = author;
        this.pubDate = pubDate;
        this.moduleId = moduleID;
        this.searchKey = searchKey;
        this.content = content;
        this.guid = guid;
        this.imageFileId = image;
        this.hitCount = 0;
    }

    /// <summary>Initializes a new instance of the <see cref="SearchItemInfo"/> class.</summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="author"></param>
    /// <param name="pubDate"></param>
    /// <param name="moduleID"></param>
    /// <param name="searchKey"></param>
    /// <param name="content"></param>
    /// <param name="guid"></param>
    /// <param name="image"></param>
    /// <param name="tabID"></param>
    public SearchItemInfo(string title, string description, int author, DateTime pubDate, int moduleID, string searchKey, string content, string guid, int image, int tabID)
    {
        this.title = title;
        this.description = description;
        this.author = author;
        this.pubDate = pubDate;
        this.moduleId = moduleID;
        this.searchKey = searchKey;
        this.content = content;
        this.guid = guid;
        this.imageFileId = image;
        this.hitCount = 0;
        this.tabId = tabID;
    }

    public int SearchItemId
    {
        get
        {
            return this.searchItemId;
        }

        set
        {
            this.searchItemId = value;
        }
    }

    public string Title
    {
        get
        {
            return this.title;
        }

        set
        {
            this.title = value;
        }
    }

    public string Description
    {
        get
        {
            return this.description;
        }

        set
        {
            this.description = value;
        }
    }

    public int Author
    {
        get
        {
            return this.author;
        }

        set
        {
            this.author = value;
        }
    }

    public DateTime PubDate
    {
        get
        {
            return this.pubDate;
        }

        set
        {
            this.pubDate = value;
        }
    }

    public int ModuleId
    {
        get
        {
            return this.moduleId;
        }

        set
        {
            this.moduleId = value;
        }
    }

    public string SearchKey
    {
        get
        {
            return this.searchKey;
        }

        set
        {
            this.searchKey = value;
        }
    }

    public string Content
    {
        get
        {
            return this.content;
        }

        set
        {
            this.content = value;
        }
    }

    public string GUID
    {
        get
        {
            return this.guid;
        }

        set
        {
            this.guid = value;
        }
    }

    public int ImageFileId
    {
        get
        {
            return this.imageFileId;
        }

        set
        {
            this.imageFileId = value;
        }
    }

    public int HitCount
    {
        get
        {
            return this.hitCount;
        }

        set
        {
            this.hitCount = value;
        }
    }

    public int TabId
    {
        get
        {
            return this.tabId;
        }

        set
        {
            this.tabId = value;
        }
    }
}
