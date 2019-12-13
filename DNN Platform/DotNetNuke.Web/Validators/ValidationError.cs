namespace DotNetNuke.Web.Validators
{
    public class ValidationError
    {
        #region "Public Properties"

        public string ErrorMessage { get; set; }

        public string PropertyName { get; set; }

        public object Validator { get; set; }

        #endregion
    }
}
