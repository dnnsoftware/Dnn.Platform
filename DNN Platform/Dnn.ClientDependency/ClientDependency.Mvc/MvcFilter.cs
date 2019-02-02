using System.Web;
using System.Web.Mvc;
using ClientDependency.Core.Module;

namespace ClientDependency.Core.Mvc
{
    /// <summary>
    /// MvcFilter is required when using ClientDependency in MVC, without it ClientDependency will not work with MVC unless you are
    /// using razor and using the Cdf view engines.
    /// </summary>
    public class MvcFilter : IFilter
    {
        private readonly MvcRogueFileFilter _rogueFileFilter = new MvcRogueFileFilter();

        #region IFilter Members

        /// <summary>
        /// Sets the http context
        /// </summary>
        /// <param name="ctx"></param>
        public void SetHttpContext(HttpContextBase ctx)
        {
            CurrentContext = ctx;

            //set the context for the internal rogue filter
            _rogueFileFilter.SetHttpContext(ctx);
        }

        public virtual bool CanExecute()
        {
            return CurrentContext.CurrentHandler is MvcHandler && IsCompressibleContentType(CurrentContext.Response);
        }

        public virtual bool ValidateCurrentHandler()
        {
            return (CurrentContext.CurrentHandler is MvcHandler);
        }

        /// <summary>
        /// Updates the html js/css templates rendered temporarily by the controls into real js/css html tags.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will also validate whether the rogue script handler should run and if so does.
        /// </remarks>
        public string UpdateOutputHtml(string html)
        {
            //first we need to check if this is MVC!
            if (CurrentContext.CurrentHandler is MvcHandler)
            {
                //parse the html output with the renderer
                var r = DependencyRenderer.GetInstance(CurrentContext);
                if (r != null)
                {
                    var output  = r.ParseHtmlPlaceholders(html);

                    //get the rogue filter going
                    if (_rogueFileFilter.CanExecute())
                        output = _rogueFileFilter.UpdateOutputHtml(output);

                    return output;
                }                
            }
            return html;            
        }

        public HttpContextBase CurrentContext { get; private set; }
      

        #endregion

        protected virtual bool IsCompressibleContentType(HttpResponseBase response)
        {
            //TODO: Is there a better way to check the ContentType is something we want to compress?
            switch (response.ContentType.ToLower())
            {
                case "text/html":
                case "text/css":
                case "text/plain":
                case "application/x-javascript":
                case "text/javascript":
                case "text/xml":
                case "application/xml":
                case "":
                    return true;

                default:
                    return false;
            }
        }
    }

}
