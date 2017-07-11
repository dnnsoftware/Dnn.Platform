using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Commands.Portal
{
    /// <summary>
    /// alias for get-portal
    /// </summary>
    [ConsoleCommand("get-site", "Retrieves information about the current site", new string[] { "id" })]
    public class GetSite : GetPortal
    {
    }
}