// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>Represents a CSS color and its components.</summary>
    public struct StyleColor
    {
        private static readonly Regex HexColorRegex = new Regex(@"([\da-f]{3}){1,2}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly byte red;
        private readonly byte green;
        private readonly byte blue;
        private string hex;

        /// <summary> Initializes a new instance of the <see cref="StyleColor"/> struct.</summary>
        public StyleColor()
            : this("FFFFFF")
        {
        }

        /// <summary>Initializes a new instance of the <see cref="StyleColor"/> struct.</summary>
        /// <param name="hexValue">The hex value to use.</param>
        public StyleColor(string hexValue)
        {
            AssertIsValidCssColor(hexValue);

            this.HexValue = hexValue;
            this.red = byte.Parse(this.HexValue.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            this.green = byte.Parse(this.HexValue.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            this.blue = byte.Parse(this.HexValue.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
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
                AssertIsValidCssColor(value);
                this.hex = value;
            }
        }

        private static void AssertIsValidCssColor(string hexValue)
        {
            if (string.IsNullOrWhiteSpace(hexValue))
            {
                throw new ArgumentNullException(nameof(hexValue), "You need to provide a CSS color value in the constructor");
            }

            if (!HexColorRegex.IsMatch(hexValue))
            {
                throw new ArgumentOutOfRangeException(nameof(hexValue), hexValue, $"The value {hexValue} that was provided is not valid, it needs to be 3 or 6 character long hexadecimal string without the # sing");
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
