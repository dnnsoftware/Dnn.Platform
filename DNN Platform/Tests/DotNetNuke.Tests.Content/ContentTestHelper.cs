#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Content
{
    public class ContentTestHelper
    {
        internal static ContentItem CreateValidContentItem()
        {
            ContentItem content = new ContentItem {Content = Constants.CONTENT_ValidContent, ContentKey = Constants.CONTENT_ValidContentKey, Indexed = Constants.CONTENT_IndexedFalse};
            return content;
        }

        internal static ContentType CreateValidContentType()
        {
            ContentType contentType = new ContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };
            return contentType;
        }

        internal static Term CreateValidHeirarchicalTerm(int vocabularyId, int parentId)
        {
            Term term = new Term(vocabularyId) {Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight, ParentTermId = parentId};
            return term;
        }

        internal static ScopeType CreateValidScopeType()
        {
            ScopeType scopeType = new ScopeType { ScopeType = Constants.SCOPETYPE_ValidScopeType };
            return scopeType;
        }

        internal static Term CreateValidSimpleTerm(int vocabularyId)
        {
            Term term = new Term(vocabularyId) {Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight};
            return term;
        }

        internal static Vocabulary CreateValidVocabulary()
        {
            Vocabulary vocabulary = new Vocabulary
                                        {
                                            Name = Constants.VOCABULARY_ValidName,
                                            Type = Constants.VOCABULARY_ValidType,
                                            ScopeTypeId = Constants.VOCABULARY_ValidScopeTypeId,
                                            ScopeId = Constants.VOCABULARY_ValidScopeId,
                                            Weight = Constants.VOCABULARY_ValidWeight
                                        };

            return vocabulary;
        }

        internal static FileInfo CreateValidFile(int fileId)
        {
            var sb = new StringBuilder();

            foreach (var b in Encoding.ASCII.GetChars(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(fileId.ToString()))))
            {
                sb.Append(b);
            }

            return new FileInfo
                {
                    ContentType = "text/plain",
                    Extension = ".txt",
                    FileId = fileId,
                    FileName = new Random().Next().ToString("x").ToLowerInvariant(),
                    Folder = @"C:\",
                    FolderId = 0,
                    FolderMappingID = 0,
                    Height = 0,
                    IsCached = false,
                    LastModificationTime = DateTime.UtcNow,
                    PortalId = 0,
                    SHA1Hash = sb.ToString(),
                    Size = 256,
                    StorageLocation = 0,
                    UniqueId = Guid.NewGuid(),
                    VersionGuid = Guid.NewGuid(),
                    Width = 0
                };
        }

        internal static string GetContent(int i)
        {
            return String.Format(String.Format(Constants.CONTENT_ValidContentFormat, i));
        }

        public static string GetContentKey(int i)
        {
            return String.Format(String.Format(Constants.CONTENT_ValidContentKeyFormat, i));
        }

        internal static string GetContentType(int i)
        {
            return String.Format(Constants.CONTENTTYPE_ValidContentTypeFormat, i);
        }

        internal static string GetScopeType(int i)
        {
            return String.Format(Constants.SCOPETYPE_ValidScopeTypeFormat, i);
        }

        internal static string GetTermName(int i)
        {
            return String.Format(Constants.TERM_ValidNameFormat, i);
        }

        internal static string GetVocabularyName(int i)
        {
            return String.Format(Constants.VOCABULARY_ValidNameFormat, i);
        }
    }
}