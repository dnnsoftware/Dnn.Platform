using Dnn.PersonaBar.Prompt.Models;
using Dnn.PersonaBar.Prompt.Repositories;
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

namespace Dnn.PersonaBar.Prompt.Services
{
    [MenuPermission(MenuName = "Dnn.Prompt")]
    [RequireHost]
    public class CommandController : ControllerBase
    {
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
                    var suggestion = GetSuggestedCommand(cmdName);
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
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return BadRequestResponse();
                }
            }
            catch (Exception ex)
            {
                return BadRequestResponse();
            }
        }

        private string GetSuggestedCommand(string cmdName)
        {

            var match = Regex.Match(cmdName, "(\\w+)\\-(\\w+)");
            if (match.Success)
            {
                var verb = match.Groups[1].Value;
                var component = match.Groups[2].Value;
                switch (verb)
                {
                    case "CREATE":
                    case "ADD":
                        switch (component)
                        {
                            case "USER":
                                return "new-user";
                            case "PAGE":
                                return "new-page";
                            case "ROLE":
                                return "new-role";
                        }
                        break;
                    case "LOAD":
                    case "FIND":
                        switch (component)
                        {
                            case "USER":
                            case "USERS":
                                return "get-user or list-users";
                            case "PAGE":
                            case "PAGES":
                                return "get-page or list-pages";
                            case "ROLE":
                            case "ROLES":
                                return "get-role or list-roles";
                        }
                        break;
                    case "UPDATE":
                    case "CHANGE":
                        switch (component)
                        {
                            case "USER":
                                return "set-user";
                            case "PAGE":
                                return "set-page";
                            case "PASSWORD":
                                return "reset-password";
                        }
                        break;
                    case "SET":
                        switch (component)
                        {
                            case "PASSWORD":
                                return "reset-password";
                        }
                        break;
                    case "GET":
                        switch (component)
                        {
                            case "ROLES":
                                return "get-role or list-roles";
                            case "USERS":
                                return "get-user or list-users";
                            case "PAGES":
                                return "get-page list-pages";
                            case "MODULES":
                                return "get-module list-modules";
                        }
                        break;
                    case "LIST":
                        switch (component)
                        {
                            case "USER":
                                return "list-users";
                            case "ROLE":
                                return "list-roles";
                            case "PAGE":
                                return "list-pages";
                            case "Module":
                                return "list-modules";
                        }
                        break;
                    case "RECOVER":
                        switch (component)
                        {
                            case "USER":
                                return "restore-user";
                        }
                        break;
                    case "REMOVE":
                        switch (component)
                        {
                            case "USER":
                                return "delete-user or purge-user";
                        }
                        break;
                }
            }

            return string.Empty;
        }

    }
}