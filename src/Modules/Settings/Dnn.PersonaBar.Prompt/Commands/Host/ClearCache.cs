using Dnn.PersonaBar.Library.Prompt.Attributes;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Host
{
    [ConsoleCommand("clear-cache", "Clears the cache and reloads the page", new string[] {})]
    public class ClearCache : ConsoleCommandBase, IConsoleCommand
{

    public string ValidationMessage { get; }

    public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
    {
        Initialize(args, portalSettings, userInfo, activeTabId);
    }

    public bool IsValid()
    {
        return true;
    }

    public ConsoleResultModel Run()
    {

        try
        {
            DataCache.ClearCache();
            DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.ClearCache();
        }
        catch (Exception ex)
        {
            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            return new ConsoleErrorResultModel("An error occurred while attempting to clear the cache.");
        }
        return new ConsoleResultModel("Cache Cleared") { MustReload = true };

    }


}

}
