#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
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
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace DotNetNuke.Web.Mvc.Routing
{
    public class RewrittenHttpRequest : HttpRequestBase
    {
        private readonly HttpRequestBase _wrappedRequest;
        private readonly string _newAppRelativeUrl;
        private readonly Uri _url;
        private readonly string _currentExecutionFilePath;

        public RewrittenHttpRequest(HttpRequestBase wrappedRequest, string newAppRelativePath)
        {
            _wrappedRequest = wrappedRequest;
            _newAppRelativeUrl = newAppRelativePath;

            if (_newAppRelativeUrl.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                _newAppRelativeUrl = _newAppRelativeUrl.Substring(2);
            }
            else if (_newAppRelativeUrl.StartsWith("~", StringComparison.OrdinalIgnoreCase) ||
                     _newAppRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                _newAppRelativeUrl = _newAppRelativeUrl.Substring(1);
            }

            string appPath = _wrappedRequest.ApplicationPath;
            if (appPath != null && !appPath.EndsWith("/"))
            {
                appPath += "/";
            }
            _currentExecutionFilePath = appPath + _newAppRelativeUrl;

            Uri originalUrl = _wrappedRequest.Url;

            // New Url copies the server, port and query but uses new path
            var newUrl = new StringBuilder();
            if (originalUrl != null)
            {
                newUrl.Append(originalUrl.Scheme);
                newUrl.Append("://");
                newUrl.Append(originalUrl.Host);
                if (originalUrl.Port != 80)
                {
                    newUrl.Append(":");
                    newUrl.Append(originalUrl.Port);
                }
                newUrl.Append(_currentExecutionFilePath);
                if (!String.IsNullOrEmpty(originalUrl.Query))
                {
                    newUrl.Append(originalUrl.Query);
                }
                if (!String.IsNullOrEmpty(originalUrl.Fragment))
                {
                    newUrl.Append(originalUrl.Fragment);
                }
            }

            _url = new Uri(newUrl.ToString());
        }

        public override string[] AcceptTypes
        {
            get
            {
                return _wrappedRequest.AcceptTypes;
            }
        }

        public override string AnonymousID
        {
            get
            {
                return _wrappedRequest.AnonymousID;
            }
        }

        public override string ApplicationPath
        {
            get
            {
                return _wrappedRequest.ApplicationPath;
            }
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get
            {
                return "~/" + _newAppRelativeUrl;
            }
        }

        public override byte[] BinaryRead(int count)
        {
            return _wrappedRequest.BinaryRead(count);
        }

        public override HttpBrowserCapabilitiesBase Browser
        {
            get
            {
                return _wrappedRequest.Browser;
            }
        }

        public override HttpClientCertificate ClientCertificate
        {
            get
            {
                return _wrappedRequest.ClientCertificate;
            }
        }

        public override Encoding ContentEncoding
        {
            get
            {
                return _wrappedRequest.ContentEncoding;
            }
            set
            {
                _wrappedRequest.ContentEncoding = value;
            }
        }

        public override int ContentLength
        {
            get
            {
                return _wrappedRequest.ContentLength;
            }
        }

        public override string ContentType
        {
            get
            {
                return _wrappedRequest.ContentType;
            }
            set
            {
                _wrappedRequest.ContentType = value;
            }
        }

        public override HttpCookieCollection Cookies
        {
            get
            {
                return _wrappedRequest.Cookies;
            }
        }

        public override string CurrentExecutionFilePath
        {
            get
            {
                return _currentExecutionFilePath;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        public override string FilePath
        {
            get
            {
                return CurrentExecutionFilePath;
            }
        }

        public override HttpFileCollectionBase Files
        {
            get
            {
                return _wrappedRequest.Files;
            }
        }

        public override Stream Filter
        {
            get
            {
                return _wrappedRequest.Filter;
            }
            set
            {
                _wrappedRequest.Filter = value;
            }
        }

        public override NameValueCollection Form
        {
            get
            {
                return _wrappedRequest.Form;
            }
        }

        public override int GetHashCode()
        {
            return _wrappedRequest.GetHashCode() ^ _newAppRelativeUrl.GetHashCode();
        }

        public override NameValueCollection Headers
        {
            get
            {
                return _wrappedRequest.Headers;
            }
        }

        public override string HttpMethod
        {
            get
            {
                return _wrappedRequest.HttpMethod;
            }
        }

        public override Stream InputStream
        {
            get
            {
                return _wrappedRequest.InputStream;
            }
        }

        public override bool IsAuthenticated
        {
            get
            {
                return _wrappedRequest.IsAuthenticated;
            }
        }

        public override bool IsLocal
        {
            get
            {
                return _wrappedRequest.IsLocal;
            }
        }

        public override bool IsSecureConnection
        {
            get
            {
                return _wrappedRequest.IsSecureConnection;
            }
        }

        public override WindowsIdentity LogonUserIdentity
        {
            get
            {
                return _wrappedRequest.LogonUserIdentity;
            }
        }

        public override int[] MapImageCoordinates(string imageFieldName)
        {
            return _wrappedRequest.MapImageCoordinates(imageFieldName);
        }

        public override string MapPath(string virtualPath)
        {
            return _wrappedRequest.MapPath(virtualPath);
        }

        public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
        {
            return _wrappedRequest.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
        }

        public override NameValueCollection Params
        {
            get
            {
                return _wrappedRequest.Params;
            }
        }

        public override string Path
        {
            get
            {
                return CurrentExecutionFilePath;
            }
        }

        public override string PathInfo
        {
            get
            {
                return String.Empty;
            }
        }

        public override string PhysicalApplicationPath
        {
            get
            {
                return _wrappedRequest.PhysicalApplicationPath;
            }
        }

        public override string PhysicalPath
        {
            get
            {
// ReSharper disable once AssignNullToNotNullAttribute
                return System.IO.Path.Combine(PhysicalApplicationPath, Path.TrimStart('/').Replace('/', '\\'));
            }
        }

        public override NameValueCollection QueryString
        {
            get
            {
                return _wrappedRequest.QueryString;
            }
        }

        public override string RawUrl
        {
            get
            {
                return CurrentExecutionFilePath;
            }
        }

        public override string RequestType
        {
            get
            {
                return _wrappedRequest.RequestType;
            }
            set
            {
                _wrappedRequest.RequestType = value;
            }
        }

        public override void SaveAs(string filename, bool includeHeaders)
        {
            _wrappedRequest.SaveAs(filename, includeHeaders);
        }

        public override NameValueCollection ServerVariables
        {
            get
            {
                return _wrappedRequest.ServerVariables;
            }
        }

        public override string this[string key]
        {
            get
            {
                return _wrappedRequest[key];
            }
        }

        public override string ToString()
        {
            return _wrappedRequest.ToString();
        }

        public override int TotalBytes
        {
            get
            {
                return _wrappedRequest.TotalBytes;
            }
        }

        public override Uri Url
        {
            get
            {
                return _url;
            }
        }

        public override Uri UrlReferrer
        {
            get
            {
                return _wrappedRequest.UrlReferrer;
            }
        }

        public override string UserAgent
        {
            get
            {
                return _wrappedRequest.UserAgent;
            }
        }

        public override string UserHostAddress
        {
            get
            {
                return _wrappedRequest.UserHostAddress;
            }
        }

        public override string UserHostName
        {
            get
            {
                return _wrappedRequest.UserHostName;
            }
        }

        public override string[] UserLanguages
        {
            get
            {
                return _wrappedRequest.UserLanguages;
            }
        }

        public override void ValidateInput()
        {
            _wrappedRequest.ValidateInput();
        }
    }
}