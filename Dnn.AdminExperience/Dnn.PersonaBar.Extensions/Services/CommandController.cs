// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Prompt.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;

    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Common;
    using Dnn.PersonaBar.Prompt.Components;
    using Dnn.PersonaBar.Prompt.Components.Models;

    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    using IConsoleCommand = DotNetNuke.Abstractions.Prompt.IConsoleCommand;
#pragma warning disable CS0618
    using IOldConsoleCommand = Dnn.PersonaBar.Library.Prompt.IConsoleCommand;
#pragma warning restore CS0618

    [MenuPermission(MenuName = "Dnn.Prompt")]
    [RequireHost]
    public class CommandController : ControllerBase, IServiceRouteMapper
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CommandController));
        private static readonly string[] DenyList = ["smtppassword", "password", "pwd", "pass", "apikey",];
        private static readonly string[] Namespaces = ["Dnn.PersonaBar.Prompt.Services",];
        private static readonly char[] Separators = [',', '|', ' ',];

        private readonly IServiceProvider serviceProvider;
        private readonly ICommandRepository commandRepository;
        private readonly Components.Repositories.ICommandRepository oldCommandRepository;
        private int portalId = -1;
        private PortalSettings portalSettings;

        /// <summary>Initializes a new instance of the <see cref="CommandController"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="commandRepository">The command repository.</param>
        /// <param name="oldCommandRepository">The obsolete command repository (to be removed in DNN 11).</param>
        public CommandController(IServiceProvider serviceProvider, ICommandRepository commandRepository, Components.Repositories.ICommandRepository oldCommandRepository)
        {
            this.serviceProvider = serviceProvider;
            this.commandRepository = commandRepository;
            this.oldCommandRepository = oldCommandRepository;
        }

        private new int PortalId
        {
            get
            {
                if (this.portalId == -1)
                {
                    this.portalId = base.PortalId;
                }

                return this.portalId;
            }

            set
            {
                this.portalId = value;
            }
        }

        private new PortalSettings PortalSettings
        {
            get
            {
                if (this.portalSettings == null)
                {
                    this.portalSettings = base.PortalSettings;
                }

                return this.portalSettings;
            }

            set
            {
                this.portalSettings = value;
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Cmd(int portalId, [FromBody] CommandInputModel command)
        {
            var portal = PortalController.Instance.GetPortal(portalId);

            if (portal == null)
            {
                var errorMessage = string.Format(Localization.GetString("Prompt_GetPortal_NotFound", Constants.LocalResourcesFile), portalId);
                Logger.Error(errorMessage);
                return this.AddLogAndReturnResponse(null, null, command, DateTime.Now, errorMessage);
            }

            this.PortalId = portalId;
            this.SetupPortalSettings(portalId);

            return this.Cmd(command);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Cmd([FromBody] CommandInputModel command)
        {
            var startTime = DateTime.Now;
            try
            {
                var args = command.Args;
                var isHelpCmd = args.First().Equals("HELP", StringComparison.CurrentCultureIgnoreCase);
                var isHelpLearn = isHelpCmd && args.Length > 1 && args[1].Equals("LEARN", StringComparison.CurrentCultureIgnoreCase);
                var isHelpSyntax = isHelpCmd && args.Length > 1 && args[1].Equals("SYNTAX", StringComparison.CurrentCultureIgnoreCase);
                var cmdName = isHelpCmd ? (args.Length > 1 ? args[1].ToUpper() : string.Empty) : args.First().ToUpper();
                if (isHelpSyntax)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new CommandHelp()
                    {
                        ResultHtml = Localization.GetString("Prompt_CommandHelpSyntax", Constants.LocalResourcesFile),
                    });
                }
                else if (isHelpLearn)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new CommandHelp()
                    {
                        ResultHtml = Localization.GetString("Prompt_CommandHelpLearn", Constants.LocalResourcesFile),
                    });
                }
                else if (isHelpCmd && args.Length == 1)
                {
                    return this.AddLogAndReturnResponse(
                        null,
                        null,
                        command,
                        startTime,
                        string.Format(
                            Localization.GetString("CommandNotFound", Constants.LocalResourcesFile),
                            cmdName.ToLower()));
                }

                // first look in new commands, then in the old commands
                var newCommand = this.commandRepository.GetCommand(this.serviceProvider, cmdName);
                if (newCommand == null)
                {
                    var allCommands = this.oldCommandRepository.GetCommands();

                    // if no command found notify
                    if (!allCommands.TryGetValue(cmdName, out var oldCommand))
                    {
                        var sbError = new StringBuilder();
                        var suggestion = Utilities.GetSuggestedCommand(cmdName);
                        sbError.AppendFormat(Localization.GetString("CommandNotFound", Constants.LocalResourcesFile), cmdName.ToLower());
                        if (!string.IsNullOrEmpty(suggestion))
                        {
                            sbError.AppendFormat(Localization.GetString("DidYouMean", Constants.LocalResourcesFile), suggestion);
                        }

                        return this.AddLogAndReturnResponse(null, null, command, startTime, sbError.ToString());
                    }

                    return this.TryRunOldCommand(command, oldCommand.CommandType, args, isHelpCmd, startTime);
                }
                else
                {
                    return this.TryRunNewCommand(command, newCommand, args, isHelpCmd, startTime);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.AddLogAndReturnResponse(null, null, command, startTime, ex.Message);
            }
        }

        /// <inheritdoc/>
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("PersonaBar", "promptwithportalid", "{controller}/{action}/{portalId}", null, new { portalId = "-?\\d+" }, Namespaces);
        }

        private static string FilterCommand(string command)
        {
            var blackList = DenyList;
            var promptDenyList = HostController.Instance.GetString("PromptBlackList", string.Empty)
                .Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            if (promptDenyList.Length > 0)
            {
                blackList = blackList.Concat(promptDenyList).Distinct().ToArray();
            }

            var args = command.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLowerInvariant()).ToList();
            foreach (var lowerKey in blackList.Select(key => key.ToLowerInvariant())
                        .Where(lowerKey => args.Any(arg => arg.Replace("-", string.Empty) == lowerKey)))
            {
                args[args.TakeWhile(arg => arg.Replace("-", string.Empty) != lowerKey).Count() + 1] = "******";
            }

            return string.Join(" ", args);
        }

        private void SetupPortalSettings(int portalId)
        {
            this.PortalSettings = new PortalSettings(portalId);
            var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
            this.PortalSettings.PrimaryAlias = portalAliases.FirstOrDefault(a => a.IsPrimary);
            this.PortalSettings.PortalAlias = PortalAliasController.Instance.GetPortalAlias(this.PortalSettings.DefaultPortalAlias);
        }

        private HttpResponseMessage TryRunOldCommand(CommandInputModel command, Type cmdTypeToRun, string[] args, bool isHelpCmd, DateTime startTime)
        {
            // Instantiate and run the command
            try
            {
                var cmdObj = (IOldConsoleCommand)ActivatorUtilities.GetServiceOrCreateInstance(this.serviceProvider, cmdTypeToRun);
                if (isHelpCmd)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, this.oldCommandRepository.GetCommandHelp(command.Args, cmdObj));
                }

                // set env. data for command use
                cmdObj.Initialize(args, this.PortalSettings, this.UserInfo, command.CurrentPage);
                return this.AddLogAndReturnResponse(cmdObj, cmdTypeToRun, command, startTime);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.AddLogAndReturnResponse(null, null, command, startTime, ex.Message);
            }
        }

        private HttpResponseMessage TryRunNewCommand(CommandInputModel command, IConsoleCommand cmdObj, string[] args, bool isHelpCmd, DateTime startTime)
        {
            // Instantiate and run the command that uses the new interfaces and base class
            try
            {
                if (isHelpCmd)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, this.commandRepository.GetCommandHelp(cmdObj));
                }

                // set env. data for command use
                cmdObj.Initialize(args, this.PortalSettings, this.UserInfo, command.CurrentPage);
                return this.AddLogAndReturnResponseNewCommands(cmdObj, command, startTime);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.AddLogAndReturnResponse(null, null, command, startTime, ex.Message);
            }
        }

        private HttpResponseMessage AddLogAndReturnResponseNewCommands(IConsoleCommand consoleCommand, CommandInputModel command, DateTime startTime, string error = null)
        {
            HttpResponseMessage message;
            var isValid = consoleCommand?.IsValid() ?? false;
            var logInfo = new LogInfo
            {
                LogTypeKey = "PROMPT_ALERT",
            };
            logInfo.LogProperties.Add(new LogDetailInfo("Command", FilterCommand(command.CmdLine)));
            logInfo.LogProperties.Add(new LogDetailInfo("IsValid", isValid.ToString()));

            try
            {
                logInfo.LogProperties.Add(new LogDetailInfo("TypeFullName", consoleCommand.GetType().FullName));
                if (isValid)
                {
                    var result = consoleCommand.Run();
                    if (result.PagingInfo != null)
                    {
                        if (result.PagingInfo.PageNo < result.PagingInfo.TotalPages)
                        {
                            result.Output = string.Format(
                                Localization.GetString("Prompt_PagingMessageWithLoad", Constants.LocalResourcesFile),
                                result.PagingInfo.PageNo,
                                result.PagingInfo.TotalPages);

                            var args = command.Args;
                            var indexOfPage = args.Any(x => x.Equals("--page", StringComparison.InvariantCultureIgnoreCase))
                                ? args.TakeWhile(arg => !arg.Equals("--page", StringComparison.InvariantCultureIgnoreCase)).Count()
                                : -1;
                            if (indexOfPage > -1)
                            {
                                args[indexOfPage + 1] = (result.PagingInfo.PageNo + 1).ToString();
                            }

                            var nextPageCommand = string.Join(" ", args);
                            if (indexOfPage == -1)
                            {
                                nextPageCommand += " --page " + (result.PagingInfo.PageNo + 1);
                            }

                            result.NextPageCommand = nextPageCommand;
                        }
                        else if (result.Records > 0)
                        {
                            result.Output = string.Format(
                                Localization.GetString("Prompt_PagingMessage", Constants.LocalResourcesFile),
                                result.PagingInfo.PageNo,
                                result.PagingInfo.TotalPages);
                        }
                    }

                    message = this.Request.CreateResponse(HttpStatusCode.OK, result);
                    logInfo.LogProperties.Add(new LogDetailInfo("RecordsAffected", result.Records.ToString()));
                    logInfo.LogProperties.Add(new LogDetailInfo("Output", result.Output));
                }
                else
                {
                    logInfo.LogProperties.Add(new LogDetailInfo("Output", consoleCommand?.ValidationMessage ?? error));
                    message = this.BadRequestResponse(consoleCommand?.ValidationMessage ?? error);
                }
            }
            catch (Exception ex)
            {
                logInfo.Exception = new ExceptionInfo(ex);
                message = this.BadRequestResponse(ex.Message);
            }

            logInfo.LogProperties.Add(new LogDetailInfo("ExecutionTime(hh:mm:ss)", TimeSpan.FromMilliseconds(DateTime.Now.Subtract(startTime).TotalMilliseconds).ToString(@"hh\:mm\:ss\.ffffff")));
            LogController.Instance.AddLog(logInfo);
            return message;
        }

        /// <summary>Log every command run by a users.</summary>
        /// <param name="consoleCommand">The old command, or <see langword="null"/>.</param>
        /// <param name="cmdTypeToRun">The type of command to run.</param>
        /// <param name="command">The command input.</param>
        /// <param name="startTime">The start time of the command run.</param>
        /// <param name="error">An error message.</param>
        /// <returns>A <see cref="ConsoleResultModel"/> response.</returns>
        private HttpResponseMessage AddLogAndReturnResponse(IOldConsoleCommand consoleCommand, Type cmdTypeToRun, CommandInputModel command, DateTime startTime, string error = null)
        {
            HttpResponseMessage message;
            var isValid = consoleCommand?.IsValid() ?? false;
            var logInfo = new LogInfo
            {
                LogTypeKey = "PROMPT_ALERT",
            };
            logInfo.LogProperties.Add(new LogDetailInfo("Command", FilterCommand(command.CmdLine)));
            logInfo.LogProperties.Add(new LogDetailInfo("IsValid", isValid.ToString()));

            try
            {
                if (cmdTypeToRun != null)
                {
                    logInfo.LogProperties.Add(new LogDetailInfo("TypeFullName", cmdTypeToRun.FullName));
                }

                if (isValid)
                {
                    var result = consoleCommand.Run();
                    if (result.PagingInfo != null)
                    {
                        if (result.PagingInfo.PageNo < result.PagingInfo.TotalPages)
                        {
                            result.Output = string.Format(
                                Localization.GetString("Prompt_PagingMessageWithLoad", Constants.LocalResourcesFile),
                                result.PagingInfo.PageNo,
                                result.PagingInfo.TotalPages);

                            var args = command.Args;
                            var indexOfPage = args.Any(x => x.Equals("--page", StringComparison.InvariantCultureIgnoreCase))
                                ? args.TakeWhile(arg => !arg.Equals("--page", StringComparison.InvariantCultureIgnoreCase)).Count()
                                : -1;
                            if (indexOfPage > -1)
                            {
                                args[indexOfPage + 1] = (result.PagingInfo.PageNo + 1).ToString();
                            }

                            var nextPageCommand = string.Join(" ", args);
                            if (indexOfPage == -1)
                            {
                                nextPageCommand += " --page " + (result.PagingInfo.PageNo + 1);
                            }

                            result.NextPageCommand = nextPageCommand;
                        }
                        else if (result.Records > 0)
                        {
                            result.Output = string.Format(
                                Localization.GetString("Prompt_PagingMessage", Constants.LocalResourcesFile),
                                result.PagingInfo.PageNo,
                                result.PagingInfo.TotalPages);
                        }
                    }

                    message = this.Request.CreateResponse(HttpStatusCode.OK, result);
                    logInfo.LogProperties.Add(new LogDetailInfo("RecordsAffected", result.Records.ToString()));
                    logInfo.LogProperties.Add(new LogDetailInfo("Output", result.Output));
                }
                else
                {
                    logInfo.LogProperties.Add(new LogDetailInfo("Output", consoleCommand?.ValidationMessage ?? error));
                    message = this.BadRequestResponse(consoleCommand?.ValidationMessage ?? error);
                }
            }
            catch (Exception ex)
            {
                logInfo.Exception = new ExceptionInfo(ex);
                message = this.BadRequestResponse(ex.Message);
            }

            logInfo.LogProperties.Add(new LogDetailInfo("ExecutionTime(hh:mm:ss)", TimeSpan.FromMilliseconds(DateTime.Now.Subtract(startTime).TotalMilliseconds).ToString(@"hh\:mm\:ss\.ffffff")));
            LogController.Instance.AddLog(logInfo);
            return message;
        }
    }
}
