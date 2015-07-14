using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.FileRegistration.Providers
{
	public class HtmlEmbedContants
	{
		public const string ScriptEmbedWithSource = "<script src=\"{0}\" {1}></script>";
        public const string CssEmbedWithSource = "<link href=\"{0}\" {1}/>";
        
        public const string ScriptEmbedWithCode = "<script type=\"text/javascript\">{0}</script>";
        //public const string CssEmbed = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
	}
}
