using System;

using WebFormsMvp;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class HttpHandlerPresenter<TView> : Presenter<TView> where TView : class, IHttpHandlerView
    {
        protected HttpHandlerPresenter(TView view) : base(view)
        {
        }
    }
}
