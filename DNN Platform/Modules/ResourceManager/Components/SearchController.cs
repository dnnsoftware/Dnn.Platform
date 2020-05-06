// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.Modules.ResourceManager.Exceptions;
using DotNetNuke.ComponentModel;
using DotNetNuke.Services.Assets;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

namespace Dnn.Modules.ResourceManager.Components
{
    public class SearchController : ComponentBase<ISearchController, SearchController>, ISearchController
    {
        private readonly IPermissionsManager _permissionsManager;

        public SearchController()
        {
            _permissionsManager = PermissionsManager.Instance;
        }

        public IList<IFileInfo> SearchFolderContent(int moduleId, IFolderInfo folder, bool recursive, string search, int pageIndex, int pageSize, string sorting, int moduleMode, out int totalCount)
        {
            var noPermissionMessage = Localization.GetExceptionMessage("UserHasNoPermissionToBrowseFolder",
                Constants.UserHasNoPermissionToBrowseFolderDefaultMessage);

            if (!_permissionsManager.HasFolderContentPermission(folder.FolderID, moduleMode))
            {
                throw new FolderPermissionNotMetException(noPermissionMessage);
            }

            search = (search ?? string.Empty).Trim();

            // Lucene does not support wildcards as the beginning of the search
            // For file names we can remove any existing wildcard at the beginning
            var cleanedKeywords = TrimStartWildCards(search);

            var files = FolderManager.Instance.SearchFiles(folder, cleanedKeywords, recursive);
            var sortProperties = SortProperties.Parse(sorting);
            var sortedFiles = SortFiles(files, sortProperties).ToList();
            totalCount = sortedFiles.Count;

            return sortedFiles.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        private static string TrimStartWildCards(string search)
        {
            var keywords = from keyword in search.Split(' ')
                           select keyword.TrimStart('*', '?');
            search = string.Join(" ", keywords);
            return search;
        }

        private static IEnumerable<IFileInfo> SortFiles(IEnumerable<IFileInfo> files, SortProperties sortProperties)
        {
            switch (sortProperties.Column)
            {
                case "ItemName":
                    return OrderBy(files, f => f.FileName, sortProperties.Ascending);
                case "LastModifiedOnDate":
                    return OrderBy(files, f => f.LastModifiedOnDate, sortProperties.Ascending);
                case "Size":
                    return OrderBy(files, f => f.Size, sortProperties.Ascending);
                case "ParentFolder":
                    return OrderBy(files, f => f.FolderId, new FolderPathComparer(), sortProperties.Ascending);
                case "CreatedOnDate":
                    return OrderBy(files, f => f.CreatedOnDate, sortProperties.Ascending);
                default:
                    return files;
            }
        }

        private static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
        {
            return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }

        private static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool ascending)
        {
            return ascending ? source.OrderBy(keySelector, comparer) : source.OrderByDescending(keySelector, comparer);
        }
    }
}
