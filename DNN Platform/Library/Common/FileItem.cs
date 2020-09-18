// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common
{
    /// <summary>
    /// This class handles basic elements about File Items. Is is a basic Get/Set for Value and Text.
    /// </summary>
    public class FileItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileItem" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="text">The text.</param>
        public FileItem(string value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; private set; }
    }
}
