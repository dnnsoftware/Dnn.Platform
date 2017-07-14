using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    /// <summary>
    /// Alias for list-portals
    /// </summary>
    [ConsoleCommand("list-sites", "Retrieves a list of sites for the current DNN Installation")]
    public class ListSites : ListPortals
    {
    }
}