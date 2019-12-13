// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings
using System.Text.RegularExpressions;
#endregion

namespace DotNetNuke.Services.Localization
{
    public static class LocalizationExtensions
    {
        public const string ResxFileLocaleRegex = "(?i)(.*)\\.((\\w\\w-)?\\w{2,3}-\\w{2,3})(\\.resx)$(?-i)";
        private static readonly Regex FileNameMatchRegex = new Regex(ResxFileLocaleRegex, RegexOptions.Compiled);

        /// <summary>
        /// Gets the name of the locale code from a resource file.
        /// E.g. My file with.fancy-characters.fr-FR.resx should return "fr-FR".
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Microsoft compatible locale code</returns>
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
        /// <returns>File name stripped of culture code or extension</returns>
        public static string GetFileNameFromLocalizedResxFile(this string fileName)
        {
            var m = FileNameMatchRegex.Match(fileName);
            return m.Success ? m.Groups[1].Value : string.Empty;
        }

    }
}
