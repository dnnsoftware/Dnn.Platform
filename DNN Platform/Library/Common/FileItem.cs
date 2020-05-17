﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Common
{
    /// <summary>
    /// This class handles basic elements about File Items. Is is a basic Get/Set for Value and Text
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
            Value = value;
            Text = text;
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
