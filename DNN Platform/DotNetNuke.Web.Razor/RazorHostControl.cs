#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
#region Usings

using System;
using System.IO;
using System.Web;
using System.Web.UI;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Web.Razor
{
    public class RazorHostControl : ModuleControlBase, IActionable
    {
        private readonly string _razorScriptFile;

        public RazorHostControl(string scriptFile)
        {
            _razorScriptFile = scriptFile;
        }

        protected virtual string RazorScriptFile
        {
            get { return _razorScriptFile; }
        }

	    private RazorEngine _engine;
	    private  RazorEngine Engine
	    {
		    get
		    {
				if (_engine == null)
			    {
					_engine = new RazorEngine(RazorScriptFile, ModuleContext, LocalResourceFile);
			    }

			    return _engine;
		    }
	    }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!(string.IsNullOrEmpty(RazorScriptFile)))
            {
                var writer = new StringWriter();
				Engine.Render(writer);
                Controls.Add(new LiteralControl(HttpUtility.HtmlDecode(writer.ToString())));
            }
        }

		public Entities.Modules.Actions.ModuleActionCollection ModuleActions
		{
			get
			{
				if (Engine.Webpage is IActionable)
				{
					return (Engine.Webpage as IActionable).ModuleActions;
				}

				return new ModuleActionCollection();
			}
		}
	}
}