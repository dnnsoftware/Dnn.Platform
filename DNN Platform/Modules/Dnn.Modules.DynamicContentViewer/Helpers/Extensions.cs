// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    /// <summary>
    /// Extension Class.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert bytes to hex.
        /// </summary>
        /// <param name="hex">the bytes value.</param>
        /// <returns></returns>
        public static string ToHexString(this byte[] hex)
        {
            if (hex == null)
            {
                return null;
            }
            if (hex.Length == 0)
            {
                return string.Empty;
            }
            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }
    }
}
