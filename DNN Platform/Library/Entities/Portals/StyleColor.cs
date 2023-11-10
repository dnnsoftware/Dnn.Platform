// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Portals;

    /// <summary>
    /// Represents a CSS color and its components.
    /// </summary>
    public struct StyleColor : IStyleColor
    {
        private static readonly Regex HexColorRegex = new Regex(@"([\da-f]{3}){1,2}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string hex;

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleColor"/> struct.
        /// </summary>
        public StyleColor()
        {
            this.hex = "FFFFFF";
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
        }

        private enum Component
        {
            Red,
            Green,
            Blue,
        }

        /// <inheritdoc/>
        public byte Red
        {
            get { return this.GetComponent(Component.Red); }
            set { this.SetComponent(Component.Red, value); }
        }

        /// <inheritdoc/>
        public byte Green
        {
            get { return this.GetComponent(Component.Green); }
            set { this.SetComponent(Component.Green, value); }
        }

        /// <inheritdoc/>
        public byte Blue
        {
            get { return this.GetComponent(Component.Blue); }
            set { this.SetComponent(Component.Blue, value); }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        private byte GetComponent(Component comp)
        {
            switch (comp)
            {
                case Component.Red:
                    return byte.Parse(this.hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                case Component.Green:
                    return byte.Parse(this.hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                case Component.Blue:
                    return byte.Parse(this.hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                default:
                    return 0;
            }
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
