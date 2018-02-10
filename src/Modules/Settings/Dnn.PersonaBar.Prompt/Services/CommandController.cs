using DotNetNuke.Web.Api;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Components;
using Dnn.PersonaBar.Prompt.Components.Models;
using Dnn.PersonaBar.Prompt.Components.Repositories;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Prompt.Services
{
    [MenuPermission(MenuName = "Dnn.Prompt")]
    [RequireHost]
    public class CommandController : ControllerBase, IServiceRouteMapper
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CommandController));
        private static readonly string[] BlackList = { "smtppassword", "password", "pwd", "pass", "apikey" };

        private int _portalId = -1;
        private new int PortalId
        {
            get
            {
                if (_portalId == -1)
                {
                    _portalId = base.PortalId;
                }
                return _portalId;
            }
            set
            {
                _portalId = value;
            }
        }
        private PortalSettings _portalSettings;
        private new PortalSettings PortalSettings
        {
            get
            {
                if (_portalSettings == null)
                {
                    _portalSettings = base.PortalSettings;
                }
                return _portalSettings;
            }
            set
            {
                _portalSettings = value;
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
                return AddLogAndReturnResponse(null, null, command, DateTime.Now, errorMessage);
            }

            PortalId = portalId;
            SetupPortalSettings(portalId);

            return Cmd(command);
        }

        private void SetupPortalSettings(int portalId)
        {
            PortalSettings = new PortalSettings(portalId);
            var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
            PortalSettings.PrimaryAlias = portalAliases.FirstOrDefault(a => a.IsPrimary);
            PortalSettings.PortalAlias = PortalAliasController.Instance.GetPortalAlias(PortalSettings.DefaultPortalAlias);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Cmd([FromBody] CommandInputModel command)
        {
            var startTime = DateTime.Now;
            try
            {
                var args = command.Args;
                var isHelpCmd = args.First().ToUpper() == "HELP";
                var isHelpLearn = isHelpCmd && args.Length > 1 && args[1].ToUpper() == "LEARN";
                var isHelpSyntax = isHelpCmd && args.Length > 1 && args[1].ToUpper() == "SYNTAX";
                var cmdName = isHelpCmd ? (args.Length > 1 ? args[1].ToUpper() : "") : args.First().ToUpper();
                if (isHelpCmd && (isHelpSyntax || isHelpLearn))
                {
                    return GetHelp(command, null, isHelpSyntax, isHelpLearn);
                }
                if (isHelpCmd && args.Length == 1)
                    return AddLogAndReturnResponse(null, null, command, startTime,
                        string.Format(Localization.GetString("CommandNotFound", Constants.LocalResourcesFile),
                            cmdName.ToLower()));

                var allCommands = CommandRepository.Instance.GetCommands();
                // if no command found notify
                if (!allCommands.ContainsKey(cmdName))
                {
                    var sbError = new StringBuilder();
                    var suggestion = Utilities.GetSuggestedCommand(cmdName);
                    sbError.AppendFormat(Localization.GetString("CommandNotFound", Constants.LocalResourcesFile), cmdName.ToLower());
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        sbError.AppendFormat(Localization.GetString("DidYouMean", Constants.LocalResourcesFile), suggestion);
                    }
                    return AddLogAndReturnResponse(null, null, command, startTime, sbError.ToString());
                }
                var cmdTypeToRun = allCommands[cmdName].CommandType;

                // Instantiate and run the command
                try
                {
                    var cmdObj = (IConsoleCommand)Activator.CreateInstance(cmdTypeToRun);
                    if (isHelpCmd) return GetHelp(command, cmdObj);
                    // set env. data for command use
                    cmdObj.Initialize(args, PortalSettings, UserInfo, command.CurrentPage);
                    return AddLogAndReturnResponse(cmdObj, cmdTypeToRun, command, startTime);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return AddLogAndReturnResponse(null, null, command, startTime, ex.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return AddLogAndReturnResponse(null, null, command, startTime, ex.Message);
            }
        }

        /// <summary>
        /// Log every command run by a users.
        /// </summary>
        /// <param name="consoleCommand"></param>
        /// <param name="cmdTypeToRun"></param>
        /// <param name="command"></param>
        /// <param name="startTime"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private HttpResponseMessage AddLogAndReturnResponse(IConsoleCommand consoleCommand, Type cmdTypeToRun, CommandInputModel command,
            DateTime startTime, string error = null)
        {
            HttpResponseMessage message;
            var isValid = consoleCommand?.IsValid() ?? false;
            var logInfo = new LogInfo
            {
                LogTypeKey = "PROMPT_ALERT"
            };
            logInfo.LogProperties.Add(new LogDetailInfo("Command", FilterCommand(command.CmdLine)));
            logInfo.LogProperties.Add(new LogDetailInfo("IsValid", isValid.ToString()));

            try
            {
                if (cmdTypeToRun != null)
                    logInfo.LogProperties.Add(new LogDetailInfo("TypeFullName", cmdTypeToRun.FullName));
                if (isValid)
                {
                    var result = consoleCommand.Run();
                    if (result.PagingInfo != null)
                    {
                        if (result.PagingInfo.PageNo < result.PagingInfo.TotalPages)
                        {
                            result.Output = string.Format(Localization.GetString("Prompt_PagingMessageWithLoad", Constants.LocalResourcesFile),
                                    result.PagingInfo.PageNo, result.PagingInfo.TotalPages);

                            var args = command.Args;
                            var indexOfPage = args.Any(x => x.ToLowerInvariant() == "--page")
                                ? args.TakeWhile(arg => arg.ToLowerInvariant() != "--page").Count()
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
                            result.Output = string.Format(Localization.GetString("Prompt_PagingMessage", Constants.LocalResourcesFile),
                                    result.PagingInfo.PageNo, result.PagingInfo.TotalPages);
                        }
                    }
                    message = Request.CreateResponse(HttpStatusCode.OK, result);
                    logInfo.LogProperties.Add(new LogDetailInfo("RecordsAffected", result.Records.ToString()));
                    logInfo.LogProperties.Add(new LogDetailInfo("Output", result.Output));
                }
                else
                {
                    logInfo.LogProperties.Add(new LogDetailInfo("Output", consoleCommand?.ValidationMessage ?? error));
                    message = BadRequestResponse(consoleCommand?.ValidationMessage ?? error);
                }
            }
            catch (Exception ex)
            {
                logInfo.Exception = new ExceptionInfo(ex);
                message = BadRequestResponse(ex.Message);
            }
            logInfo.LogProperties.Add(new LogDetailInfo("ExecutionTime(hh:mm:ss)", TimeSpan.FromMilliseconds(DateTime.Now.Subtract(startTime).TotalMilliseconds).ToString(@"hh\:mm\:ss\.ffffff")));
            LogController.Instance.AddLog(logInfo);
            return message;
        }

        private HttpResponseMessage GetHelp(CommandInputModel command, IConsoleCommand consoleCommand, bool showSyntax = false, bool showLearn = false)
        {
            return Request.CreateResponse(HttpStatusCode.OK, CommandRepository.Instance.GetCommandHelp(command, consoleCommand, showSyntax, showLearn));
        }

        private static string FilterCommand(string command)
        {
            var blackList = BlackList;
            var promptBlackList = HostController.Instance.GetString("PromptBlackList", string.Empty)
                .Split(new[] { ',', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (promptBlackList.Length > 0)
            {
                blackList = blackList.Concat(promptBlackList).Distinct().ToArray();
            }
            var args = command.Split(new[] { ',', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLowerInvariant()).ToList();
            foreach (var lowerKey in blackList.Select(key => key.ToLowerInvariant())
                        .Where(lowerKey => args.Any(arg => arg.Replace("-", "") == lowerKey)))
            {
                args[args.TakeWhile(arg => arg.Replace("-", "") != lowerKey).Count() + 1] = "******";
            }
            return string.Join(" ", args);
        }

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("PersonaBar", "promptwithportalid", "{controller}/{action}/{portalId}", null, new { portalId = "-?\\d+" }, new[] { "Dnn.PersonaBar.Prompt.Services" });
        }
    }
}