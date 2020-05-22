namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommandOption
    {
        string DefaultValue { get; set; }
        string DescriptionKey { get; set; }
        string Name { get; set; }
        bool Required { get; set; }
    }
}
