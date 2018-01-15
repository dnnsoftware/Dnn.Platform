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

using System;
using System.Security.Cryptography;
using System.Text;

using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Tests.Utilities
{
    public class ContentTestHelper
    {
        public static ContentItem CreateValidContentItem()
        {
            ContentItem content = new ContentItem {Content = Constants.CONTENT_ValidContent, ContentKey = Constants.CONTENT_ValidContentKey, Indexed = Constants.CONTENT_IndexedFalse};
            return content;
        }

        public static ContentType CreateValidContentType()
        {
            ContentType contentType = new ContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };
            return contentType;
        }

        public static Term CreateValidHeirarchicalTerm(int vocabularyId, int parentId)
        {
            Term term = new Term(vocabularyId) {Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight, ParentTermId = parentId};
            return term;
        }

        public static ScopeType CreateValidScopeType()
        {
            ScopeType scopeType = new ScopeType { ScopeType = Constants.SCOPETYPE_ValidScopeType };
            return scopeType;
        }

        public static Term CreateValidSimpleTerm(int vocabularyId)
        {
            Term term = new Term(vocabularyId) {Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight};
            return term;
        }

        public static Vocabulary CreateValidVocabulary()
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

        public static FileInfo CreateValidFile(int fileId)
        {
            var sb = new StringBuilder();

            using (var hasher = SHA1.Create())
            {
                foreach (var b in Encoding.ASCII.GetChars(hasher.ComputeHash(Encoding.UTF8.GetBytes(fileId.ToString()))))
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
        }

        public static string GetContent(int i)
        {
            return String.Format(String.Format(Constants.CONTENT_ValidContentFormat, i));
        }

        public static string GetContentKey(int i)
        {
            return String.Format(String.Format(Constants.CONTENT_ValidContentKeyFormat, i));
        }

        public static string GetContentType(int i)
        {
            return String.Format(Constants.CONTENTTYPE_ValidContentTypeFormat, i);
        }

        public static string GetScopeType(int i)
        {
            return String.Format(Constants.SCOPETYPE_ValidScopeTypeFormat, i);
        }

        public static string GetTermName(int i)
        {
            return String.Format(Constants.TERM_ValidNameFormat, i);
        }

        public static string GetVocabularyName(int i)
        {
            return String.Format(Constants.VOCABULARY_ValidNameFormat, i);
        }
    }
}