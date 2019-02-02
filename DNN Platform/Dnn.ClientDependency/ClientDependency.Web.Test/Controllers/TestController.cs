using System;
using System.Web.Mvc;
using ClientDependency.Core.Mvc;
using ClientDependency.Web.Test.Models;

namespace ClientDependency.Web.Test.Controllers
{

    
    public class TestController : Controller
    {
        /// <summary>
        /// Try to render any view that matches the action name
        /// </summary>
        /// <param name="actionName"></param>
        protected override void HandleUnknownAction(string actionName)
        {
            try
            {
                this.View(actionName).ExecuteResult(this.ControllerContext);
            }
            catch (InvalidOperationException ieox)
            {
                throw;
                //ViewData["error"] = "Unknown Action: \"" +
                //    Server.HtmlEncode(actionName) + "\"";
                //ViewData["exMessage"] = ieox.Message;
                //this.View("Error").ExecuteResult(this.ControllerContext);
            }
        }                

        /// <summary>
        /// Custom action to dynamically register a dependency in an action
        /// </summary>
        /// <returns></returns>
        public ActionResult DynamicPathRegistration()
        {            
            ControllerContext.GetLoader().AddPath("NewJsPath", "~/Js/TestPath");

            return View(new TestModel());
        }
        
    }
}
