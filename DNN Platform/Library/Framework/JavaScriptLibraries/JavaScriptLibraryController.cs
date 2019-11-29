#region Copyright
// 
// DotNetNuke® - https://www.dnnsoftware.com
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
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    public class JavaScriptLibraryController 
                        : ServiceLocator<IJavaScriptLibraryController, JavaScriptLibraryController>
                        , IJavaScriptLibraryController
    {
        private void ClearCache()
        {
	        DataCache.RemoveCache(DataCache.JavaScriptLibrariesCacheKey);
        }

        protected override Func<IJavaScriptLibraryController> GetFactory()
        {
            return () => new JavaScriptLibraryController();
        }

        #region IJavaScriptController Implementation

        /// <summary>Delete the library reference from the database</summary>
        /// <param name="library">Library to be deleted</param>
        public void DeleteLibrary(JavaScriptLibrary library)
        {
            DataProvider.Instance().ExecuteNonQuery("DeleteJavaScriptLibrary", library.JavaScriptLibraryID);
            ClearCache();
        }

        /// <summary>Get information about the latest version of a <see cref="JavaScriptLibrary"/> that matches the given <paramref name="predicate"/></summary>
        /// <param name="predicate">A function used to filter the library</param>
        /// <example>
        /// JavaScriptLibraryController.Instance.GetLibrary(l => string.Equals(l.LibraryName, "Knockout", StringComparison.OrdinalIgnoreCase))
        /// </example>
        /// <returns>The highest version <see cref="JavaScriptLibrary"/> instance that matches the <paramref name="predicate"/>, or <c>null</c> if no library matches</returns>
        public JavaScriptLibrary GetLibrary(Func<JavaScriptLibrary, bool> predicate)
        {
            return GetLibraries(predicate).OrderByDescending(l => l.Version).FirstOrDefault();
        }

        /// <summary>Gets all of the <see cref="JavaScriptLibrary"/> instances matching the given <paramref name="predicate"/></summary>
        /// <param name="predicate">A function used to filter the library</param>
        /// <example>
        /// JavaScriptLibraryController.Instance.GetLibraries(l => string.Equals(l.LibraryName, "Knockout", StringComparison.OrdinalIgnoreCase))
        /// </example>
        /// <returns>A sequence of <see cref="JavaScriptLibrary"/> instances</returns>
        public IEnumerable<JavaScriptLibrary> GetLibraries(Func<JavaScriptLibrary, bool> predicate)
        {
            return GetLibraries().Where(predicate);
        }

        /// <summary>Gets all of the <see cref="JavaScriptLibrary"/> instances</summary>
        /// <returns>A sequence of <see cref="JavaScriptLibrary"/> instances</returns>
        public IEnumerable<JavaScriptLibrary> GetLibraries()
        {
	    return CBO.GetCachedObject<IEnumerable<JavaScriptLibrary>>(new CacheItemArgs(DataCache.JavaScriptLibrariesCacheKey,
											DataCache.JavaScriptLibrariesCacheTimeout,
											DataCache.JavaScriptLibrariesCachePriority),
                                 c => CBO.FillCollection<JavaScriptLibrary>(DataProvider.Instance().ExecuteReader("GetJavaScriptLibraries")));
        }

        /// <summary>Save a library to the database</summary>
        /// <param name="library">Library to be saved</param>
        public void SaveLibrary(JavaScriptLibrary library)
        {
            library.JavaScriptLibraryID = DataProvider.Instance().ExecuteScalar<int>("SaveJavaScriptLibrary", 
                                                        library.JavaScriptLibraryID,
                                                        library.PackageID,
                                                        library.LibraryName,
                                                        library.Version.ToString(3),
                                                        library.FileName,
                                                        library.ObjectName,
                                                        library.PreferredScriptLocation,
                                                        library.CDNPath);
            ClearCache();
        }

        #endregion
    }
}
