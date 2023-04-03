// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System.IO;

    /// <summary>Deletes files.</summary>
    internal interface IDeleteFilesStep : IStep
    {
        /// <summary>Gets or sets the path to the directory to search, relative to the application root path.</summary>
        string RelativePath { get; set; }

        /// <summary>
        /// Gets or sets the search string to match against the names of files in path.
        /// This parameter can contain a combination of valid literal path and wildcard
        /// (* and ?) characters, but doesn't support regular expressions.
        /// </summary>
        string SearchPattern { get; set; }

        /// <summary>
        /// Gets or sets an enumeration value that specifies whether the search operation should
        /// include only the current directory or should include all subdirectories.
        /// The default value is <see cref="SearchOption.TopDirectoryOnly"/>.
        /// </summary>
        SearchOption SearchOption { get; set; }
    }
}
