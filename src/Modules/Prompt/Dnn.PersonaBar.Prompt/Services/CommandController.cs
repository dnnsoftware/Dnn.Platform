using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace Dnn.PersonaBar.Prompt.Services
{
    [MenuPermission(MenuName = "Dnn.Prompt", Scope = ServiceScope.Admin)]
    public class CommandController : BaseController
    {
        public static Dictionary<string, Type> Commands { get; private set; } 
        public static Dictionary<string, ConsoleCommandAttribute> CommandAttributes { get; private set; } 

        public CommandController()
        {
            try
            {
                Commands = new Dictionary<string, Type>();
                CommandAttributes = new Dictionary<string, ConsoleCommandAttribute>();
                foreach(var cmd in GetAllCommands())
                {
                    var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault();
                    if (attr != null)
                    {
                        var cmdAttr = (ConsoleCommandAttribute)attr;
                        Commands.Add(cmdAttr.Name.ToUpper(), cmd);
                        CommandAttributes.Add(cmdAttr.Name.ToUpper(), cmdAttr);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }


        }

        [ValidateAntiForgeryToken()]
        [HttpPost]
        public HttpResponseMessage Cmd([FromBody] CommandInputModel command)
        {

            try
            {
                var args = command.GetArgs();
                var cmdName = args.First().ToUpper();

                // if no command found notify
                if (!Commands.ContainsKey(cmdName))
                {
                    StringBuilder sbError = new StringBuilder();
                    string suggestion = GetSuggestedCommand(cmdName);
                    sbError.AppendFormat("Command '{0}' not found.", cmdName.ToLower());
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        sbError.AppendFormat(" Did you mean '{0}'?", suggestion);
                    }

                    return BadRequestResponse(sbError.ToString());
                }
                Type cmdTypeToRun = Commands[cmdName];

                // Instantiate and run the command
                try
                {
                    var cmdObj = (IConsoleCommand)Activator.CreateInstance(cmdTypeToRun);
                    // set env. data for command use
                    cmdObj.Init(args, PortalSettings, UserInfo, command.currentPage);
                    if (cmdObj.IsValid())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, cmdObj.Run());
                    }
                    else
                    {
                        return BadRequestResponse(cmdObj.ValidationMessage);
                    }
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

        private static IEnumerable<Type> GetAllCommands()
        {
            var typeLocator = new TypeLocator();
            return typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible &&
                     typeof(BaseConsoleCommand).IsAssignableFrom(t));
        }

    }
}