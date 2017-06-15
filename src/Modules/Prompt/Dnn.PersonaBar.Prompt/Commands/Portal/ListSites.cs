using Dnn.PersonaBar.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Commands.Portal
{
    /// <summary>
    /// Alias for list-portals
    /// </summary>
    [ConsoleCommand("list-sites", "Retrieves a list of sites for the current DNN Installation", new string[] { })]
    public class ListSites : ListPortals
    {
    }
}