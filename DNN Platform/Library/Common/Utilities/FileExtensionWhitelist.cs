// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FileExtensionWhitelist
    {
        private readonly List<string> _extensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExtensionWhitelist"/> class.
        /// </summary>
        /// <param name="extensionList">a comma seperated list of file extensions with no '.'.</param>
        /// <remarks><paramref name="extensionList"/>should match the format used in the FileExtensions Host setting specifically it
        /// should not have an '.' in the extensions (e.g. txt,jpg,png,doc).</remarks>
        public FileExtensionWhitelist(string extensionList)
        {
            this._extensions = EscapedString.Seperate(extensionList.ToLowerInvariant()).Select(item => "." + item).ToList();
        }

        /// <summary>
        /// Gets the list of extensions in the whitelist.
        /// </summary>
        /// <remarks>All extensions are lowercase and prefixed with a '.'.</remarks>
        public IEnumerable<string> AllowedExtensions
        {
            get
            {
                return this._extensions;
            }
        }

        /// <summary>
        /// Returns a string suitale for display to an end user.
        /// </summary>
        /// <returns>A String of the whitelist extensions formatted for display to an end user.</returns>
        public string ToDisplayString()
        {
            return this.ToDisplayString(null);
        }

        /// <summary>
        /// Formats the extension whitelist appropriate for display to an end user.
        /// </summary>
        /// <param name="additionalExtensions">A list of additionalExtensions to add to the current extensions.</param>
        /// <remarks><paramref name="additionalExtensions"/>case and '.' prefix will be corrected, and duplicates will be excluded from the string.</remarks>
        /// <returns>A String of the whitelist extensions formatted for storage display to an end user.</returns>
        public string ToDisplayString(IEnumerable<string> additionalExtensions)
        {
            IEnumerable<string> allExtensions = this.CombineLists(additionalExtensions);
            return "*" + string.Join(", *", allExtensions.ToArray());
        }

        /// <summary>
        /// Indicates if the file extension is permitted by the Host Whitelist.
        /// </summary>
        /// <param name="extension">The file extension with or without preceding '.'.</param>
        /// <returns>True if extension is in whitelist or whitelist is empty.  False otherwise.</returns>
        public bool IsAllowedExtension(string extension)
        {
            return this.IsAllowedExtension(extension, null);
        }

        /// <summary>
        /// Indicates if the file extension is permitted by the Host Whitelist.
        /// </summary>
        /// <param name="extension">The file extension with or without preceding '.'.</param>
        /// <param name="additionalExtensions"></param>
        /// <returns>True if extension is in whitelist or whitelist is empty.  False otherwise.</returns>
        public bool IsAllowedExtension(string extension, IEnumerable<string> additionalExtensions)
        {
            List<string> allExtensions = this.CombineLists(additionalExtensions).ToList();
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
            return this.ToDisplayString();
        }

        /// <summary>
        /// Formats the extension whitelist appropriate for storage in the Host setting.
        /// </summary>
        /// <returns>A String of the whitelist extensions formatted for storage as a Host setting.</returns>
        public string ToStorageString()
        {
            return this.ToStorageString(null);
        }

        /// <summary>
        /// Formats the extension whitelist appropriate for storage in the Host setting.
        /// </summary>
        /// <param name="additionalExtensions">A list of additionalExtensions to add to the current extensions.</param>
        /// <remarks><paramref name="additionalExtensions"/>case and '.' prefix will be corrected, and duplicates will be excluded from the string.</remarks>
        /// <returns>A String of the whitelist extensions formatted for storage as a Host setting.</returns>
        public string ToStorageString(IEnumerable<string> additionalExtensions)
        {
            IEnumerable<string> allExtensions = this.CombineLists(additionalExtensions);
            var leadingDotRemoved = allExtensions.Select(ext => ext.Substring(1));
            return EscapedString.Combine(leadingDotRemoved);
        }

        public FileExtensionWhitelist RestrictBy(FileExtensionWhitelist parentList)
        {
            var filter = parentList._extensions;
            return new FileExtensionWhitelist(string.Join(",", this._extensions.Where(x => filter.Contains(x)).Select(s => s.Substring(1))));
        }

        private IEnumerable<string> CombineLists(IEnumerable<string> additionalExtensions)
        {
            if (additionalExtensions == null)
            {
                return this._extensions;
            }

            // toList required to ensure that multiple enumerations of the list are possible
            var additionalExtensionsList = additionalExtensions.ToList();
            if (!additionalExtensionsList.Any())
            {
                return this._extensions;
            }

            var normalizedExtensions = this.NormalizeExtensions(additionalExtensionsList);
            return this._extensions.Union(normalizedExtensions);
        }

        private IEnumerable<string> NormalizeExtensions(IEnumerable<string> additionalExtensions)
        {
            return additionalExtensions.Select(ext => (ext.StartsWith(".") ? ext : "." + ext).ToLowerInvariant());
        }
    }
}
