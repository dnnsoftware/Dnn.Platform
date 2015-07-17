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
        public static event EventHandler<ApplyingResponseFilterEventArgs> ApplyingResponseFilter;

        private void OnApplyingResponseFilter(ApplyingResponseFilterEventArgs e)
        {
            var handler = ApplyingResponseFilter;
            if (handler != null) handler(this, e);
        }

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

            //if debug is on, then don't compress
            if (!http.IsDebuggingEnabled)
            {
                //IMPORTANT: Compression must be assigned before any other filters are executed!
                // if compression is applied after the response has been modified then we will end
                // up getting encoding errors.
                // The compressor will not attempt to compress if the current filter is not ASP.Net's 
                // original filter. The filter could be changed by developers or perhaps even hosting
                // providers (based on their machine.config with their own modules.
                var c = new MimeTypeCompressor(new HttpContextWrapper(app.Context));
                c.AddCompression();
            }

            var filters = LoadFilters(http);

            if (ValidateCurrentHandler(filters))
            {
                ExecuteFilter(http, filters);
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
            //raise event, deverlopers can cancel the filter from being applied depending on what is on the http context.
            var args = new ApplyingResponseFilterEventArgs(http);
            OnApplyingResponseFilter(args);

            if (!args.Cancel)
            {
                var filter = new ResponseFilterStream(http.Response.Filter, http);
                foreach (var f in filters.Where(f => f.CanExecute()))
                {
                    filter.TransformString += f.UpdateOutputHtml;
                }
                http.Response.Filter = filter;    
            }
            
        }

        #endregion

    }
}
