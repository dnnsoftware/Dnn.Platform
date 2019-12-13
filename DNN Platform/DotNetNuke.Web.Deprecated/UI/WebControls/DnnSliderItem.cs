#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnSliderItem : RadSliderItem
    {
        public DnnSliderItem()
        {
        }

        public DnnSliderItem(string text) : base(text)
        {
        }

        public DnnSliderItem(string text, string value) : base(text, value)
        {
        }
    }
}
