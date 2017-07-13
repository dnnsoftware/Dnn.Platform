using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Host
{
    [ConsoleCommand("clear-cache", "Clears the cache and reloads the page")]
    public class ClearCache : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClearCache));

        public override ConsoleResultModel Run()
        {
            try
            {
                DataCache.ClearCache();
                DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.ClearCache();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to clear the cache.");
            }
            return new ConsoleResultModel("Cache Cleared") { MustReload = true };
        }
    }
}
