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
using System.Collections;
using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Profile;

namespace DotNetNuke.Web.Mvc.Routing
{
    public class RewrittenHttpContext : HttpContextBase
    {
        private readonly HttpContextBase _wrappedContext;
        private readonly HttpRequestBase _rewrittenRequest;

        public RewrittenHttpContext(HttpContextBase wrappedContext, string appRelativePath)
        {
            _wrappedContext = wrappedContext;
            _rewrittenRequest = new RewrittenHttpRequest(wrappedContext.Request, appRelativePath);
        }

        public override void AddError(Exception errorInfo)
        {
            _wrappedContext.AddError(errorInfo);
        }

        public override Exception[] AllErrors
        {
            get
            {
                return _wrappedContext.AllErrors;
            }
        }

        public override HttpApplicationStateBase Application
        {
            get
            {
                return _wrappedContext.Application;
            }
        }

        public override HttpApplication ApplicationInstance
        {
            get
            {
                return _wrappedContext.ApplicationInstance;
            }
            set
            {
                _wrappedContext.ApplicationInstance = value;
            }
        }

        public override Cache Cache
        {
            get
            {
                return _wrappedContext.Cache;
            }
        }

        public override void ClearError()
        {
            _wrappedContext.ClearError();
        }

        public override IHttpHandler CurrentHandler
        {
            get
            {
                return _wrappedContext.CurrentHandler;
            }
        }

        public override RequestNotification CurrentNotification
        {
            get
            {
                return _wrappedContext.CurrentNotification;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        public override Exception Error
        {
            get
            {
                return _wrappedContext.Error;
            }
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey)
        {
            return _wrappedContext.GetGlobalResourceObject(classKey, resourceKey);
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey, CultureInfo culture)
        {
            return _wrappedContext.GetGlobalResourceObject(classKey, resourceKey, culture);
        }

        public override int GetHashCode()
        {
            return _wrappedContext.GetHashCode() ^ _rewrittenRequest.GetHashCode();
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey)
        {
            return _wrappedContext.GetLocalResourceObject(virtualPath, resourceKey);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey, CultureInfo culture)
        {
            return _wrappedContext.GetLocalResourceObject(virtualPath, resourceKey, culture);
        }

        public override object GetSection(string sectionName)
        {
            return _wrappedContext.GetSection(sectionName);
        }

        public override object GetService(Type serviceType)
        {
            return _wrappedContext.GetService(serviceType);
        }

        public override IHttpHandler Handler
        {
            get
            {
                return _wrappedContext.Handler;
            }
            set
            {
                _wrappedContext.Handler = value;
            }
        }

        public override bool IsCustomErrorEnabled
        {
            get
            {
                return _wrappedContext.IsCustomErrorEnabled;
            }
        }

        public override bool IsDebuggingEnabled
        {
            get
            {
                return _wrappedContext.IsDebuggingEnabled;
            }
        }

        public override bool IsPostNotification
        {
            get
            {
                return _wrappedContext.IsPostNotification;
            }
        }

        public override IDictionary Items
        {
            get
            {
                return _wrappedContext.Items;
            }
        }

        public override IHttpHandler PreviousHandler
        {
            get
            {
                return _wrappedContext.PreviousHandler;
            }
        }

        public override ProfileBase Profile
        {
            get
            {
                return _wrappedContext.Profile;
            }
        }

        public override HttpRequestBase Request
        {
            get
            {
                return _rewrittenRequest;
            }
        }

        public override HttpResponseBase Response
        {
            get
            {
                return _wrappedContext.Response;
            }
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString)
        {
            _wrappedContext.RewritePath(filePath, pathInfo, queryString);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath)
        {
            _wrappedContext.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
        }

        public override void RewritePath(string path)
        {
            _wrappedContext.RewritePath(path);
        }

        public override void RewritePath(string path, bool rebaseClientPath)
        {
            _wrappedContext.RewritePath(path, rebaseClientPath);
        }

        public override HttpServerUtilityBase Server
        {
            get
            {
                return _wrappedContext.Server;
            }
        }

        public override HttpSessionStateBase Session
        {
            get
            {
                return _wrappedContext.Session;
            }
        }

        public override bool SkipAuthorization
        {
            get
            {
                return _wrappedContext.SkipAuthorization;
            }
            set
            {
                _wrappedContext.SkipAuthorization = value;
            }
        }

        public override DateTime Timestamp
        {
            get
            {
                return _wrappedContext.Timestamp;
            }
        }

        public override string ToString()
        {
            return _wrappedContext.ToString();
        }

        public override TraceContext Trace
        {
            get
            {
                return _wrappedContext.Trace;
            }
        }

        public override IPrincipal User
        {
            get
            {
                return _wrappedContext.User;
            }
            set
            {
                _wrappedContext.User = value;
            }
        }
    }
}