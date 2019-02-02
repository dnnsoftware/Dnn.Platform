using System.Web;
using System.Web.Mvc;

namespace ClientDependency.Core.Mvc
{
    /// <summary>
    /// Extension methods for ClientDependency
    /// </summary>
    public static class DependencyRendererExtensions
    {

        /// <summary>
        /// Gets the loader from the ViewContext
        /// </summary>
        /// <param name="vc"></param>
        /// <returns></returns>
        public static DependencyRenderer GetLoader(this ViewContext vc)
        {
            bool isNew;
            var instance = DependencyRenderer.TryCreate(vc.HttpContext, out isNew);
            return instance;
        }

        /// <summary>
        /// Gets the loader from the ControllerContext
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static DependencyRenderer GetLoader(this ControllerContext cc)
        {
            bool isNew;
            var instance = DependencyRenderer.TryCreate(cc.HttpContext, out isNew);
            return instance;
        }

        /// <summary>
        /// Gets the loader from the HttpContextBase
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static DependencyRenderer GetLoader(this HttpContextBase http)
        {
            bool isNew;
            var instance = DependencyRenderer.TryCreate(http, out isNew);
            return instance;
        }
    }
}