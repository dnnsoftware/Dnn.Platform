#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnToolBarButton : RadToolBarButton
    {
        public DnnToolBarButton()
        {
        }

        public DnnToolBarButton(string text) : base(text)
        {
        }

        public DnnToolBarButton(string text, bool isChecked, string @group) : base(text, isChecked, @group)
        {
        }
    }
}
