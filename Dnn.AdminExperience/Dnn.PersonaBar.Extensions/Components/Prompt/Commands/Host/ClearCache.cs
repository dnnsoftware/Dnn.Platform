// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Host
{
    [ConsoleCommand("clear-cache", Constants.HostCategory, "Prompt_ClearCache_Description")]
    public class ClearCache : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

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
                return new ConsoleErrorResultModel(LocalizeString("Prompt_ClearCache_Error"));
            }
            return new ConsoleResultModel(LocalizeString("Prompt_ClearCache_Success")) { MustReload = true };
        }
    }
}
