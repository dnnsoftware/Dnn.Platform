namespace DotNetNuke.Entities.Portals.Templates
{
    public interface IPortalTemplateInfo
    {
        string ResourceFilePath { get; }
        string Name { get; }
        string CultureCode { get; }
        string TemplateFilePath { get; }
        string LanguageFilePath { get; }
        string Description { get; }
    }
}
