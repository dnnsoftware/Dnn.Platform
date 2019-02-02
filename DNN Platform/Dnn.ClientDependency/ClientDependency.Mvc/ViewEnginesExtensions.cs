using System.Linq;
using System.Web.Mvc;

namespace ClientDependency.Core.Mvc
{
    public static class ViewEnginesExtensions
    {
        /// <summary>
        /// Replaces the default razor view engine with the specified one
        /// </summary>
        /// <param name="engines"></param>
        /// <param name="replacement"></param>
        public static void ReplaceDefaultRazorEngine(this ViewEngineCollection engines, IViewEngine replacement)
        {
            engines.ReplaceEngine<RazorViewEngine>(replacement);         
        }

        /// <summary>
        /// Replaces the engine matching 'T' with the specified one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="engines"></param>
        /// <param name="replacement"></param>
        public static void ReplaceEngine<T>(this ViewEngineCollection engines, IViewEngine replacement)
            where T : IViewEngine
        {
            var engine = engines.SingleOrDefault(x => x.GetType() == typeof(T));
            if (engine != null)
            {
                engines.Remove(engine);
            }
            engines.Add(replacement);
        }
    }
}