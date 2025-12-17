// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Upgrade;

    using Ganss.Xss;

    /// <summary>HtmlUtils is a Utility class that provides HTML Utility methods.</summary>
    public partial class HtmlUtils
    {
        // Create Regular Expression objects
        private const string PunctuationMatch = "[~!#\\$%\\^&*\\(\\)-+=\\{\\[\\}\\]\\|;:\\x22'<,>\\.\\?\\\\\\t\\r\\v\\f\\n]";

        private static readonly Regex HtmlDetectionRegex = new Regex("<(.*\\s*)>", RegexOptions.Compiled);
        private static readonly Regex StripWhiteSpaceRegex = new Regex("\\s+", RegexOptions.Compiled);
        private static readonly Regex StripNonWordRegex = new Regex("\\W*", RegexOptions.Compiled);
        private static readonly Regex StripTagsRegex = new Regex("<[^<>]*>", RegexOptions.Compiled);
        private static readonly Regex RemoveInlineStylesRegEx = new Regex("<style>.*?</style>", RegexOptions.Compiled | RegexOptions.Multiline);

        // Match all variants of <br> tag (<br>, <BR>, <br/>, including embedded space
        private static readonly Regex ReplaceHtmlNewLinesRegex = new Regex("\\s*<\\s*[bB][rR]\\s*/\\s*>\\s*", RegexOptions.Compiled);
        private static readonly Regex AfterRegEx = new Regex(PunctuationMatch + "\\s", RegexOptions.Compiled);
        private static readonly Regex BeforeRegEx = new Regex("\\s" + PunctuationMatch, RegexOptions.Compiled);
        private static readonly Regex EntityRegEx = new Regex("&[^;]+;", RegexOptions.Compiled);
        private static readonly Regex UrlEncodedRegEx = new Regex("%[0-9A-Fa-f]{2}", RegexOptions.Compiled);

        /// <summary>
        /// Clean removes any HTML Tags, Entities (and optionally any punctuation) from
        /// a string.
        /// </summary>
        /// <remarks>
        /// Encoded Tags are getting decoded, as they are part of the content!.
        /// </remarks>
        /// <param name="html">The HTML to clean.</param>
        /// <param name="removePunctuation">A flag indicating whether to remove punctuation.</param>
        /// <returns>The cleaned up string.</returns>
        public static string Clean(string html, bool removePunctuation)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            if (html.Contains("&lt;"))
            {
                // Fix when it is a double-encoded document
                html = HttpUtility.HtmlDecode(html);
            }

            // First remove any HTML Tags ("<....>")
            html = StripTags(html, true);

            // Second replace any HTML entities (&nbsp; &lt; etc) through their char symbol
            html = HttpUtility.HtmlDecode(html);

            // Thirdly remove any punctuation
            if (removePunctuation)
            {
                html = StripPunctuation(html, true);

                // When RemovePunctuation is false, HtmlDecode() would have already had removed these
                // Finally remove extra whitespace
                html = StripWhiteSpace(html, true);
            }

            return html;
        }

        /// <summary>CleanWithTagInfo removes unspecified HTML Tags, Entities (and optionally any punctuation) from a string.</summary>
        /// <param name="html">The HTML to clean.</param>
        /// <param name="tagsFilter">A regular expression indicating attributes of which to keep the value, e.g. <c>"alt|href|src|title"</c>.</param>
        /// <param name="removePunctuation">Whether to remove punctuation from <paramref name="html"/>.</param>
        /// <returns>The cleaned up string.</returns>
        public static string CleanWithTagInfo(string html, string tagsFilter, bool removePunctuation)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            // First remove unspecified HTML Tags ("<....>")
            html = StripUnspecifiedTags(html, tagsFilter, true);

            // Second replace any HTML entities (&nbsp; &lt; etc.) through their char symbol
            html = HttpUtility.HtmlDecode(html);

            // Thirdly remove any punctuation
            if (removePunctuation)
            {
                html = StripPunctuation(html, true);
            }

            // Finally remove extra whitespace
            html = StripWhiteSpace(html, true);

            return html;
        }

        /// <summary>Formats an Email address as a cloaked html link.</summary>
        /// <param name="email">The email address to format.</param>
        /// <returns>The email address as a cloaked html link.</returns>
        public static string FormatEmail(string email)
        {
            return FormatEmail(email, true);
        }

        /// <summary>Formats an Email address.</summary>
        /// <param name="email">The email address to format.</param>
        /// <param name="cloak">A flag that indicates whether the text should be cloaked.</param>
        /// <returns>The formatted email address.</returns>
        public static string FormatEmail(string email, bool cloak)
        {
            string formatEmail = string.Empty;
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(email.Trim()))
            {
                if (email.IndexOf("@", StringComparison.Ordinal) != -1)
                {
                    formatEmail = $"<a href=\"mailto:{email}\">{email}</a>";
                }
                else
                {
                    formatEmail = email;
                }
            }

            if (cloak)
            {
                formatEmail = Globals.CloakText(formatEmail);
            }

            return formatEmail;
        }

        /// <summary>FormatText replaces <c>&lt;br /&gt;</c> tags by LineFeed characters.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <param name="retainSpace">Whether retain Space.</param>
        /// <returns>The cleaned up string.</returns>
        public static string FormatText(string html, bool retainSpace)
        {
            return ReplaceHtmlNewLinesRegex.Replace(html, Environment.NewLine);
        }

        /// <summary>Formats <paramref name="strText"/> as HTML by replacing line feeds by <c>&lt;br /&gt;</c>.</summary>
        /// <param name="strText">Text to format.</param>
        /// <returns>The formatted HTML.</returns>
        public static string ConvertToHtml(string strText)
        {
            if (!string.IsNullOrEmpty(strText))
            {
                var htmlBuilder = new StringBuilder(strText);
                htmlBuilder.Replace("\r\n", "<br />");
                htmlBuilder.Replace("\n", "<br />");
                htmlBuilder.Replace("\r", "<br />");
                return htmlBuilder.ToString();
            }

            return strText;
        }

        /// <summary>Formats HTML as text by removing <c>&lt;br /&gt;</c> tags and replacing by linefeeds.</summary>
        /// <param name="strHtml">HTML to format.</param>
        /// <returns>The formatted text.</returns>
        public static string ConvertToText(string strHtml)
        {
            string strText = strHtml;

            if (!string.IsNullOrEmpty(strText))
            {
                // First remove white space (html does not render white space anyway and it screws up the conversion to text)
                // Replace it by a single space
                strText = StripWhiteSpace(strText, true);

                // Replace all variants of <br> by Linefeeds
                strText = FormatText(strText, false);
            }

            return strText;
        }

        /// <summary>Format a domain name including link.</summary>
        /// <param name="website">The domain name to format.</param>
        /// <returns>The formatted domain name.</returns>
        public static string FormatWebsite(object website)
        {
            string formatWebsite = string.Empty;
            if (website != DBNull.Value)
            {
                if (!string.IsNullOrEmpty(website.ToString().Trim()))
                {
                    if (website.ToString().IndexOf(".", StringComparison.Ordinal) > -1)
                    {
                        var prefix = website.ToString().IndexOf("://", StringComparison.Ordinal) > -1 ? string.Empty : "http://";
                        formatWebsite = $"<a href=\"{prefix}{website}\">{website}</a>";
                    }
                    else
                    {
                        formatWebsite = website.ToString();
                    }
                }
            }

            return formatWebsite;
        }

        /// <summary>Shorten returns the first <paramref name="length"/> characters of a string.</summary>
        /// <param name="txt">The text to reduce.</param>
        /// <param name="length">The max number of characters to return.</param>
        /// <param name="suffix">An optional suffix to append to the shortened string.</param>
        /// <returns>The shortened string.</returns>
        public static string Shorten(string txt, int length, string suffix)
        {
            if (!string.IsNullOrEmpty(txt) && txt.Length > length)
            {
                txt = txt.Substring(0, length) + suffix;
            }

            return txt;
        }

        /// <summary>StripEntities removes the HTML Entities from the content.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <param name="retainSpace">Indicates whether to replace the Entity by a space (<see langword="true"/>) or nothing (<see langword="false"/>).</param>
        /// <returns>The cleaned up string.</returns>
        [DnnDeprecated(7, 0, 0, "Please use System.Web.HtmlUtility.HtmlDecode", RemovalVersion = 11)]
        public static partial string StripEntities(string html, bool retainSpace)
        {
            var repString = retainSpace ? " " : string.Empty;

            // Replace Entities by replacement String and return modified string
            return EntityRegEx.Replace(html, repString);
        }

        /// <summary>Checks whether the string contains any HTML Entity or not.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <returns>True if the string contains any entity.</returns>
        public static bool ContainsEntity(string html)
        {
            return !string.IsNullOrEmpty(html) && EntityRegEx.IsMatch(html);
        }

        /// <summary>Checks whether the string contains any URL encoded entity or not.</summary>
        /// <param name="text">The string check.</param>
        /// <returns>True if the string contains any URL encoded entity.</returns>
        public static bool IsUrlEncoded(string text)
        {
            return !string.IsNullOrEmpty(text) && UrlEncodedRegEx.IsMatch(text);
        }

        /// <summary>Removes Inline CSS Styles.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <returns>The cleaned up string.</returns>
        public static string RemoveInlineStyle(string html)
        {
            return RemoveInlineStylesRegEx.Replace(html, string.Empty);
        }

        /// <summary>StripTags removes the HTML Tags from the content.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <param name="retainSpace">Indicates whether to replace the Tag by a space (<see langword="true"/>) or nothing (<see langword="false"/>).</param>
        /// <returns>The cleaned up string.</returns>
        public static string StripTags(string html, bool retainSpace)
        {
            return StripTagsRegex.Replace(html, retainSpace ? " " : string.Empty);
        }

        /// <summary>  StripUnspecifiedTags removes the HTML tags from the content -- leaving behind the info for the specified HTML tags. </summary>
        /// <param name="html">The HTML to clean.</param>
        /// <param name="specifiedTags">A regular expression indicating attributes of which to keep the value, e.g. <c>"alt|href|src|title"</c>.</param>
        /// <param name="retainSpace">Whether to replace tags with spaces or <see cref="string.Empty"/>.</param>
        /// <returns>The cleaned up string.</returns>
        public static string StripUnspecifiedTags(string html, string specifiedTags, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            // Set up Replacement String
            var repString = retainSpace ? " " : string.Empty;

            // Stripped HTML
            result.Append(StripTagsRegex.Replace(html, repString));

            // Adding Tag info from specified tags
            foreach (Match m in Regex.Matches(html, "(?<=(" + specifiedTags + ")=)\"(?<a>.*?)\""))
            {
                if (m.Value.Length > 0)
                {
                    result.Append(" " + m.Value);
                }
            }

            return result.ToString();
        }

        /// <summary>StripPunctuation removes the Punctuation from the content.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <param name="retainSpace">Indicates whether to replace the Punctuation by a space (true) or nothing (false).</param>
        /// <returns>The cleaned up string.</returns>
        public static string StripPunctuation(string html, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            // Define return string
            string retHTML = html + " "; // Make sure any punctuation at the end of the String is removed

            // Set up Replacement String
            var repString = retainSpace ? " " : string.Empty;
            while (BeforeRegEx.IsMatch(retHTML))
            {
                retHTML = BeforeRegEx.Replace(retHTML, repString);
            }

            while (AfterRegEx.IsMatch(retHTML))
            {
                retHTML = AfterRegEx.Replace(retHTML, repString);
            }

            // Return modified string after trimming leading and ending quotation marks
            return retHTML.Trim('"');
        }

        /// <summary>StripWhiteSpace removes the WhiteSpace from the content.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <param name="retainSpace">Indicates whether to replace the WhiteSpace by a space (true) or nothing (false).</param>
        /// <returns>The cleaned up string.</returns>
        public static string StripWhiteSpace(string html, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return Null.NullString;
            }

            return StripWhiteSpaceRegex.Replace(html, retainSpace ? " " : string.Empty);
        }

        /// <summary>StripNonWord removes any Non-Word Character from the content.</summary>
        /// <param name="html">The HTML content to clean up.</param>
        /// <param name="retainSpace">Indicates whether to replace the Non-Word Character by a space (true) or nothing (false).</param>
        /// <returns>The cleaned up string.</returns>
        public static string StripNonWord(string html, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            string repString = retainSpace ? " " : string.Empty;
            return StripNonWordRegex.Replace(html, repString);
        }

        /// <summary>Determines whether the passed in string contains any HTML tags.</summary>
        /// <param name="text">Text to be inspected.</param>
        /// <returns>True for HTML and False for plain text.</returns>
        public static bool IsHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return HtmlDetectionRegex.IsMatch(text);
        }

        /// <summary>WriteError outputs an Error Message during Install/Upgrade etc.</summary>
        /// <param name="response">The ASP.Net Response object.</param>
        /// <param name="file">The filename where the Error Occurred.</param>
        /// <param name="message">The error message.</param>
        public static void WriteError(HttpResponse response, string file, string message)
        {
            response.Write("<h2>Error Details</h2>");
            response.Write("<table cellspacing=0 cellpadding=0 border=0>");
            response.Write("<tr><td><b>File</b></td><td><b>" + file + "</b></td></tr>");
            response.Write("<tr><td><b>Error</b>&nbsp;&nbsp;</td><td><b>" + message + "</b></td></tr>");
            response.Write("</table>");
            response.Write("<br><br>");
            response.Flush();
        }

        /// <summary>WriteFeedback outputs a Feedback Line during Install/Upgrade etc.</summary>
        /// <param name="response">The ASP.Net Response object.</param>
        /// <param name="indent">The indent for this feedback message.</param>
        /// <param name="message">The feedback message.</param>
        public static void WriteFeedback(HttpResponse response, int indent, string message)
        {
            WriteFeedback(response, indent, message, true);
        }

        /// <summary>WriteFeedback outputs a Feedback Line during Install/Upgrade etc.</summary>
        /// <param name="response">The ASP.Net Response object.</param>
        /// <param name="indent">The indent for this feedback message.</param>
        /// <param name="message">The feedback message.</param>
        /// <param name="showtime">Show the timespan before the message.</param>
        public static void WriteFeedback(HttpResponse response, int indent, string message, bool showtime)
        {
            try
            {
                bool showInstallationMessages;
                string configSetting = Config.GetSetting("ShowInstallationMessages") ?? "true";
                if (bool.TryParse(configSetting, out showInstallationMessages) && showInstallationMessages)
                {
                    // Get the time of the feedback
                    TimeSpan timeElapsed = Upgrade.RunTime;
                    string strMessage = string.Empty;
                    if (showtime)
                    {
                        strMessage += timeElapsed.ToString().Substring(0, timeElapsed.ToString().LastIndexOf(".", StringComparison.Ordinal) + 4) + " -";
                    }

                    for (int i = 0; i <= indent; i++)
                    {
                        strMessage += "&nbsp;";
                    }

                    strMessage += message;
                    response.Write(strMessage);
                    response.Flush();
                }
            }
            catch (HttpException ex)
            {
                // Swallowing this for when requests have timed out. Log in case a listener is implemented
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
        }

        /// <summary>This method adds an empty char to the response stream to avoid closing http connection on long-running tasks.</summary>
        public static void WriteKeepAlive()
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            if (!HttpContext.Current.Request.RawUrl.ToLowerInvariant().Contains("install.aspx?"))
            {
                return;
            }

            var response = HttpContext.Current.Response;
            response.Write(" ");
            response.Flush();
        }

        /// <summary>WriteFooter outputs the Footer during Install/Upgrade etc.</summary>
        /// <param name="response">The ASP.Net Response object.</param>
        public static void WriteFooter(HttpResponse response)
        {
            response.Write("</body>");
            response.Write("</html>");
            response.Flush();
        }

        /// <summary>WriteHeader outputs the Header during Install/Upgrade etc.</summary>
        /// <param name="response">The ASP.Net Response object.</param>
        /// <param name="mode">The mode Install/Upgrade etc.</param>
        public static void WriteHeader(HttpResponse response, string mode)
        {
            // Set Response buffer to False
            response.Buffer = false;

            // create an install page if it does not exist already
            if (!File.Exists(HttpContext.Current.Server.MapPath("~/Install/Install.htm")))
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~/Install/Install.template.htm")))
                {
                    File.Copy(HttpContext.Current.Server.MapPath("~/Install/Install.template.htm"), HttpContext.Current.Server.MapPath("~/Install/Install.htm"));
                }
            }

            // read install page and insert into response stream
            if (File.Exists(HttpContext.Current.Server.MapPath("~/Install/Install.htm")))
            {
                response.Write(FileSystemUtils.ReadFile(HttpContext.Current.Server.MapPath("~/Install/Install.htm")));
            }

            switch (mode)
            {
                case "install":
                    response.Write("<h1>Installing DNN</h1>");
                    break;
                case "upgrade":
                    response.Write("<h1>Upgrading DNN</h1>");
                    break;
                case "addPortal":
                    response.Write("<h1>Adding New Portal</h1>");
                    break;
                case "installResources":
                    response.Write("<h1>Installing Resources</h1>");
                    break;
                case "executeScripts":
                    response.Write("<h1>Executing Scripts</h1>");
                    break;
                case "none":
                    response.Write("<h1>Nothing To Install At This Time</h1>");
                    break;
                case "noDBVersion":
                    response.Write("<h1>New DNN Database</h1>");
                    break;
                case "error":
                    response.Write("<h1>Error Installing DNN</h1>");
                    break;
                default:
                    response.Write("<h1>" + mode + "</h1>");
                    break;
            }

            response.Flush();
        }

        public static void WriteSuccessError(HttpResponse response, bool bSuccess)
        {
            WriteFeedback(response, 0, bSuccess ? "<font color='green'>Success</font><br>" : "<font color='red'>Error!</font><br>", false);
        }

        public static void WriteScriptSuccessError(HttpResponse response, bool bSuccess, string strLogFile)
        {
            if (bSuccess)
            {
                WriteFeedback(response, 0, "<font color='green'>Success</font><br>", false);
            }
            else
            {
                WriteFeedback(response, 0, "<font color='red'>Error!</font> (see " + strLogFile + " for more information)<br>", false);
            }
        }

        /// <summary>Searches the provided html for absolute hrefs that match the provided aliases and converts them to relative urls.</summary>
        /// <param name="html">The input html.</param>
        /// <param name="aliases">a list of aliases that should be made into relative urls.</param>
        /// <returns>html string.</returns>
        public static string AbsoluteToRelativeUrls(string html, IEnumerable<string> aliases)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            foreach (string portalAlias in aliases)
            {
                string searchAlias = portalAlias;
                if (!portalAlias.EndsWith("/", StringComparison.Ordinal))
                {
                    searchAlias = $"{portalAlias}/";
                }

                var exp = new Regex("((?:href|src)=&quot;)https?://" + searchAlias + "(.*?&quot;)", RegexOptions.IgnoreCase);

                if (portalAlias.Contains("/", StringComparison.Ordinal))
                {
                    html = exp.Replace(html, "$1" + portalAlias.Substring(portalAlias.IndexOf("/", StringComparison.Ordinal)) + "/$2");
                }
                else
                {
                    html = exp.Replace(html, "$1/$2");
                }
            }

            return html;
        }

        /// <inheritdoc cref="HttpUtility.JavaScriptStringEncode(string)"/>
        public static IHtmlString JavaScriptStringEncode(string value)
            => JavaScriptStringEncode(value, false);

        /// <inheritdoc cref="HttpUtility.JavaScriptStringEncode(string,bool)"/>
        public static IHtmlString JavaScriptStringEncode(string value, bool addDoubleQuotes)
            => new HtmlString(HttpUtility.JavaScriptStringEncode(value, addDoubleQuotes));

        /// <summary>Sanitize the given HTML, removing element which could include JavaScript.</summary>
        /// <param name="htmlInput">The HTML to sanitize.</param>
        /// <returns>The sanitized HTML.</returns>
        public static string CleanOutOfJavascript(string htmlInput)
        {
            var sanitizer = new HtmlSanitizer();

            // We need to disallow all attributes that might contain JS
            sanitizer.AllowedAttributes.Remove("onclick");
            sanitizer.AllowedAttributes.Remove("onmouseover");
            sanitizer.AllowedAttributes.Remove("onmouseout");
            sanitizer.AllowedAttributes.Remove("onkeypress");
            sanitizer.AllowedAttributes.Remove("onkeydown");
            sanitizer.AllowedAttributes.Remove("onkeyup");

            // We need to disallow tags like '<form action="javascript:submitForm()">'
            sanitizer.AllowedSchemes.Remove("javascript");

            // Tags like '<script>' are obviously not allowed
            sanitizer.AllowedTags.Remove("script");

            return sanitizer.Sanitize(htmlInput);
        }

        /// <summary>Determines whether the given <paramref name="htmlInput"/> contains any JavaScript.</summary>
        /// <param name="htmlInput">The HTML to check.</param>
        /// <returns><see langword="true"/> if <paramref name="htmlInput"/> contains JavaScript, otherwise <see langword="false"/>.</returns>
        public static bool ContainsJavaScript(string htmlInput)
        {
            if (string.IsNullOrEmpty(htmlInput))
            {
                return false;
            }

            string cleaned = CleanOutOfJavascript(htmlInput);

            // Strip all HTML syntax characters and whitespace for comparison
            string strippedOriginal = StripHtmlSyntax(htmlInput);
            string strippedCleaned = StripHtmlSyntax(cleaned);

            // If the stripped versions are different, JavaScript was likely removed
            return !string.Equals(strippedOriginal, strippedCleaned, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Sanitizes the given <paramref name="rawHtmlInput"/> if <paramref name="allowJavaScript"/> is <see langword="false"/>.</summary>
        /// <param name="rawHtmlInput">The raw HTML input.</param>
        /// <param name="allowJavaScript">Whether to allow JavaScript in the HTML.</param>
        /// <returns>The HTML, potentially sanitized.</returns>
        public static string SanitizeHtmlIfNeeded(string rawHtmlInput, bool allowJavaScript)
        {
            // If input is null or empty: nothing to do
            if (string.IsNullOrEmpty(rawHtmlInput))
            {
                return string.Empty;
            }

            // If JavaScript is not allowed: HTML must be sanitized
            if (!allowJavaScript)
            {
                return CleanOutOfJavascript(rawHtmlInput);
            }

            return rawHtmlInput;
        }

        private static string StripHtmlSyntax(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            // Remove all whitespace and HTML syntax characters
            return Regex.Replace(html, @"[\s<>/""'=]", string.Empty);
        }
    }
}
