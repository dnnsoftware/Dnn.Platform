namespace DotNetNuke.Web.Validators
{
    public abstract class ObjectValidator
    {
        public abstract ValidationResult ValidateObject(object target);
    }
}
