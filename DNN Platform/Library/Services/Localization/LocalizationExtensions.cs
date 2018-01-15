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
