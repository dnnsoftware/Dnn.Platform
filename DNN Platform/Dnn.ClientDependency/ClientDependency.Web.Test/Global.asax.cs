using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using ClientDependency.Core;
using ClientDependency.Core.Config;
using ClientDependency.Core.Mvc;

namespace ClientDependency.Web.Test
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*allaxd}", new { allaxd = @".*\.axd(/.*)?" });
            routes.IgnoreRoute("{*allaspx}", new { allaspx = @".*\.aspx(/.*)?" });
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" }); 

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}", // URL with parameters
                new { controller = "Test" } // Parameter defaults
            );
        }

        public static void CreateBundles()
        {
            BundleManager.CreateCssBundle("CssBundle", 
                new CssFile("~/Css/BundleTest/css1.css"),
                new CssFile("~/Css/BundleTest/css2.css"),
                new CssFile("~/Css/BundleTest/css3.css"));
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.ReplaceDefaultRazorEngine(new CdfRazorViewEngine());

            //remove MVC filter (to test the view engines) (if you want to test)
            //ClientDependencySettings.Instance.ConfigSection.Filters.Remove("MvcFilter");

            CreateBundles();
        }
    }

}