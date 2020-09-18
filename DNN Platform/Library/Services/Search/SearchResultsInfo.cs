// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchResultsInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchResultsInfo represents a Search Result Item.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    [Serializable]
    public class SearchResultsInfo
    {
        private string m_Author;
        private string m_AuthorName;
        private bool m_Delete;
        private string m_Description;
        private string m_Guid;
        private int m_Image;
        private int m_ModuleId;
        private int m_Occurrences;
        private int m_PortalId;
        private DateTime m_PubDate;
        private int m_Relevance;
        private int m_SearchItemID;
        private string m_SearchKey;
        private int m_TabId;
        private string m_Title;

        public int SearchItemID
        {
            get
            {
                return this.m_SearchItemID;
            }

            set
            {
                this.m_SearchItemID = value;
            }
        }

        public string Title
        {
            get
            {
                return this.m_Title;
            }

            set
            {
                this.m_Title = value;
            }
        }

        public string Description
        {
            get
            {
                return this.m_Description;
            }

            set
            {
                this.m_Description = value;
            }
        }

        public string Author
        {
            get
            {
                return this.m_Author;
            }

            set
            {
                this.m_Author = value;
            }
        }

        public DateTime PubDate
        {
            get
            {
                return this.m_PubDate;
            }

            set
            {
                this.m_PubDate = value;
            }
        }

        public string Guid
        {
            get
            {
                return this.m_Guid;
            }

            set
            {
                this.m_Guid = value;
            }
        }

        public int Image
        {
            get
            {
                return this.m_Image;
            }

            set
            {
                this.m_Image = value;
            }
        }

        public int TabId
        {
            get
            {
                return this.m_TabId;
            }

            set
            {
                this.m_TabId = value;
            }
        }

        public string SearchKey
        {
            get
            {
                return this.m_SearchKey;
            }

            set
            {
                this.m_SearchKey = value;
            }
        }

        public int Occurrences
        {
            get
            {
                return this.m_Occurrences;
            }

            set
            {
                this.m_Occurrences = value;
            }
        }

        public int Relevance
        {
            get
            {
                return this.m_Relevance;
            }

            set
            {
                this.m_Relevance = value;
            }
        }

        public int ModuleId
        {
            get
            {
                return this.m_ModuleId;
            }

            set
            {
                this.m_ModuleId = value;
            }
        }

        public bool Delete
        {
            get
            {
                return this.m_Delete;
            }

            set
            {
                this.m_Delete = value;
            }
        }

        public string AuthorName
        {
            get
            {
                return this.m_AuthorName;
            }

            set
            {
                this.m_AuthorName = value;
            }
        }

        public int PortalId
        {
            get
            {
                return this.m_PortalId;
            }

            set
            {
                this.m_PortalId = value;
            }
        }
    }
}
