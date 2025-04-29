// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using System;

using DotNetNuke.Internal.SourceGenerators;

/// Namespace:  DotNetNuke.Services.Search
/// Project:    DotNetNuke
/// Class:      SearchResultsInfo
/// <summary>The SearchResultsInfo represents a Search Result Item.</summary>
[DnnDeprecated(7, 1, 0, "No longer used in the Search infrastructure", RemovalVersion = 10)]
[Serializable]
public partial class SearchResultsInfo
{
    private string author;
    private string authorName;
    private bool delete;
    private string description;
    private string guid;
    private int image;
    private int moduleId;
    private int occurrences;
    private int portalId;
    private DateTime pubDate;
    private int relevance;
    private int searchItemID;
    private string searchKey;
    private int tabId;
    private string title;

    public int SearchItemID
    {
        get
        {
            return this.searchItemID;
        }

        set
        {
            this.searchItemID = value;
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

    public string Author
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

    public string Guid
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

    public int Image
    {
        get
        {
            return this.image;
        }

        set
        {
            this.image = value;
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

    public int Occurrences
    {
        get
        {
            return this.occurrences;
        }

        set
        {
            this.occurrences = value;
        }
    }

    public int Relevance
    {
        get
        {
            return this.relevance;
        }

        set
        {
            this.relevance = value;
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

    public bool Delete
    {
        get
        {
            return this.delete;
        }

        set
        {
            this.delete = value;
        }
    }

    public string AuthorName
    {
        get
        {
            return this.authorName;
        }

        set
        {
            this.authorName = value;
        }
    }

    public int PortalId
    {
        get
        {
            return this.portalId;
        }

        set
        {
            this.portalId = value;
        }
    }
}
