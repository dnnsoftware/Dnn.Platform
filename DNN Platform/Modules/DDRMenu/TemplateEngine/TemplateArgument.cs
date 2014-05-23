namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    public class TemplateArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public TemplateArgument()
        {
        }

        public TemplateArgument(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}