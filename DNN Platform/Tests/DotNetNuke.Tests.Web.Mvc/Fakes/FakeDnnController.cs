#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System.Web.Routing;

namespace DotNetNuke.Tests.Web.Mvc.Fakes
{
    public class FakeDnnController : DnnController
    {
        public ActionResult Action1()
        {
            return View("Action1");
        }

        public ActionResult Action2()
        {
            return View("Action2", "Master2");
        }

        public ActionResult Action3(Dog dog)
        {
            return View("Action3", "Master3", dog);
        }

        public void MockInitialize(RequestContext requestContext)
        {
            // Mocking out the entire MvcHandler and Controller lifecycle proved to be difficult
            // This method executes the initialization logic that occurs on every request which is
            // executed from the Execute method.
            Initialize(requestContext);
        }    
    }
}
