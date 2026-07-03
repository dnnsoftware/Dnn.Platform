// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Services.Localization;

/// <summary>Utilities for the journal module.</summary>
public class Utilities
{
    private static readonly Regex PageRegex = new Regex(
        "<(title)[^>]*?>((?:.|\\n)*?)</\\s*\\1\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex MetaRegex = new Regex(
        "<meta\\s*(?:(?:\\b(\\w|-)+\\b\\s*(?:=\\s*(?:\"[^\"]*\"|'[^']*'|[^\"'<> ]+)\\s*)?)*)/?\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex MetaSubRegex = new Regex(
        "<meta[\\s]+[^>]*?(((name|property)*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?)|(content*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?))((content*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|(name*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|>)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex MetaSubRegex2 = new Regex(
        "<img[\\s]+[^>]*?((alt*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?)|(src*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?))((src*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|(alt*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|>)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex ResexRegex = new Regex("(\\{resx:.+?\\})", RegexOptions.Compiled);

    private static readonly Regex HtmlTextRegex = new Regex("<(.|\\n)*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Localizes a control.</summary>
    /// <param name="controlText">The text of the control to localize.</param>
    /// <returns>The localized text.</returns>
    public static string LocalizeControl(string controlText)
    {
        var matches = ResexRegex.Matches(controlText);
        foreach (Match match in matches)
        {
            var key = match.Value;
            var replace = GetSharedResource(key);

            var newValue = match.Value;
            if (!string.IsNullOrEmpty(replace))
            {
                newValue = replace;
            }

            controlText = controlText.Replace(key, newValue);
        }

        return controlText;
    }

    /// <summary>Gets a shared resource string for the specified key.</summary>
    /// <param name="key">The key of the shared resource string.</param>
    /// <returns>The shared resource value string.</returns>
    public static string GetSharedResource(string key)
    {
        var value = Localization.GetString(key, Constants.SharedResourcesPath);
        return value == string.Empty ? key : value;
    }

    /// <summary>Removes HTML tags from the specified text.</summary>
    /// <param name="sText">The text from which to remove HTML tags.</param>
    /// <returns>The text without HTML tags.</returns>
    public static string RemoveHTML(string sText)
    {
        if (string.IsNullOrEmpty(sText))
        {
            return string.Empty;
        }

        sText = HttpUtility.HtmlDecode(sText);
        sText = HttpUtility.UrlDecode(sText);
        sText = sText.Trim();
        if (string.IsNullOrEmpty(sText))
        {
            return string.Empty;
        }

        sText = HtmlTextRegex.Replace(sText, string.Empty);
        return HttpUtility.HtmlEncode(sText);
    }

    /// <summary>Determines whether two users are friends.</summary>
    /// <param name="profileUser">The profile user.</param>
    /// <param name="currentUser">The current user.</param>
    /// <returns><c>true</c> if the users are friends; otherwise, <c>false</c>.</returns>
    public static bool AreFriends(UserInfo profileUser, UserInfo currentUser)
    {
        var friendsRelationShip = RelationshipController.Instance.GetFriendRelationship(profileUser, currentUser);
        return friendsRelationShip is { Status: RelationshipStatus.Accepted, };
    }

    /// <summary>Gets an image from the specified URL.</summary>
    /// <param name="url">The URL of the image.</param>
    /// <returns>The image as a Bitmap, or null if the image could not be retrieved.</returns>
    internal static Bitmap GetImageFromURL(string url)
    {
        Bitmap bmp = null;
        try
        {
            if (!TryGetSafeUri(url, out var imageUri))
            {
                return null;
            }

            var myRequest = WebRequest.Create(imageUri);
            myRequest.Proxy = null;
            using var myResponse = myRequest.GetResponse();
            using var myStream = myResponse.GetResponseStream();
            var contentType = myResponse.ContentType;
            var extension = string.Empty;
            if (contentType.Contains("png"))
            {
                extension = ".png";
            }
            else if (contentType.Contains("jpg") || contentType.Contains("jpeg"))
            {
                extension = ".jpg";
            }
            else if (contentType.Contains("gif"))
            {
                extension = ".gif";
            }

            if (!string.IsNullOrEmpty(extension) && myStream is not null)
            {
                bmp = new Bitmap(myStream);
            }

            return bmp;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Prepares a URL for safe use by validating it and ensuring it uses the HTTP or HTTPS scheme.</summary>
    /// <param name="url">The URL to prepare.</param>
    /// <returns>The prepared URL.</returns>
    /// <exception cref="UriFormatException">Thrown if the URL is invalid or unsafe.</exception>
    internal static string PrepareURL(string url)
    {
        return !TryGetSafeUri(url, out var uri)
            ? throw new UriFormatException("Invalid or unsafe URL.")
            : uri.AbsoluteUri;
    }

    /// <summary>Gets link data from the specified URL, including title, description, and images.</summary>
    /// <param name="url">The URL of the page to retrieve link data from.</param>
    /// <returns>A <see cref="LinkInfo"/> object containing the link data.</returns>
    internal static LinkInfo GetLinkData(string url)
    {
        var page = GetPageFromURL(ref url, string.Empty, string.Empty);
        var link = new LinkInfo();
        if (string.IsNullOrEmpty(page))
        {
            return link;
        }

        link.URL = url;
        link.Images = [];
        var m = PageRegex.Match(page);
        if (m.Success)
        {
            link.Title = m.Groups[2].ToString().Trim();
        }

        var matches = MetaRegex.Matches(page);
        var i = 0;
        foreach (Match match in matches)
        {
            var sTempDesc = match.Groups[0].Value;
            foreach (Match subM in MetaSubRegex.Matches(sTempDesc))
            {
                if (subM.Groups[4].Value.Equals("OG:DESCRIPTION", StringComparison.InvariantCultureIgnoreCase) || subM.Groups[4].Value.Equals("DESCRIPTION", StringComparison.InvariantCultureIgnoreCase))
                {
                    link.Description = subM.Groups[9].Value;
                }

                if (subM.Groups[4].Value.Equals("OG:TITLE", StringComparison.InvariantCultureIgnoreCase))
                {
                    link.Title = subM.Groups[9].Value;
                }

                if (subM.Groups[4].Value.Equals("OG:IMAGE", StringComparison.InvariantCultureIgnoreCase))
                {
                    var image = subM.Groups[9].Value;
                    link.Images.Add(new ImageInfo { URL = image, });
                    i += 1;
                }
            }
        }

        if (!string.IsNullOrEmpty(link.Description))
        {
            link.Description = HttpUtility.HtmlDecode(link.Description);
            link.Description = HttpUtility.UrlDecode(link.Description);
            link.Description = RemoveHTML(link.Description);
        }

        if (!string.IsNullOrEmpty(link.Title))
        {
            link.Title = link.Title.Replace("&amp;", "&");
        }

        matches = MetaSubRegex2.Matches(page);

        var imgList = string.Empty;
        if (!url.Contains("http"))
        {
            url = $"http://{url}";
        }

        var uri = new Uri(url);
        var hostUrl = uri.Host;
        hostUrl = url.Contains("https:")
            ? $"https://{hostUrl}"
            : $"http://{hostUrl}";

        foreach (Match match in matches)
        {
            var imgSrc = match.Groups[5].Value;
            if (string.IsNullOrEmpty(imgSrc))
            {
                imgSrc = match.Groups[8].Value;
            }

            if (string.IsNullOrEmpty(imgSrc))
            {
                continue;
            }

            if (!imgSrc.Contains("http"))
            {
                imgSrc = hostUrl + imgSrc;
            }

            var img = new ImageInfo { URL = imgSrc, };
            if (!imgList.Contains(imgSrc))
            {
                var bmp = GetImageFromURL(imgSrc);
                if (bmp != null)
                {
                    if (bmp.Height > 25 & bmp.Height < 500 & bmp.Width > 25 & bmp.Width < 500)
                    {
                        link.Images.Add(img);
                        imgList += imgSrc;
                        i += 1;
                    }
                }
            }

            if (i == 10)
            {
                break;
            }
        }

        return link;
    }

    /// <summary>Gets the HTML content of a page from the specified URL, optionally using provided credentials for authentication.</summary>
    /// <param name="url">The URL of the page to retrieve.</param>
    /// <param name="username">The username for authentication (optional).</param>
    /// <param name="password">The password for authentication (optional).</param>
    /// <returns>The HTML content of the page as a string.</returns>
    internal static string GetPageFromURL(ref string url, string username, string password)
    {
        var cookies = new CookieContainer();

        Uri pageUri;
        try
        {
            url = PrepareURL(url);
            pageUri = new Uri(url);
        }
        catch (Exception ex)
        {
            Services.Exceptions.Exceptions.LogException(ex);
            return string.Empty;
        }

        var webRequest = (HttpWebRequest)WebRequest.Create(pageUri);
        webRequest.KeepAlive = false;
        webRequest.Proxy = null;
        webRequest.CookieContainer = cookies;
        if (!string.IsNullOrEmpty(username) & !string.IsNullOrEmpty(password))
        {
            webRequest.Credentials = new NetworkCredential(username, password);
        }

        var html = string.Empty;
        try
        {
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            if (webRequest.HaveResponse & webResponse.StatusCode == HttpStatusCode.OK)
            {
                webResponse.Cookies = webRequest.CookieContainer.GetCookies(webRequest.RequestUri);
                using var stream = webResponse.GetResponseStream();
                using var streamReader = new StreamReader(stream, Encoding.UTF8);
                html = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
            }

            webResponse.Close();
        }
        catch (Exception ex)
        {
            Services.Exceptions.Exceptions.LogException(ex);
        }

        return html;
    }

    private static bool TryGetSafeUri(string input, out Uri uri)
    {
        uri = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var url = input.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = $"http://{url}";
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            return false;
        }

        if (!(uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Block loopback hostnames explicitly. Even if DNS resolution changes, localhost and
        // loopback names should never be fetched by this server-side preview endpoint.
        if (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var addresses = Dns.GetHostAddresses(uri.DnsSafeHost);
            if (addresses.Length == 0)
            {
                return false;
            }

            if (addresses.Any(IsBlockedAddress))
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static bool IsBlockedAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        switch (address.AddressFamily)
        {
            case AddressFamily.InterNetwork:
                {
                    var bytes = address.GetAddressBytes();

                    // IPv4 ranges blocked for SSRF protection:
                    // - 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16 (RFC 1918 private networks)
                    // - 127.0.0.0/8 (loopback)
                    // - 169.254.0.0/16 (link-local/APIPA)
                    // These are non-public/internal address spaces and should not be reachable via URL preview.
                    return bytes[0] == 10
                           || bytes[0] == 127
                           || (bytes[0] == 169 && bytes[1] == 254)
                           || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                           || (bytes[0] == 192 && bytes[1] == 168);
                }

            case AddressFamily.InterNetworkV6:
                {
                    var bytes = address.GetAddressBytes();
                    var uniqueLocal = (bytes[0] & 0xFE) == 0xFC;

                    // IPv6 ranges blocked for the same reason as IPv4 internal ranges:
                    // - ::1 (loopback)
                    // - fe80::/10 (link-local)
                    // - fec0::/10 (site-local, deprecated but still non-public)
                    // - fc00::/7 (unique local addresses)
                    // These are internal/non-routable scopes and should not be fetched by server-side previews.
                    return address.Equals(IPAddress.IPv6Loopback)
                           || address.IsIPv6LinkLocal
                           || address.IsIPv6SiteLocal
                           || uniqueLocal;
                }

            default:
                return false;
        }
    }
}
