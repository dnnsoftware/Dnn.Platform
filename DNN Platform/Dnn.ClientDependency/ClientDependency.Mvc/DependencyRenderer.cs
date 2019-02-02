using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using ClientDependency.Core.Config;
using ClientDependency.Core.FileRegistration.Providers;
using System.Text.RegularExpressions;

namespace ClientDependency.Core.Mvc
{
    /// <summary>
    /// This is the class that controls rendering dependencies for MVC
    /// </summary>
    public class DependencyRenderer : BaseLoader
    {
        /// <summary>
        /// Constructor based on MvcHandler 
        /// </summary>
        /// <param name="ctx"></param>
        private DependencyRenderer(HttpContextBase ctx)
            : base(ctx)
        {
            //by default the provider is the default provider 
            Provider = ClientDependencySettings.Instance.DefaultMvcRenderer;
            if (ctx.Items.Contains(ContextKey))
                throw new InvalidOperationException("Only one ClientDependencyLoader may exist in a context");
            ctx.Items[ContextKey] = this;
        }


        #region Constants
        public const string ContextKey = "MvcLoader";
        private const string JsMarkupRegex = "<!--\\[Javascript:Name=\"(?<renderer>.*?)\"\\]//-->";
        private const string CssMarkupRegex = "<!--\\[Css:Name=\"(?<renderer>.*?)\"\\]//-->";
        #endregion

        #region Static methods

        /// <summary>
        /// used for locking
        /// </summary>
        private static readonly object Locker = new object();

        /// <summary>
        /// Singleton per request instance.
        /// </summary>
        /// <exception cref="NullReferenceException">
        /// If no MvcDependencyLoader control exists on the context, an exception is thrown.
        /// </exception>
        public static DependencyRenderer GetInstance(HttpContextBase ctx)
        {
            if (!ctx.Items.Contains(ContextKey))
                return null;
            return ctx.Items[ContextKey] as DependencyRenderer;
        }

        /// <summary>
        /// Checks if a loader already exists, if it does, it returns it, otherwise it will
        /// create a new one in the control specified.
        /// isNew will be true if a loader was created, otherwise false if it already existed.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        internal static DependencyRenderer TryCreate(HttpContextBase ctx, out bool isNew)
        {
            if (GetInstance(ctx) == null)
            {
                lock (Locker)
                {
                    //double check
                    if (GetInstance(ctx) == null)
                    {
                        var loader = new DependencyRenderer(ctx);
                        isNew = true;
                        return loader;
                    }
                }

            }

            isNew = false;
            return GetInstance(ctx);

        }

        #endregion
        
        #region Internal Methods
        
        /// <summary>
        /// This replaces the HTML placeholders that we're rendered into the html
        /// markup before the module calls this method to update the placeholders with 
        /// the real dependencies.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string ParseHtmlPlaceholders(string html)
        {
            GenerateOutput();

            return PlaceholderParser.ParseHtmlPlaceholders(CurrentContext, html, JsMarkupRegex, CssMarkupRegex, _output.ToArray());
        }

        /// <summary>
        /// Renders the HTML markup placeholder with the default provider name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        internal string RenderPlaceholder(ClientDependencyType type, IEnumerable<IClientDependencyPath> paths)
        {
            return RenderPlaceholder(type, Provider.Name, paths);
        }

        /// <summary>
        /// Renders the HTML markup placeholder with the provider specified by rendererName
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rendererName"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        internal string RenderPlaceholder(ClientDependencyType type, string rendererName, IEnumerable<IClientDependencyPath> paths)
        {
            Paths.UnionWith(paths);

            return string.Format("<!--[{0}:Name=\"{1}\"]//-->"
                , type
                , rendererName);
        }

        #endregion


        private readonly List<RendererOutput> _output = new List<RendererOutput>();

        /// <summary>
        /// Loop through each object and
        /// get the output for both js and css from each provider in the list
        /// based on each list items dependencies.
        /// </summary>
        /// <remarks>
        /// For some reason ampersands that aren't html escaped are not compliant to HTML standards when they exist in 'link' or 'script' tags in URLs,
        /// we need to replace the ampersands with &amp; . This is only required for this one w3c compliancy, the URL itself is a valid URL.
        /// </remarks>
        private void GenerateOutput()
        {
            foreach (var x in Dependencies)
            {
                var renderer = ((BaseRenderer)x.Provider);
                string js, css;
                renderer.RegisterDependencies(x.Dependencies, Paths, out js, out css, CurrentContext);

                //store the output in a new output object
                _output.Add(new RendererOutput()
                {
                    Name = x.Provider.Name,
                    OutputCss = css,
                    OutputJs = js
                });
            }
        }

        


    }
}
