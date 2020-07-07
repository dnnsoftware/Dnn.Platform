// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.JavaScriptLibraries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    public class JavaScriptLibraryController
                        : ServiceLocator<IJavaScriptLibraryController, JavaScriptLibraryController>,
                        IJavaScriptLibraryController
    {
        /// <summary>Delete the library reference from the database.</summary>
        /// <param name="library">Library to be deleted.</param>
        public void DeleteLibrary(JavaScriptLibrary library)
        {
            DataProvider.Instance().ExecuteNonQuery("DeleteJavaScriptLibrary", library.JavaScriptLibraryID);
            this.ClearCache();
        }

        /// <summary>Get information about the latest version of a <see cref="JavaScriptLibrary"/> that matches the given <paramref name="predicate"/>.</summary>
        /// <param name="predicate">A function used to filter the library.</param>
        /// <example>
        /// JavaScriptLibraryController.Instance.GetLibrary(l => string.Equals(l.LibraryName, "Knockout", StringComparison.OrdinalIgnoreCase)).
        /// </example>
        /// <returns>The highest version <see cref="JavaScriptLibrary"/> instance that matches the <paramref name="predicate"/>, or <c>null</c> if no library matches.</returns>
        public JavaScriptLibrary GetLibrary(Func<JavaScriptLibrary, bool> predicate)
        {
            return this.GetLibraries(predicate).OrderByDescending(l => l.Version).FirstOrDefault();
        }

        /// <summary>Gets all of the <see cref="JavaScriptLibrary"/> instances matching the given <paramref name="predicate"/>.</summary>
        /// <param name="predicate">A function used to filter the library.</param>
        /// <example>
        /// JavaScriptLibraryController.Instance.GetLibraries(l => string.Equals(l.LibraryName, "Knockout", StringComparison.OrdinalIgnoreCase)).
        /// </example>
        /// <returns>A sequence of <see cref="JavaScriptLibrary"/> instances.</returns>
        public IEnumerable<JavaScriptLibrary> GetLibraries(Func<JavaScriptLibrary, bool> predicate)
        {
            return this.GetLibraries().Where(predicate);
        }

        /// <summary>Gets all of the <see cref="JavaScriptLibrary"/> instances.</summary>
        /// <returns>A sequence of <see cref="JavaScriptLibrary"/> instances.</returns>
        public IEnumerable<JavaScriptLibrary> GetLibraries()
        {
            return CBO.GetCachedObject<IEnumerable<JavaScriptLibrary>>(
                new CacheItemArgs(
                DataCache.JavaScriptLibrariesCacheKey,
                DataCache.JavaScriptLibrariesCacheTimeout,
                DataCache.JavaScriptLibrariesCachePriority),
                c => CBO.FillCollection<JavaScriptLibrary>(DataProvider.Instance().ExecuteReader("GetJavaScriptLibraries")));
        }

        /// <summary>Save a library to the database.</summary>
        /// <param name="library">Library to be saved.</param>
        public void SaveLibrary(JavaScriptLibrary library)
        {
            library.JavaScriptLibraryID = DataProvider.Instance().ExecuteScalar<int>(
                "SaveJavaScriptLibrary",
                library.JavaScriptLibraryID,
                library.PackageID,
                library.LibraryName,
                library.Version.ToString(3),
                library.FileName,
                library.ObjectName,
                library.PreferredScriptLocation,
                library.CDNPath);
            this.ClearCache();
        }

        protected override Func<IJavaScriptLibraryController> GetFactory()
        {
            return () => new JavaScriptLibraryController();
        }

        private void ClearCache()
        {
            DataCache.RemoveCache(DataCache.JavaScriptLibrariesCacheKey);
        }
    }
}
