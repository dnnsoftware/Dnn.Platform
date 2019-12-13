namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFieldLabel : DnnLabel
    {
        public override void LocalizeStrings()
        {
            base.LocalizeStrings();
            Text = Text + Utilities.GetLocalizedString("FieldSuffix.Text");
        }
    }
}
