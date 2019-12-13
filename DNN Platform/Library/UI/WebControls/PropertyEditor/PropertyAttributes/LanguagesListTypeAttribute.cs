// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Services.Localization;

#endregion

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
            _ListType = type;
        }

        public LanguagesListType ListType
        {
            get
            {
                return _ListType;
            }
        }
    }
}
