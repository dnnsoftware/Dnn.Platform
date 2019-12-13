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
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class RazorHostControl : ModuleControlBase, IActionable
    {
        private readonly string _razorScriptFile;

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public RazorHostControl(string scriptFile)
        {
            _razorScriptFile = scriptFile;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
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

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
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

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public ModuleActionCollection ModuleActions
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
