namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFieldLiteral : DnnLiteral
    {
        public override void LocalizeStrings()
        {
            base.LocalizeStrings();
            Text = Text + Utilities.GetLocalizedString("FieldSuffix.Text");
        }
    }
}
