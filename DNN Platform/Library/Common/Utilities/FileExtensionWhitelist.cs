// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Security;

    /// <summary>The default <see cref="IFileExtensionAllowList"/> implementation.</summary>
    public class FileExtensionWhitelist : IFileExtensionAllowList
    {
        private readonly List<string> extensions;

        /// <summary>Initializes a new instance of the <see cref="FileExtensionWhitelist"/> class.</summary>
        /// <param name="extensionList">a comma seperated list of file extensions with no '.'.</param>
        /// <remarks><paramref name="extensionList"/>should match the format used in the FileExtensions Host setting specifically it
        /// should not have an '.' in the extensions (e.g. txt,jpg,png,doc).</remarks>
        public FileExtensionWhitelist(string extensionList)
        {
            this.extensions = EscapedString.Seperate(extensionList.ToLowerInvariant(), true).Select(item => "." + item).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<string> AllowedExtensions => this.extensions;

        /// <inheritdoc />
        public string ToDisplayString()
        {
            return this.ToDisplayString(null);
        }

        /// <inheritdoc />
        public string ToDisplayString(IEnumerable<string> additionalExtensions)
        {
            IEnumerable<string> allExtensions = this.CombineLists(additionalExtensions);
            return "*" + string.Join(", *", allExtensions.ToArray());
        }

        /// <inheritdoc />
        public bool IsAllowedExtension(string extension)
        {
            return this.IsAllowedExtension(extension, null);
        }

        /// <inheritdoc />
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ToDisplayString();
        }

        /// <inheritdoc />
        public string ToStorageString()
        {
            return this.ToStorageString(null);
        }

        /// <inheritdoc />
        public string ToStorageString(IEnumerable<string> additionalExtensions)
        {
            IEnumerable<string> allExtensions = this.CombineLists(additionalExtensions);
            var leadingDotRemoved = allExtensions.Select(ext => ext.Substring(1));
            return EscapedString.Combine(leadingDotRemoved);
        }

        /// <inheritdoc cref="IFileExtensionAllowList.RestrictBy"/>
        public FileExtensionWhitelist RestrictBy(FileExtensionWhitelist parentList)
        {
            var filter = parentList.extensions;
            return new FileExtensionWhitelist(string.Join(",", this.extensions.Where(x => filter.Contains(x)).Select(s => s.Substring(1))));
        }

        /// <inheritdoc />
        public IFileExtensionAllowList RestrictBy(IFileExtensionAllowList parentList)
        {
            var filter = parentList.AllowedExtensions;
            return new FileExtensionWhitelist(string.Join(",", this.extensions.Where(x => filter.Contains(x)).Select(s => s.Substring(1))));
        }

        private static IEnumerable<string> NormalizeExtensions(IEnumerable<string> additionalExtensions)
        {
            return additionalExtensions.Select(ext => (ext.StartsWith(".") ? ext : "." + ext).ToLowerInvariant());
        }

        private IEnumerable<string> CombineLists(IEnumerable<string> additionalExtensions)
        {
            if (additionalExtensions == null)
            {
                return this.extensions;
            }

            // toList required to ensure that multiple enumerations of the list are possible
            var additionalExtensionsList = additionalExtensions.ToList();
            if (!additionalExtensionsList.Any())
            {
                return this.extensions;
            }

            var normalizedExtensions = NormalizeExtensions(additionalExtensionsList);
            return this.extensions.Union(normalizedExtensions);
        }
    }
}
