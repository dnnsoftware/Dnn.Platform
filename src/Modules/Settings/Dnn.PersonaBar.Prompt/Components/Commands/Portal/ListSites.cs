using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    /// <summary>
    /// Alias for list-portals
    /// </summary>
    [ConsoleCommand("list-sites", Constants.PortalCategory, "Prompt_ListSites_Description")]
    public class ListSites : ListPortals
    {
    }
}