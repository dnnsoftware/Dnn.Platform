using System.IO;

namespace log4net.ObjectRenderer
{
	public interface IObjectRenderer
	{
		void RenderObject(RendererMap rendererMap, object obj, TextWriter writer);
	}
}