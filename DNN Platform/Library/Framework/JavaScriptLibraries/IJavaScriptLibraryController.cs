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

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    public interface IJavaScriptLibraryController
    {
        /// <summary>Delete the library reference from the database</summary>
        /// <param name="library">Library to be deleted</param>
        void DeleteLibrary(JavaScriptLibrary library);

        /// <summary>Get information about the latest version of a <see cref="JavaScriptLibrary"/> that matches the given <paramref name="predicate"/></summary>
        /// <param name="predicate">A function used to filter the library</param>
        /// <example>
        /// JavaScriptLibraryController.Instance.GetLibrary(l => string.Equals(l.LibraryName, "Knockout", StringComparison.OrdinalIgnoreCase))
        /// </example>
        /// <returns>The highest version <see cref="JavaScriptLibrary"/> instance that matches the <paramref name="predicate"/>, or <c>null</c> if no library matches</returns>
        JavaScriptLibrary GetLibrary(Func<JavaScriptLibrary, bool> predicate);

        /// <summary>Gets all of the <see cref="JavaScriptLibrary"/> instances matching the given <paramref name="predicate"/></summary>
        /// <param name="predicate">A function used to filter the library</param>
        /// <example>
        /// JavaScriptLibraryController.Instance.GetLibraries(l => string.Equals(l.LibraryName, "Knockout", StringComparison.OrdinalIgnoreCase))
        /// </example>
        /// <returns>A sequence of <see cref="JavaScriptLibrary"/> instances</returns>
        IEnumerable<JavaScriptLibrary> GetLibraries(Func<JavaScriptLibrary, bool> predicate);

        /// <summary>Gets all of the <see cref="JavaScriptLibrary"/> instances</summary>
        /// <returns>A sequence of <see cref="JavaScriptLibrary"/> instances</returns>
        IEnumerable<JavaScriptLibrary> GetLibraries();

        /// <summary>Save a library to the database</summary>
        /// <param name="library">Library to be saved</param>
        void SaveLibrary(JavaScriptLibrary library);
    }
}
