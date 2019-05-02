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

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchItemInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchItemInfo represents a Search Item
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    [Serializable]
    public class SearchItemInfo
    {
        private int _Author;
        private string _Content;
        private string _Description;
        private string _GUID;
        private int _HitCount;
        private int _ImageFileId;
        private int _ModuleId;
        private DateTime _PubDate;
        private int _SearchItemId;
        private string _SearchKey;
        private int _TabId;
        private string _Title;

        public SearchItemInfo()
        {
        }

        public SearchItemInfo(string Title, string Description, int Author, DateTime PubDate, int ModuleID, string SearchKey, string Content)
            : this(Title, Description, Author, PubDate, ModuleID, SearchKey, Content, "", Null.NullInteger)
        {
        }

        public SearchItemInfo(string Title, string Description, int Author, DateTime PubDate, int ModuleID, string SearchKey, string Content, string Guid)
            : this(Title, Description, Author, PubDate, ModuleID, SearchKey, Content, Guid, Null.NullInteger)
        {
        }

        public SearchItemInfo(string Title, string Description, int Author, DateTime PubDate, int ModuleID, string SearchKey, string Content, int Image)
            : this(Title, Description, Author, PubDate, ModuleID, SearchKey, Content, "", Image)
        {
        }

        public SearchItemInfo(string Title, string Description, int Author, DateTime PubDate, int ModuleID, string SearchKey, string Content, string Guid, int Image)
        {
            _Title = Title;
            _Description = Description;
            _Author = Author;
            _PubDate = PubDate;
            _ModuleId = ModuleID;
            _SearchKey = SearchKey;
            _Content = Content;
            _GUID = Guid;
            _ImageFileId = Image;
            _HitCount = 0;
        }

        public SearchItemInfo(string Title, string Description, int Author, DateTime PubDate, int ModuleID, string SearchKey, string Content, string Guid, int Image, int TabID)
        {
            _Title = Title;
            _Description = Description;
            _Author = Author;
            _PubDate = PubDate;
            _ModuleId = ModuleID;
            _SearchKey = SearchKey;
            _Content = Content;
            _GUID = Guid;
            _ImageFileId = Image;
            _HitCount = 0;
            _TabId = TabID;
        }


        public int SearchItemId
        {
            get
            {
                return _SearchItemId;
            }
            set
            {
                _SearchItemId = value;
            }
        }

        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
            }
        }

        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }

        public int Author
        {
            get
            {
                return _Author;
            }
            set
            {
                _Author = value;
            }
        }

        public DateTime PubDate
        {
            get
            {
                return _PubDate;
            }
            set
            {
                _PubDate = value;
            }
        }

        public int ModuleId
        {
            get
            {
                return _ModuleId;
            }
            set
            {
                _ModuleId = value;
            }
        }

        public string SearchKey
        {
            get
            {
                return _SearchKey;
            }
            set
            {
                _SearchKey = value;
            }
        }

        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
            }
        }

        public string GUID
        {
            get
            {
                return _GUID;
            }
            set
            {
                _GUID = value;
            }
        }

        public int ImageFileId
        {
            get
            {
                return _ImageFileId;
            }
            set
            {
                _ImageFileId = value;
            }
        }

        public int HitCount
        {
            get
            {
                return _HitCount;
            }
            set
            {
                _HitCount = value;
            }
        }

        public int TabId
        {
            get
            {
                return _TabId;
            }
            set
            {
                _TabId = value;
            }
        }
    }
}
