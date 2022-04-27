// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    /// <summary>
    /// Find and replace text in files.
    /// </summary>
    internal interface IReplaceTextInFileStep : IStep
    {
        /// <summary>
        /// Gets or sets the file path relative to the site's root folder.
        /// </summary>
        string RelativeFilePath { get; set; }

        /// <summary>
        /// Gets or sets the regular expression pattern to match.
        /// </summary>
        string SearchPattern { get; set; }

        /// <summary>
        /// Gets or sets the text to replace the match with.
        /// </summary>
        string Replacement { get; set; }

        /// <summary>
        /// Gets the number of matches found in text.
        /// </summary>
        int MatchCount { get; }
    }
}
