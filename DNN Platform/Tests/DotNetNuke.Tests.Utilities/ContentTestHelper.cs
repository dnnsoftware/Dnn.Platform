// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Services.FileSystem;

    public class ContentTestHelper
    {
        public static ContentItem CreateValidContentItem()
        {
            ContentItem content = new ContentItem { Content = Constants.CONTENT_ValidContent, ContentKey = Constants.CONTENT_ValidContentKey, Indexed = Constants.CONTENT_IndexedFalse };
            return content;
        }

        public static ContentType CreateValidContentType()
        {
            ContentType contentType = new ContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };
            return contentType;
        }

        public static Term CreateValidHeirarchicalTerm(int vocabularyId, int parentId)
        {
            Term term = new Term(vocabularyId) { Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight, ParentTermId = parentId };
            return term;
        }

        public static ScopeType CreateValidScopeType()
        {
            ScopeType scopeType = new ScopeType { ScopeType = Constants.SCOPETYPE_ValidScopeType };
            return scopeType;
        }

        public static Term CreateValidSimpleTerm(int vocabularyId)
        {
            Term term = new Term(vocabularyId) { Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight };
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
                Weight = Constants.VOCABULARY_ValidWeight,
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
                    Width = 0,
                };
            }
        }

        public static string GetContent(int i)
        {
            return string.Format(string.Format(Constants.CONTENT_ValidContentFormat, i));
        }

        public static string GetContentKey(int i)
        {
            return string.Format(string.Format(Constants.CONTENT_ValidContentKeyFormat, i));
        }

        public static string GetContentType(int i)
        {
            return string.Format(Constants.CONTENTTYPE_ValidContentTypeFormat, i);
        }

        public static string GetScopeType(int i)
        {
            return string.Format(Constants.SCOPETYPE_ValidScopeTypeFormat, i);
        }

        public static string GetTermName(int i)
        {
            return string.Format(Constants.TERM_ValidNameFormat, i);
        }

        public static string GetVocabularyName(int i)
        {
            return string.Format(Constants.VOCABULARY_ValidNameFormat, i);
        }
    }
}
