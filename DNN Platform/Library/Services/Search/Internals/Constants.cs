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
using Version = Lucene.Net.Util.Version;
#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// Constants
    /// </summary>
    internal static class Constants
    {
        internal const string UniqueKeyTag = "key";
        internal const string PortalIdTag = "portal";
        internal const string SearchTypeTag = "searchtype";
        internal const string TitleTag = "title";
        internal const string DescriptionTag = "description";
        internal const string ContentTag = "content";
        internal const string UrlTag = "url";
        internal const string BodyTag = "body";
        internal const string TabIdTag = "tab";
        internal const string TabMetaDataPrefixTag = "tabMetaData_";
        internal const string ModuleIdTag = "module";
        internal const string ModuleDefIdTag = "moduledef";
        internal const string ModuleMetaDataPrefixTag = "moduleMetaData_";
        internal const string CommentsTag = "comments";
        internal const string ViewsTag = "views";
        internal const string LikesTag = "likes";
        internal const string AuthorIdTag = "authorid";
        internal const string AuthorNameTag = "authorname";
        internal const string PermissionsTag = "perm";
        internal const string Tag = "tag";
        internal const string LocaleTag = "locale";
        internal const string ModifiedTimeTag = "time";
        internal const string QueryStringTag = "querystring";
        internal const string NumericKeyPrefixTag = "nk-";
        internal const string KeywordsPrefixTag = "kw-";
        internal const string SubjectTag = "subject";
        internal const string CategoryTag = "category";
        internal const string StatusTag = "status";
        internal const string RoleIdTag = "role";

        //internal const string FolderIdTag = "folderid";
        //internal const string FileIdTag = "fileid";
        //internal const string FolderNameTag = "foldername";
        //internal const string FileNameTag = "filename";

        internal const string DateTimeFormat = "yyyyMMddHHmmssfff";
        internal const string ReindexDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        internal static Version LuceneVersion = Version.LUCENE_30;

        //Field Boost Settings - they are scaled down by 10.
        internal const int DefaultSearchTitleBoost = 50;
        internal const int DefaultSearchTagBoost = 40;
        internal const int DefaultSearchKeywordBoost = 35;
        internal const int DefaultSearchDescriptionBoost = 20;
        internal const int DefaultSearchAuthorBoost = 15;
        internal const int StandardLuceneBoost = 10; //Lucene's default boost is 1.0f

        //Field Bosst Setting Names
        internal const string SearchTitleBoostSetting = "Search_Title_Boost";
        internal const string SearchTagBoostSetting = "Search_Tag_Boost";
        internal const string SearchContentBoostSetting = "Search_Content_Boost";
        internal const string SearchDescriptionBoostSetting = "Search_Description_Boost";
        internal const string SearchAuthorBoostSetting = "Search_Author_Boost";

        //If weighted sum of Likes, Comment and Weight is the number below, Document gets a boost of 1.0
        internal const int DefaultDocumentBoostScale = 1000;

        internal readonly static string[] FieldsNeedAnalysis = { TitleTag, SubjectTag, CommentsTag, AuthorNameTag, StatusTag, CategoryTag };

        internal readonly static string[] KeyWordSearchFields = { TitleTag, 
                                                                Tag, 
                                                                DescriptionTag, 
                                                                BodyTag, 
                                                                ContentTag, 
                                                                KeywordsPrefixTag + TitleTag,
                                                                KeywordsPrefixTag + SubjectTag,
                                                                KeywordsPrefixTag + CommentsTag,
                                                                KeywordsPrefixTag + AuthorNameTag};
        
        // search index tokenizers word lengths
        internal const int MinimumMinLen = 1;
        internal const int DefaultMinLen = 3;
        internal const int MaximumMinLen = 10;

        internal const int MinimumMaxLen = 10;
        internal const int DefaultMaxLen = 255;
        internal const int MaximumMaxLen = 500;

        internal const string SearchMinLengthKey = "Search_MinKeyWordLength";
        internal const string SearchMaxLengthKey = "Search_MaxKeyWordLength";
        internal const string SearchIndexFolderKey = "Search_IndexFolder";
        internal const string SearchReaderRefreshTimeKey = "Search_ReaderRefreshTime";
        internal const string SearchReindexSettingName = "Search_ReindexRequestedOn";
        internal const string SearchLastSuccessIndexName = "Search_LastSuccessfulIndexedOn";
        internal const string SearchOptimizeFlagName = "Search_OptimizeIndex";
        internal const string SearchCustomAnalyzer = "Search_CustomAnalyzer";

        // misc.
        internal const string TlsSearchInfo = "TLS_SEARCH_INFO";
    }
}