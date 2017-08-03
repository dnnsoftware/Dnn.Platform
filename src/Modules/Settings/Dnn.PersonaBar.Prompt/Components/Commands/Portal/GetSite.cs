using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    /// <summary>
    /// alias for get-portal
    /// </summary>
    [ConsoleCommand("get-site", Constants.PortalCategory, "Prompt_GetSite_Description")]
    public class GetSite : GetPortal
    {
    }
}