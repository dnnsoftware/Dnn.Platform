using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace DotNetNuke.Modules.Journal.Components {
    public class Utilities {
        static internal Bitmap GetImageFromURL(string url) {
            string sImgName = string.Empty;
            System.Net.WebRequest myRequest = default(System.Net.WebRequest);
            Bitmap bmp = null;
            try {
                myRequest = System.Net.WebRequest.Create(url);
                myRequest.Proxy = null;
                using (WebResponse myResponse = myRequest.GetResponse()) {
                    using (Stream myStream = myResponse.GetResponseStream()) {
                        string sContentType = myResponse.ContentType;
                        string sExt = string.Empty;
                        if (sContentType.Contains("png")) {
                            sExt = ".png";
                        } else if (sContentType.Contains("jpg")) {
                            sExt = ".jpg";
                        } else if (sContentType.Contains("jpeg")) {
                            sExt = ".jpg";
                        } else if (sContentType.Contains("gif")) {
                            sExt = ".gif";
                        }
                        if (!string.IsNullOrEmpty(sExt)) {
                            bmp = new Bitmap(myStream);
                        }

                    }
                }

                return bmp;


            } catch {
                return null;
            }

        }

        static internal string PrepareURL(string url) {
            url = url.Trim();
            if (!url.StartsWith("http")) {
                url = "http://" + url;
            }
            if (url.Contains("https://")) {
                url = url.Replace("https://", "http://");
            }
            if (url.Contains("http://http://")) {
                url = url.Replace("http://http://", "http://");
            }
            if (!(url.IndexOf("http://") == 0)) {
                url = "http://" + url;
            }
            Uri objURI = null;

            objURI = new Uri(url);
            return url;

        }
        static internal LinkInfo GetLinkData(string URL) {
            string sPage = GetPageFromURL(ref URL, string.Empty, string.Empty);
            LinkInfo link = new LinkInfo();
            if (string.IsNullOrEmpty(sPage)) {
                return link;
            }
            string sTitle = string.Empty;
            string sDescription = string.Empty;
            string sImage = string.Empty;
            
            link.URL = URL;
            link.Images = new List<ImageInfo>();
            Match m = Regex.Match(sPage, "<(title)[^>]*?>((?:.|\\n)*?)</\\s*\\1\\s*>", RegexOptions.IgnoreCase & RegexOptions.Multiline);
            if (m.Success) {
                link.Title = m.Groups[2].ToString().Trim();
            }
            //
            Regex regExp = new Regex("<meta\\s*(?:(?:\\b(\\w|-)+\\b\\s*(?:=\\s*(?:\"[^\"]*\"|'[^']*'|[^\"'<> ]+)\\s*)?)*)/?\\s*>", RegexOptions.IgnoreCase & RegexOptions.Multiline);
            MatchCollection matches = default(MatchCollection);
            matches = regExp.Matches(sPage);
            int i = 0;
            foreach (Match match in matches) {
                string sTempDesc = match.Groups[0].Value;
                Regex subReg = new Regex("<meta[\\s]+[^>]*?(((name|property)*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?)|(content*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?))((content*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|(name*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                foreach (Match subM in subReg.Matches(sTempDesc)) {
                    if (subM.Groups[4].Value.ToUpperInvariant() == "OG:DESCRIPTION") {
                        link.Description = subM.Groups[9].Value;
                    } else if (subM.Groups[4].Value.ToUpperInvariant() == "DESCRIPTION".ToUpperInvariant()) {
                        link.Description = subM.Groups[9].Value;
                    }
                    if (subM.Groups[4].Value.ToUpperInvariant() == "OG:TITLE") {
                        link.Title = subM.Groups[9].Value;
                    }
                    
                    if (subM.Groups[4].Value.ToUpperInvariant() == "OG:IMAGE") {
                        sImage = subM.Groups[9].Value;
                        ImageInfo img = new ImageInfo();
                        img.URL = sImage;
                        link.Images.Add(img);
                        i += 1;
                    }
                }
            }
            if (!string.IsNullOrEmpty(link.Description)) {
                link.Description = HttpUtility.HtmlDecode(link.Description);
                link.Description = HttpUtility.UrlDecode(link.Description);
                link.Description = RemoveHTML(link.Description);
            }
            if (!string.IsNullOrEmpty(link.Title)) {
                link.Title = link.Title.Replace("&amp;", "&");
            }
            regExp = new Regex("<img[\\s]+[^>]*?((alt*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?)|(src*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?))((src*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|(alt*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            matches = regExp.Matches(sPage);

            string imgList = string.Empty;
            string hostUrl = string.Empty;
            if (!URL.Contains("http")) {
                URL = "http://" + URL;
            } 
            Uri uri = new Uri(URL);
            hostUrl = uri.Host;
            if (URL.Contains("https:")) {
                hostUrl = "https://" + hostUrl;
            } else {
                hostUrl = "http://" + hostUrl;
            }
            foreach (Match match in matches) {
                string sImg = match.Groups[5].Value;
                if (string.IsNullOrEmpty(sImg)) {
                    sImg = match.Groups[8].Value;
                }
                if (!string.IsNullOrEmpty(sImg)) {
                    if (!sImg.Contains("http")) {
                        sImg = hostUrl + sImg;
                    }
                  
                    ImageInfo img = new ImageInfo();
                    img.URL = sImg;
                    if (!imgList.Contains(sImg)) {
                        Bitmap bmp = Utilities.GetImageFromURL(sImg);
                        if ((bmp != null)) {
                            if (bmp.Height > 25 & bmp.Height < 500 & bmp.Width > 25 & bmp.Width < 500) {
                                link.Images.Add(img);
                                imgList += sImg;
                                i += 1;

                            }
                        }
                    }
                    if (i == 10) {
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }

            }
            return link;
        }
        static internal string GetPageFromURL(ref string url, string username, string password) {

            url = PrepareURL(url);
            HttpWebRequest objWebRequest = default(HttpWebRequest);
            HttpWebResponse objWebResponse = default(HttpWebResponse);
            CookieContainer cookies = new CookieContainer();
            Uri objURI = new Uri(url);
            objWebRequest = (HttpWebRequest)HttpWebRequest.Create(objURI);
            objWebRequest.KeepAlive = false;
            objWebRequest.Proxy = null;
            objWebRequest.CookieContainer = cookies;
            if (!string.IsNullOrEmpty(username) & !string.IsNullOrEmpty(password)) {
                NetworkCredential nc = new NetworkCredential(username, password);
                objWebRequest.Credentials = nc;
            }
            string sHTML = string.Empty;
            try {
                objWebResponse = (HttpWebResponse)objWebRequest.GetResponse();
                Encoding enc = Encoding.UTF8;


                string contentType = objWebResponse.ContentType;

                if ((objWebRequest.HaveResponse == true) & objWebResponse.StatusCode == HttpStatusCode.OK) {
                    objWebResponse.Cookies = objWebRequest.CookieContainer.GetCookies(objWebRequest.RequestUri);
                    using (Stream objStream = objWebResponse.GetResponseStream()) {
                        using (StreamReader objStreamReader = new StreamReader(objStream, enc)) {
                            sHTML = objStreamReader.ReadToEnd();
                            objStreamReader.Close();
                        }
                        objStream.Close();
                    }
                }
                objWebResponse.Close();
            } catch (Exception ex) {
                Services.Exceptions.Exceptions.LogException(ex);
            }
            
            return sHTML;
        }

        public static string LocalizeControl(string controlText) {
            string sKey = "";
            string sReplace = "";
            string pattern = "(\\{resx:.+?\\})";
            Regex regExp = new Regex(pattern);
            MatchCollection matches = default(MatchCollection);
            matches = regExp.Matches(controlText);
            foreach (Match match in matches) {
                sKey = match.Value;
                sReplace = GetSharedResource(sKey);
                
                string newValue = match.Value;
                if (!string.IsNullOrEmpty(sReplace)) {
                    newValue = sReplace;
                }
                controlText = controlText.Replace(sKey, newValue);
            }

            return controlText;
        }
        public static string GetSharedResource(string key) {
            string sValue = key;
            sValue = DotNetNuke.Services.Localization.Localization.GetString(key, Constants.SharedResourcesPath);
            if (sValue == string.Empty) {
                return key;
            } else {
                return sValue;
            }
        }

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
            string pattern = "<(.|\\n)*?>";
            sText = Regex.Replace(sText, pattern, string.Empty, RegexOptions.IgnoreCase);
            sText = HttpUtility.HtmlEncode(sText);
            return sText;
        }

    }
}