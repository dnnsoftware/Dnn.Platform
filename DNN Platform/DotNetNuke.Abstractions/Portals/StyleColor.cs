// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a CSS color and its components.
    /// </summary>
    public struct StyleColor
    {
        private static readonly Regex HexColorRegex = new Regex(@"([\da-f]{3}){1,2}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly byte red;
        private readonly byte green;
        private readonly byte blue;
        private string hex;

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleColor"/> struct.
        /// </summary>
        public StyleColor()
            : this("FFFFFF")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleColor"/> struct.
        /// </summary>
        /// <param name="hexValue">The hex value to use.</param>
        public StyleColor(string hexValue)
        {
            if (this.IsValidCssColor(hexValue))
            {
                this.HexValue = ExpandColor(hexValue);
            }
            else
            {
                this.HexValue = ExpandColor("FFFFFF");
            }

            this.red = byte.Parse(this.HexValue.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            this.green = byte.Parse(this.HexValue.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            this.blue = byte.Parse(this.HexValue.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }

        private enum Component
        {
            Red,
            Green,
            Blue,
        }

        /// <summary>
        /// Gets or sets the value for the red component.
        /// </summary>
        public byte Red
        {
            get => this.red;
            set { this.SetComponent(Component.Red, value); }
        }

        /// <summary>
        /// Gets or sets the value for the green component.
        /// </summary>
        public byte Green
        {
            get => this.green;
            set { this.SetComponent(Component.Green, value); }
        }

        /// <summary>
        /// Gets or sets the value for the blue component.
        /// </summary>
        public byte Blue
        {
            get => this.blue;
            set { this.SetComponent(Component.Blue, value); }
        }

        /// <summary>
        /// Gets or sets the color hexadecimal value.
        /// </summary>
        public string HexValue
        {
            get
            {
                return this.hex;
            }

            set
            {
                if (this.IsValidCssColor(value))
                {
                    this.hex = ExpandColor(value);
                }
            }
        }

        /// <summary>
        /// Gets a minified hexadecimal value for the color.
        /// </summary>
        /// <example>If the color is 0088FF, it should return 08F.</example>
        public string MinifiedHex
        {
            get
            {
                if (
                    this.hex[0] == this.hex[1] &&
                    this.hex[2] == this.hex[3] &&
                    this.hex[4] == this.hex[5])
                {
                    return this.hex[0].ToString() + this.hex[2].ToString() + this.hex[4].ToString();
                }

                return this.hex;
            }
        }

        private static string ExpandColor(string hexValue)
        {
            if (hexValue.Length == 6)
            {
                return hexValue.ToUpperInvariant();
            }

            string value;
            var r = hexValue[0];
            var g = hexValue[1];
            var b = hexValue[2];
            value = string.Concat(r, r, g, g, b, b);
            return value.ToUpperInvariant();
        }

        private bool IsValidCssColor(string hexValue)
        {
            if (string.IsNullOrWhiteSpace(hexValue))
            {
                throw new ArgumentNullException("You need to provide a CSS color value in the constructor");
            }

            if (!HexColorRegex.IsMatch(hexValue))
            {
                throw new ArgumentOutOfRangeException($"The value {hexValue} that was provided is not valid, it needs to be 3 or 6 character long hexadecimal string without the # sing");
            }

            return true;
        }

        private void SetComponent(Component comp, byte value)
        {
            switch (comp)
            {
                case Component.Red:
                    this.hex = this.hex.Remove(0, 2).Insert(0, $"{value:x2}");
                    break;
                case Component.Green:
                    this.hex = this.hex.Remove(2, 2).Insert(2, $"{value:x2}");
                    break;
                case Component.Blue:
                    this.hex = this.hex.Remove(4, 2).Insert(4, $"{value:x2}");
                    break;
                default:
                    break;
            }
        }
    }
}
