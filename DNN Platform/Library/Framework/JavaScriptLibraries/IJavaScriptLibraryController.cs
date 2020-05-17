﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
