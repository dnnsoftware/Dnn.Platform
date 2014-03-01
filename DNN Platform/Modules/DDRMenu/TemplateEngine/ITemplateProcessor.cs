using System.Web.UI;

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
	internal interface ITemplateProcessor
	{
		bool LoadDefinition(TemplateDefinition baseDefinition);
		void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition);
	}
}