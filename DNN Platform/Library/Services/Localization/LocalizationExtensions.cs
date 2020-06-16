// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System.Text.RegularExpressions;

    public static class LocalizationExtensions
    {
        public const string ResxFileLocaleRegex = "(?i)(.*)\\.((\\w\\w-)?\\w{2,3}-\\w{2,3})(\\.resx)$(?-i)";
        private static readonly Regex FileNameMatchRegex = new Regex(ResxFileLocaleRegex, RegexOptions.Compiled);

        /// <summary>
        /// Gets the name of the locale code from a resource file.
        /// E.g. My file with.fancy-characters.fr-FR.resx should return "fr-FR".
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Microsoft compatible locale code.</returns>
        public static string GetLocaleCodeFromFileName(this string fileName)
        {
            var m = FileNameMatchRegex.Match(fileName);
            return m.Success ? m.Groups[2].Value : string.Empty;
        }

        /// <summary>
        /// Gets the file name part from localized resource file.
        /// E.g. My file with.fancy-characters.fr-FR.resx should return "My file with.fancy-characters".
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>File name stripped of culture code or extension.</returns>
        public static string GetFileNameFromLocalizedResxFile(this string fileName)
        {
            var m = FileNameMatchRegex.Match(fileName);
            return m.Success ? m.Groups[1].Value : string.Empty;
        }
    }
}
