// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI
{
    /// <summary>A contract specifying the ability to localize text within an object.</summary>
    public interface ILocalizable
    {
        /// <summary>Gets or sets the path to the resource file.</summary>
        string LocalResourceFile { get; set; }

        /// <summary>Gets or sets a value indicating whether to localize the text.</summary>
        bool Localize { get; set; }

        /// <summary>Update the text to be localized.</summary>
        void LocalizeStrings();
    }
}
