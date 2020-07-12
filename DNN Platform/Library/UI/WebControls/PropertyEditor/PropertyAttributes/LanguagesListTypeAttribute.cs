// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LanguagesListTypeAttribute : Attribute
    {
        private readonly LanguagesListType _ListType;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguagesListTypeAttribute"/> class.
        /// </summary>
        /// <param name="type">The type of List.</param>
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
