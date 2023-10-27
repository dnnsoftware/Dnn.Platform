// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    /// <summary>
    /// Represents a CSS color and its components.
    /// </summary>
    public interface IStyleColor
    {
        /// <summary>
        /// Gets or sets the red component as a byte between 0 and 255.
        /// </summary>
        byte Red { get; set; }

        /// <summary>
        /// Gets or sets the green component as a byte between 0 and 255.
        /// </summary>
        byte Green { get; set; }

        /// <summary>
        /// Gets or sets the blue component as a byte between 0 and 255.
        /// </summary>
        byte Blue { get; set; }

        /// <summary>
        /// Gets or sets the color using a 3 or 6 characters hex string such as 0088FF.
        /// </summary>
        string HexValue { get; set; }

        /// <summary>
        /// Gets a shorter sting for the hex color value if possible.
        /// ex: AABBCC would return ABC.
        /// </summary>
        string MinifiedHex { get; }
    }
}
