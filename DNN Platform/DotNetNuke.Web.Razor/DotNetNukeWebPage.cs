using System;
using System.Web.WebPages;
using DotNetNuke.Common;
using DotNetNuke.Web.Razor.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.Razor
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public abstract class DotNetNukeWebPage : WebPageBase
    {
        private dynamic _model;

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected internal DnnHelper Dnn { get; internal set; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected internal HtmlHelper Html { get; internal set; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected internal UrlHelper Url { get; internal set; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void ConfigurePage(WebPageBase parentPage)
        {
            base.ConfigurePage(parentPage);

            //Child pages need to get their context from the Parent
            Context = parentPage.Context;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public dynamic Model
        {
            get { return _model ?? (_model = PageContext.Model); }
            set { _model = value; }
        }
    }

    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public abstract class DotNetNukeWebPage<TModel> :DotNetNukeWebPage where TModel : class
    {
        private TModel _model;

        public DotNetNukeWebPage()
        {
            var model = Globals.DependencyProvider.GetService<TModel>();
            Model = model ?? Activator.CreateInstance<TModel>();
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public new TModel Model
        {
            get { return PageContext?.Model as TModel ?? _model; }
            set { _model = value; }
        }
    }
}
