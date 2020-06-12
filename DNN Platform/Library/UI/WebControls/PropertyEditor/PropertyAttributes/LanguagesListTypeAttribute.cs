

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

using DotNetNuke.Services.Localization;

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LanguagesListTypeAttribute : Attribute
    {
        private readonly LanguagesListType _ListType;

        /// <summary>
        /// Initializes a new instance of the LanguagesListTypeAttribute class.
        /// </summary>
        /// <param name="type">The type of List</param>
        public LanguagesListTypeAttribute(LanguagesListType type)
        {
            this._ListType = type;
        }

        public LanguagesListType ListType
        {
            get
            {
                return this._ListType;
            }
        }
    }
}
