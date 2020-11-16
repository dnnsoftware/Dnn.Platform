// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities.HttpSimulator
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.SessionState;

    public enum HttpVerb
    {
        GET,
        HEAD,
        POST,
        PUT,
        DELETE,
    }

    /// <summary>
    /// Useful class for simulating the HttpContext. This does not actually
    /// make an HttpRequest, it merely simulates the state that your code
    /// would be in "as if" handling a request. Thus the HttpContext.Current
    /// property is populated.
    /// </summary>
    public class HttpSimulator : IDisposable
    {
        private static readonly string WebsitePhysicalAppPath = ConfigurationManager.AppSettings["DefaultPhysicalAppPath"];
        private readonly NameValueCollection _formVars = new NameValueCollection();
        private readonly NameValueCollection _headers = new NameValueCollection();
        private StringBuilder _builder;
        private Uri _referer;

        private string _applicationPath = "/";

        private string _physicalApplicationPath = WebsitePhysicalAppPath;

        private string _physicalPath = WebsitePhysicalAppPath;

        public HttpSimulator()
            : this("/", WebsitePhysicalAppPath)
        {
        }

        public HttpSimulator(string applicationPath)
            : this(applicationPath, WebsitePhysicalAppPath)
        {
        }

        public HttpSimulator(string applicationPath, string physicalApplicationPath)
        {
            this.ApplicationPath = applicationPath;
            this.PhysicalApplicationPath = physicalApplicationPath;
        }

        /// <summary>
        /// Gets physical path to the requested file (used for simulation purposes).
        /// </summary>
        public string PhysicalPath
        {
            get { return this._physicalPath; }
        }

        /// <summary>
        /// Gets the text from the response to the simulated request.
        /// </summary>
        public string ResponseText
        {
            get
            {
                return (this._builder ?? new StringBuilder()).ToString();
            }
        }

        public string Host { get; private set; }

        public string LocalPath { get; private set; }

        public int Port { get; private set; }

        /// <summary>
        /// Gets portion of the URL after the application.
        /// </summary>
        public string Page { get; private set; }

        /// <summary>
        /// Gets or sets the same thing as the IIS Virtual directory. It's
        /// what gets returned by Request.ApplicationPath.
        /// </summary>
        public string ApplicationPath
        {
            get { return this._applicationPath; }

            set
            {
                this._applicationPath = value ?? "/";
                this._applicationPath = NormalizeSlashes(this._applicationPath);
            }
        }

        /// <summary>
        /// Gets or sets physical path to the application (used for simulation purposes).
        /// </summary>
        public string PhysicalApplicationPath
        {
            get { return this._physicalApplicationPath; }

            set
            {
                this._physicalApplicationPath = value ?? WebsitePhysicalAppPath;

                // strip trailing backslashes.
                this._physicalApplicationPath = StripTrailingBackSlashes(this._physicalApplicationPath) + @"\";
            }
        }

        public TextWriter ResponseWriter { get; set; }

        public SimulatedHttpRequest WorkerRequest { get; private set; }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a GET request.
        /// </summary>
        /// <remarks>
        /// Simulates a request to http://localhost/.
        /// </remarks>
        /// <returns></returns>
        public HttpSimulator SimulateRequest()
        {
            return this.SimulateRequest(new Uri("http://localhost/"));
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a GET request.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url)
        {
            return this.SimulateRequest(url, HttpVerb.GET);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb)
        {
            return this.SimulateRequest(url, httpVerb, null, null);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a POST request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formVariables"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, NameValueCollection formVariables)
        {
            return this.SimulateRequest(url, HttpVerb.POST, formVariables, null);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a POST request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formVariables"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, NameValueCollection formVariables, NameValueCollection headers)
        {
            return this.SimulateRequest(url, HttpVerb.POST, formVariables, headers);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb, NameValueCollection headers)
        {
            return this.SimulateRequest(url, httpVerb, null, headers);
        }

        /// <summary>
        /// Sets the referer for the request. Uses a fluent interface.
        /// </summary>
        /// <param name="referer"></param>
        /// <returns></returns>
        public HttpSimulator SetReferer(Uri referer)
        {
            if (this.WorkerRequest != null)
            {
                this.WorkerRequest.SetReferer(referer);
            }

            this._referer = referer;
            return this;
        }

        /// <summary>
        /// Sets a form variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpSimulator SetFormVariable(string name, string value)
        {
            if (this.WorkerRequest != null)
            {
                throw new InvalidOperationException("Cannot set form variables after calling Simulate().");
            }

            this._formVars.Add(name, value);

            return this;
        }

        /// <summary>
        /// Sets a header value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpSimulator SetHeader(string name, string value)
        {
            if (this.WorkerRequest != null)
            {
                throw new InvalidOperationException("Cannot set headers after calling Simulate().");
            }

            this._headers.Add(name, value);

            return this;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2.</filterpriority>
        public void Dispose()
        {
            HttpContext.Current = null;
        }

        protected static HostingEnvironment GetHostingEnvironment()
        {
            HostingEnvironment environment;
            try
            {
                environment = new HostingEnvironment();
            }
            catch (InvalidOperationException)
            {
                // Shoot, we need to grab it via reflection.
                environment = ReflectionHelper.GetStaticFieldValue<HostingEnvironment>("_theHostingEnvironment", typeof(HostingEnvironment));
            }

            return environment;
        }

        protected static string NormalizeSlashes(string s)
        {
            if (string.IsNullOrEmpty(s) || s == "/")
            {
                return "/";
            }

            s = s.Replace(@"\", "/");

            // Reduce multiple slashes in row to single.
            var normalized = Regex.Replace(s, "(/)/+", "$1");

            // Strip left.
            normalized = StripPrecedingSlashes(normalized);

            // Strip right.
            normalized = StripTrailingSlashes(normalized);
            return "/" + normalized;
        }

        protected static string StripPrecedingSlashes(string s)
        {
            return Regex.Replace(s, "^/*(.*)", "$1");
        }

        protected static string StripTrailingSlashes(string s)
        {
            return Regex.Replace(s, "(.*)/*$", "$1", RegexOptions.RightToLeft);
        }

        protected static string StripTrailingBackSlashes(string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : Regex.Replace(s, @"(.*)\\*$", "$1", RegexOptions.RightToLeft);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <param name="formVariables"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected virtual HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb, NameValueCollection formVariables, NameValueCollection headers)
        {
            HttpContext.Current = null;

            this.ParseRequestUrl(url);

            if (this.ResponseWriter == null)
            {
                this._builder = new StringBuilder();
                this.ResponseWriter = new StringWriter(this._builder);
            }

            this.SetHttpRuntimeInternals();

            var query = ExtractQueryStringPart(url);

            if (formVariables != null)
            {
                this._formVars.Add(formVariables);
            }

            if (this._formVars.Count > 0)
            {
                httpVerb = HttpVerb.POST; // Need to enforce this.
            }

            if (headers != null)
            {
                this._headers.Add(headers);
            }

            this.WorkerRequest = new SimulatedHttpRequest(this.ApplicationPath, this.PhysicalApplicationPath, this.PhysicalPath, this.Page, query, this.ResponseWriter, this.Host, this.Port, httpVerb.ToString());

            this.WorkerRequest.Form.Add(this._formVars);
            this.WorkerRequest.Headers.Add(this._headers);

            if (this._referer != null)
            {
                this.WorkerRequest.SetReferer(this._referer);
            }

            this.InitializeSession();

            InitializeApplication();

            // Console.WriteLine("host: " + Host);
            // Console.WriteLine("virtualDir: " + _applicationPath);
            // Console.WriteLine("page: " + LocalPath);
            // Console.WriteLine("pathPartAfterApplicationPart: " + Page);
            // Console.WriteLine("appPhysicalDir: " + _physicalApplicationPath);
            // if (HttpContext.Current != null)
            // {
            //    Console.WriteLine("Request.Url.LocalPath: " + HttpContext.Current.Request.Url.LocalPath);
            //    Console.WriteLine("Request.Url.Host: " + HttpContext.Current.Request.Url.Host);
            //    Console.WriteLine("Request.FilePath: " + HttpContext.Current.Request.FilePath);
            //    Console.WriteLine("Request.Path: " + HttpContext.Current.Request.Path);
            //    Console.WriteLine("Request.RawUrl: " + HttpContext.Current.Request.RawUrl);
            //    Console.WriteLine("Request.Url: " + HttpContext.Current.Request.Url);
            //    Console.WriteLine("Request.Url.Port: " + HttpContext.Current.Request.Url.Port);
            //    Console.WriteLine("Request.ApplicationPath: " + HttpContext.Current.Request.ApplicationPath);
            //    Console.WriteLine("Request.PhysicalPath: " + HttpContext.Current.Request.PhysicalPath);
            // }
            // Console.WriteLine("HttpRuntime.AppDomainAppPath: " + HttpRuntime.AppDomainAppPath);
            // Console.WriteLine("HttpRuntime.AppDomainAppVirtualPath: " + HttpRuntime.AppDomainAppVirtualPath);
            // Console.WriteLine("HostingEnvironment.ApplicationPhysicalPath: " + HostingEnvironment.ApplicationPhysicalPath);
            // Console.WriteLine("HostingEnvironment.ApplicationVirtualPath: " + HostingEnvironment.ApplicationVirtualPath);
            return this;
        }

        private static void InitializeApplication()
        {
            var appFactoryType = Type.GetType("System.Web.HttpApplicationFactory, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var appFactory = ReflectionHelper.GetStaticFieldValue<object>("_theApplicationFactory", appFactoryType);
            ReflectionHelper.SetPrivateInstanceFieldValue("_state", appFactory, HttpContext.Current.Application);
        }

        private static string RightAfter(string original, string search)
        {
            if (search.Length > original.Length || search.Length == 0)
            {
                return original;
            }

            original = original.Trim();
            search = search.Trim();
            var searchIndex = original.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase);

            if (searchIndex < 0)
            {
                return original;
            }

            // mod to add one onto the search length - otherwise strange results??
            // return original.Substring(original.IndexOf(search) + search.Length + 1);
            // return original.Substring(original.IndexOf(search) + search.Length);
            // original = original.Substring(searchIndex, original.Length - searchIndex);
            // return original.Replace(search,"");
            var regexMatch = @"(" + search + ")(?<keep>.+)";
            const string regexReplace = @"${keep}";
            var result = Regex.Replace(original, regexMatch, regexReplace, RegexOptions.IgnoreCase);
            return result;
        }

        private static string ExtractQueryStringPart(Uri url)
        {
            var query = url.Query;
            return query.StartsWith("?") ? query.Substring(1) : query;
        }

        private void InitializeSession()
        {
            HttpContext.Current = new HttpContext(this.WorkerRequest);
            HttpContext.Current.Items.Clear();
            var session = (HttpSessionState)ReflectionHelper.Instantiate(typeof(HttpSessionState), new[] { typeof(IHttpSessionState) }, new FakeHttpSessionState());

            HttpContext.Current.Items.Add("AspSession", session);
        }

        private void ParseRequestUrl(Uri url)
        {
            if (url == null)
            {
                return;
            }

            this.Host = url.Host;
            this.Port = url.Port;
            this.LocalPath = url.LocalPath;
            this.Page = StripPrecedingSlashes(RightAfter(url.LocalPath, this.ApplicationPath));
            this._physicalPath = Path.Combine(this._physicalApplicationPath, this.Page.Replace("/", @"\"));
        }

        private void SetHttpRuntimeInternals()
        {
            // We cheat by using reflection.

            // get singleton property value
            var runtime = ReflectionHelper.GetStaticFieldValue<HttpRuntime>("_theRuntime", typeof(HttpRuntime));

            // set app path property value
            ReflectionHelper.SetPrivateInstanceFieldValue("_appDomainAppPath", runtime, this.PhysicalApplicationPath);

            // set app virtual path property value
            const string vpathTypeName = "System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            var virtualPath = ReflectionHelper.Instantiate(vpathTypeName, new[] { typeof(string) }, new object[] { this.ApplicationPath });
            ReflectionHelper.SetPrivateInstanceFieldValue("_appDomainAppVPath", runtime, virtualPath);

            // set codegen dir property value
            ReflectionHelper.SetPrivateInstanceFieldValue("_codegenDir", runtime, this.PhysicalApplicationPath);

            var environment = GetHostingEnvironment();
            ReflectionHelper.SetPrivateInstanceFieldValue("_appPhysicalPath", environment, this.PhysicalApplicationPath);
            ReflectionHelper.SetPrivateInstanceFieldValue("_appVirtualPath", environment, virtualPath);
            ReflectionHelper.SetPrivateInstanceFieldValue("_configMapPath", environment, new ConfigMapPath(this));
        }

        public class FakeHttpSessionState : NameObjectCollectionBase, IHttpSessionState
        {
            private const bool _isNewSession = true;
            private readonly string _sessionId = Guid.NewGuid().ToString();
            private readonly HttpStaticObjectsCollection _staticObjects = new HttpStaticObjectsCollection();
            private readonly object _syncRoot = new object();
            private int _timeout = 30; // minutes

            /// <summary>
            /// Gets the unique session identifier for the session.
            /// </summary>
            ///
            /// <returns>
            /// The session ID.
            /// </returns>
            ///
            public string SessionID
            {
                get { return this._sessionId; }
            }

            /// <summary>
            /// Gets a value indicating whether the session was created with the current request.
            /// </summary>
            ///
            /// <returns>
            /// true if the session was created with the current request; otherwise, false.
            /// </returns>
            ///
            public bool IsNewSession
            {
                get { return _isNewSession; }
            }

            /// <summary>
            /// Gets the current session-state mode.
            /// </summary>
            ///
            /// <returns>
            /// One of the <see cref="T:System.Web.SessionState.SessionStateMode"></see> values.
            /// </returns>
            ///
            public SessionStateMode Mode
            {
                get { return SessionStateMode.InProc; }
            }

            /// <summary>
            /// Gets a value indicating whether the session ID is embedded in the URL or stored in an HTTP cookie.
            /// </summary>
            ///
            /// <returns>
            /// true if the session is embedded in the URL; otherwise, false.
            /// </returns>
            ///
            public bool IsCookieless
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value that indicates whether the application is configured for cookieless sessions.
            /// </summary>
            ///
            /// <returns>
            /// One of the <see cref="T:System.Web.HttpCookieMode"></see> values that indicate whether the application is configured for cookieless sessions. The default is <see cref="F:System.Web.HttpCookieMode.UseCookies"></see>.
            /// </returns>
            ///
            public HttpCookieMode CookieMode
            {
                get { return HttpCookieMode.UseCookies; }
            }

            /// <summary>
            /// Gets a collection of objects declared by &lt;object Runat="Server" Scope="Session"/&gt; tags within the ASP.NET application file Global.asax.
            /// </summary>
            ///
            /// <returns>
            /// An <see cref="T:System.Web.HttpStaticObjectsCollection"></see> containing objects declared in the Global.asax file.
            /// </returns>
            ///
            public HttpStaticObjectsCollection StaticObjects
            {
                get { return this._staticObjects; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the collection of session-state values.
            /// </summary>
            ///
            /// <returns>
            /// An object that can be used to synchronize access to the collection.
            /// </returns>
            ///
            public object SyncRoot
            {
                get { return this._syncRoot; }
            }

            /// <summary>
            /// Gets a value indicating whether access to the collection of session-state values is synchronized (thread safe).
            /// </summary>
            /// <returns>
            /// true if access to the collection is synchronized (thread safe); otherwise, false.
            /// </returns>
            ///
            public bool IsSynchronized
            {
                get { return true; }
            }

            /// <summary>
            /// Gets or sets and sets the time-out period (in minutes) allowed between requests before the session-state provider terminates the session.
            /// </summary>
            ///
            /// <returns>
            /// The time-out period, in minutes.
            /// </returns>
            ///
            public int Timeout
            {
                get { return this._timeout; }
                set { this._timeout = value; }
            }

            /// <summary>
            /// Gets or sets the locale identifier (LCID) of the current session.
            /// </summary>
            ///
            /// <returns>
            /// A <see cref="T:System.Globalization.CultureInfo"></see> instance that specifies the culture of the current session.
            /// </returns>
            ///
            public int LCID { get; set; }

            /// <summary>
            /// Gets or sets the code-page identifier for the current session.
            /// </summary>
            ///
            /// <returns>
            /// The code-page identifier for the current session.
            /// </returns>
            ///
            public int CodePage { get; set; }

            /// <summary>
            /// Gets a value indicating whether the session is read-only.
            /// </summary>
            ///
            /// <returns>
            /// true if the session is read-only; otherwise, false.
            /// </returns>
            ///
            bool IHttpSessionState.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Gets or sets a session-state item value by name.
            /// </summary>
            ///
            /// <returns>
            /// The session-state item value specified in the name parameter.
            /// </returns>
            ///
            /// <param name="name">The key name of the session-state item value. </param>
            public object this[string name]
            {
                get { return this.BaseGet(name); }
                set { this.BaseSet(name, value); }
            }

            /// <summary>
            /// Gets or sets a session-state item value by numerical index.
            /// </summary>
            ///
            /// <returns>
            /// The session-state item value specified in the index parameter.
            /// </returns>
            ///
            /// <param name="index">The numerical index of the session-state item value. </param>
            public object this[int index]
            {
                get { return this.BaseGet(index); }
                set { this.BaseSet(index, value); }
            }

            /// <summary>
            /// Ends the current session.
            /// </summary>
            ///
            public void Abandon()
            {
                this.BaseClear();
            }

            /// <summary>
            /// Adds a new item to the session-state collection.
            /// </summary>
            ///
            /// <param name="name">The name of the item to add to the session-state collection. </param>
            /// <param name="value">The value of the item to add to the session-state collection. </param>
            public void Add(string name, object value)
            {
                this.BaseAdd(name, value);
            }

            /// <summary>
            /// Deletes an item from the session-state item collection.
            /// </summary>
            ///
            /// <param name="name">The name of the item to delete from the session-state item collection. </param>
            public void Remove(string name)
            {
                this.BaseRemove(name);
            }

            /// <summary>
            /// Deletes an item at a specified index from the session-state item collection.
            /// </summary>
            ///
            /// <param name="index">The index of the item to remove from the session-state collection. </param>
            public void RemoveAt(int index)
            {
                this.BaseRemoveAt(index);
            }

            /// <summary>
            /// Clears all values from the session-state item collection.
            /// </summary>
            ///
            public void Clear()
            {
                this.BaseClear();
            }

            /// <summary>
            /// Clears all values from the session-state item collection.
            /// </summary>
            ///
            public void RemoveAll()
            {
                this.BaseClear();
            }

            /// <summary>
            /// Copies the collection of session-state item values to a one-dimensional array, starting at the specified index in the array.
            /// </summary>
            ///
            /// <param name="array">The <see cref="T:System.Array"></see> that receives the session values. </param>
            /// <param name="index">The index in array where copying starts. </param>
            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }
        }

        internal class ConfigMapPath : IConfigMapPath
        {
            private readonly HttpSimulator _requestSimulation;

            public ConfigMapPath(HttpSimulator simulation)
            {
                this._requestSimulation = simulation;
            }

            public string GetMachineConfigFilename()
            {
                throw new NotImplementedException();
            }

            public string GetRootWebConfigFilename()
            {
                throw new NotImplementedException();
            }

            public void GetPathConfigFilename(string siteId, string path, out string directory, out string baseName)
            {
                throw new NotImplementedException();
            }

            public void GetDefaultSiteNameAndID(out string siteName, out string siteId)
            {
                throw new NotImplementedException();
            }

            public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteId)
            {
                throw new NotImplementedException();
            }

            public string MapPath(string siteId, string path)
            {
                var page = StripPrecedingSlashes(RightAfter(path, this._requestSimulation.ApplicationPath));
                return Path.Combine(this._requestSimulation.PhysicalApplicationPath, page.Replace("/", @"\"));
            }

            public string GetAppPathForPath(string siteId, string path)
            {
                return this._requestSimulation.ApplicationPath;
            }
        }
    }
}
