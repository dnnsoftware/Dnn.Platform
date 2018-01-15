#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetNuke.Common.Utilities
{
    public class FileExtensionWhitelist
    {
        private readonly List<String> _extensions;

        /// <summary>
        /// Initializes a new instance of the FileExtensionWhiteList class.
        /// </summary>
        /// <param name="extensionList">a comma seperated list of file extensions with no '.'</param>
        /// <remarks><paramref name="extensionList"/>should match the format used in the FileExtensions Host setting specifically it
        /// should not have an '.' in the extensions (e.g. txt,jpg,png,doc)</remarks>
        public FileExtensionWhitelist(string extensionList)
        {
            _extensions = EscapedString.Seperate(extensionList.ToLowerInvariant()).Select(item => "." + item).ToList();
        }

        /// <summary>
        /// Returns a string suitale for display to an end user
        /// </summary>
        /// <returns>A String of the whitelist extensions formatted for display to an end user</returns>
        public string ToDisplayString()
        {
            return ToDisplayString(null);
        }

        /// <summary>
        /// Formats the extension whitelist appropriate for display to an end user
        /// </summary>
        /// <param name="additionalExtensions">A list of additionalExtensions to add to the current extensions</param>
        /// <remarks><paramref name="additionalExtensions"/>case and '.' prefix will be corrected, and duplicates will be excluded from the string</remarks>
        /// <returns>A String of the whitelist extensions formatted for storage display to an end user</returns>
        public string ToDisplayString(IEnumerable<string> additionalExtensions)
        {
            IEnumerable<string> allExtensions = CombineLists(additionalExtensions);
            return "*" + string.Join(", *", allExtensions.ToArray());
        }

        /// <summary>
        /// The list of extensions in the whitelist.
        /// </summary>
        /// <remarks>All extensions are lowercase and prefixed with a '.'</remarks>
        public IEnumerable<String> AllowedExtensions
        {
            get
            {
                return _extensions;
            }
        }

        /// <summary>
        /// Indicates if the file extension is permitted by the Host Whitelist
        /// </summary>
        /// <param name="extension">The file extension with or without preceding '.'</param>
        /// <returns>True if extension is in whitelist or whitelist is empty.  False otherwise.</returns>
        public bool IsAllowedExtension(String extension)
        {
            return IsAllowedExtension(extension, null);
        }

        /// <summary>
        /// Indicates if the file extension is permitted by the Host Whitelist
        /// </summary>
        /// <param name="extension">The file extension with or without preceding '.'</param>
        /// <param name="additionalExtensions"></param>
        /// <returns>True if extension is in whitelist or whitelist is empty.  False otherwise.</returns>
        public bool IsAllowedExtension(string extension, IEnumerable<string> additionalExtensions)
        {
            List<string> allExtensions = CombineLists(additionalExtensions).ToList();
            if (!allExtensions.Any())
            {
                return true;
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension.ToLowerInvariant();
            }
            else
            {
                extension = extension.ToLowerInvariant();
            }

            return allExtensions.Contains(extension);
        }

        public override string ToString()
        {
            return ToDisplayString();
        }

        /// <summary>
        /// Formats the extension whitelist appropriate for storage in the Host setting
        /// </summary>
        /// <returns>A String of the whitelist extensions formatted for storage as a Host setting</returns>
        public string ToStorageString()
        {
            return ToStorageString(null);
        }

        /// <summary>
        /// Formats the extension whitelist appropriate for storage in the Host setting
        /// </summary>
        /// <param name="additionalExtensions">A list of additionalExtensions to add to the current extensions</param>
        /// <remarks><paramref name="additionalExtensions"/>case and '.' prefix will be corrected, and duplicates will be excluded from the string</remarks>
        /// <returns>A String of the whitelist extensions formatted for storage as a Host setting</returns>
        public string ToStorageString(IEnumerable<string> additionalExtensions)
        {
            IEnumerable<string> allExtensions = CombineLists(additionalExtensions);
            var leadingDotRemoved = allExtensions.Select(ext => ext.Substring(1));
            return EscapedString.Combine(leadingDotRemoved);
        }

        private IEnumerable<string> CombineLists(IEnumerable<string> additionalExtensions)
        {
            if(additionalExtensions == null)
            {
                return _extensions;
            }

            //toList required to ensure that multiple enumerations of the list are possible
            var additionalExtensionsList = additionalExtensions.ToList();
            if( !additionalExtensionsList.Any())
            {
                return _extensions;
            }

            var normalizedExtensions = NormalizeExtensions(additionalExtensionsList);
            return _extensions.Union(normalizedExtensions);
        }

        private IEnumerable<string> NormalizeExtensions(IEnumerable<string> additionalExtensions)
        {
            return additionalExtensions.Select(ext => (ext.StartsWith(".") ? ext : "." + ext).ToLowerInvariant());
        }
    }
}