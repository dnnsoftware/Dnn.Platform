using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.Module
{

    /// <summary>
    /// This module currently replaces rogue scripts with composite scripts.
    /// Eventually it will handle css files and MVC implementation
    /// </summary>
    public class ClientDependencyModule : IHttpModule
    {
        #region IHttpModule Members

        void IHttpModule.Dispose() { }

        /// <summary>
        /// Binds the events
        /// </summary>
        /// <param name="app"></param>
        void IHttpModule.Init(HttpApplication app)
        {
            //This event is late enough that the ContentType of the request is set
            //but not too late that we've lost the ability to change the response
            //app.BeginRequest += new EventHandler(HandleRequest);
            app.PostRequestHandlerExecute += HandleRequest;
            LoadFilterTypes();
        }

        /// <summary>
        /// Checks if any assigned filters validate the current handler, if so then assigns any filter
        /// that CanExecute to the response filter chain.
        /// 
        /// Checks if the request MIME type matches the list of mime types specified in the config,
        /// if it does, then it compresses it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var http = new HttpContextWrapper(app.Context);

            var filters = LoadFilters(http);

            if (ValidateCurrentHandler(filters))
            {
                ExecuteFilter(http, filters);
            }

            //if debug is on, then don't compress
            if (!http.IsDebuggingEnabled)
            {
                var c = new MimeTypeCompressor(new HttpContextWrapper(app.Context));
                c.AddCompression();
            }
        }

        #endregion

        private List<Type> m_FilterTypes = new List<Type>();

        #region Private Methods

        private void LoadFilterTypes()
        {
            foreach (var f in ClientDependencySettings.Instance.ConfigSection.Filters.Cast<ProviderSettings>())
            {
                var t = BuildManager.GetType(f.Type, false, true);
                if (t != null)
                {
                    m_FilterTypes.Add(t);
                }
            }
        }

        /// <summary>
        /// loads instances of all registered filters.
        /// </summary>
        /// <param name="http"></param>
        private IEnumerable<IFilter> LoadFilters(HttpContextBase http)
        {
            var loadedFilters = new List<IFilter>();

            foreach (var t in m_FilterTypes)
            {
                var filter = (IFilter)Activator.CreateInstance(t);
                filter.SetHttpContext(http);
                loadedFilters.Add(filter);

            }

            return loadedFilters;
        }

        /// <summary>
        /// Ensure the current running handler is valid in order to proceed with the module filter.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        private static bool ValidateCurrentHandler(IEnumerable<IFilter> filters)
        {
            return filters.Any(f => f.ValidateCurrentHandler());
        }

        private void ExecuteFilter(HttpContextBase http, IEnumerable<IFilter> filters)
        {
            var filter = new ResponseFilterStream(http.Response.Filter, http);
            foreach (var f in filters.Where(f => f.CanExecute()))
            {
                filter.TransformString += f.UpdateOutputHtml;
            }
            http.Response.Filter = filter;
        }

        #endregion

    }
}
