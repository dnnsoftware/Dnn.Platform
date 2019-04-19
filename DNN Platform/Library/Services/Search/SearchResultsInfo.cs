#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchResultsInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchResultsInfo represents a Search Result Item
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
                return m_SearchItemID;
            }
            set
            {
                m_SearchItemID = value;
            }
        }

        public string Title
        {
            get
            {
                return m_Title;
            }
            set
            {
                m_Title = value;
            }
        }

        public string Description
        {
            get
            {
                return m_Description;
            }
            set
            {
                m_Description = value;
            }
        }

        public string Author
        {
            get
            {
                return m_Author;
            }
            set
            {
                m_Author = value;
            }
        }

        public DateTime PubDate
        {
            get
            {
                return m_PubDate;
            }
            set
            {
                m_PubDate = value;
            }
        }

        public string Guid
        {
            get
            {
                return m_Guid;
            }
            set
            {
                m_Guid = value;
            }
        }

        public int Image
        {
            get
            {
                return m_Image;
            }
            set
            {
                m_Image = value;
            }
        }

        public int TabId
        {
            get
            {
                return m_TabId;
            }
            set
            {
                m_TabId = value;
            }
        }

        public string SearchKey
        {
            get
            {
                return m_SearchKey;
            }
            set
            {
                m_SearchKey = value;
            }
        }

        public int Occurrences
        {
            get
            {
                return m_Occurrences;
            }
            set
            {
                m_Occurrences = value;
            }
        }

        public int Relevance
        {
            get
            {
                return m_Relevance;
            }
            set
            {
                m_Relevance = value;
            }
        }

        public int ModuleId
        {
            get
            {
                return m_ModuleId;
            }
            set
            {
                m_ModuleId = value;
            }
        }

        public bool Delete
        {
            get
            {
                return m_Delete;
            }
            set
            {
                m_Delete = value;
            }
        }

        public string AuthorName
        {
            get
            {
                return m_AuthorName;
            }
            set
            {
                m_AuthorName = value;
            }
        }

        public int PortalId
        {
            get
            {
                return m_PortalId;
            }
            set
            {
                m_PortalId = value;
            }
        }
    }
}
