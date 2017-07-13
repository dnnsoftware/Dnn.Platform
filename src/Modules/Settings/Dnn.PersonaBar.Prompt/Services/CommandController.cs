using DotNetNuke.Web.Api;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Components;
using Dnn.PersonaBar.Prompt.Components.Models;
using Dnn.PersonaBar.Prompt.Components.Repositories;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Prompt.Services
{
    [MenuPermission(MenuName = "Dnn.Prompt")]
    [RequireHost]
    public class CommandController : ControllerBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CommandController));

        [HttpGet]
        public HttpResponseMessage List()
        {
            return Request.CreateResponse(HttpStatusCode.OK, CommandRepository.Instance.GetCommands().Values);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Cmd([FromBody] CommandInputModel command)
        {

            try
            {
                var args = command.Args;
                var cmdName = args.First().ToUpper();
                var allCommands = CommandRepository.Instance.GetCommands();
                //If command not found and command contain namespace.
                if (!allCommands.ContainsKey(cmdName) && cmdName.IndexOf('.') == -1)
                {
                    var seek = allCommands.Values.FirstOrDefault(c => c.Name.ToUpper() == cmdName);
                    // if there is a command which matches then we assume the user meant that namespace
                    if (seek != null)
                    {
                        cmdName = $"{seek.NameSpace.ToUpper()}.{cmdName}";
                    }
                }

                // if no command found notify
                if (!allCommands.ContainsKey(cmdName))
                {
                    var sbError = new StringBuilder();
                    var suggestion = Utilities.GetSuggestedCommand(cmdName);
                    sbError.AppendFormat("Command '{0}' not found.", cmdName.ToLower());
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        sbError.AppendFormat(" Did you mean '{0}'?", suggestion);
                    }

                    return BadRequestResponse(sbError.ToString());
                }
                var cmdTypeToRun = allCommands[cmdName].CommandType;

                // Instantiate and run the command
                try
                {
                    var cmdObj = (IConsoleCommand)Activator.CreateInstance(cmdTypeToRun);
                    // set env. data for command use
                    cmdObj.Init(args, PortalSettings, UserInfo, command.CurrentPage);
                    return cmdObj.IsValid()
                        ? Request.CreateResponse(HttpStatusCode.OK, cmdObj.Run())
                        : BadRequestResponse(cmdObj.ValidationMessage);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return BadRequestResponse();
                }
            }
            catch (Exception ex)
            {
                return BadRequestResponse();
            }
        }
    }
}