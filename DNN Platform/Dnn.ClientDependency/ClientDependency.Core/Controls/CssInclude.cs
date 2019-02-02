using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.Controls
{
    /// <summary>
    /// A control used to add a Css file dependency
    /// </summary>
	public class CssInclude : ClientDependencyInclude
	{
	    private CssMediaType _cssMedia;
	    internal bool EncodeImages { get; set; }

		public CssInclude()
		{
			DependencyType = ClientDependencyType.Css;
		    CssMedia = CssMediaType.All;
		}
		public CssInclude(IClientDependencyFile file)
			: base(file)
		{
			DependencyType = ClientDependencyType.Css;
            CssMedia = CssMediaType.All;
		}
        public CssInclude(IClientDependencyFile file, CssMediaType mediaType)
            : base(file)
        {
            DependencyType = ClientDependencyType.Css;
            CssMedia = mediaType;
        }

        public CssMediaType CssMedia
        {
            get { return _cssMedia; }
            set
            {
                if (value != CssMediaType.All)
                {
                    HtmlAttributes.Remove("media");
                    HtmlAttributes.Remove("Media");
                    HtmlAttributes["media"] = value.ToString().ToLowerInvariant();
                }
                _cssMedia = value;
            }
        }
	}
}
