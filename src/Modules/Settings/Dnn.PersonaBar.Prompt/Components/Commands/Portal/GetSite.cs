using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    /// <summary>
    /// alias for get-portal
    /// </summary>
    [ConsoleCommand("get-site", "Prompt_GetSite_Description", new[] { "id" })]
    public class GetSite : GetPortal
    {
    }
}