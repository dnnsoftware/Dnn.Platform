using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Services.Localization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Prompt.Components.Repositories
{
    public class CommandRepository : ServiceLocator<ICommandRepository, CommandRepository>, ICommandRepository
    {
        protected override Func<ICommandRepository> GetFactory()
        {
            return () => new CommandRepository();
        }

        public SortedDictionary<string, Command> GetCommands()
        {
            return
                DataCache.GetCachedData<SortedDictionary<string, Command>>(
                    new CacheItemArgs("DnnPromptCommands", CacheItemPriority.Default),
                    c => GetCommandsInternal());
        }

        private static SortedDictionary<string, Command> GetCommandsInternal()
        {
            var commands = new SortedDictionary<string, Command>();
            var typeLocator = new TypeLocator();
            var allCommandTypes = typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible &&
                     typeof(ConsoleCommandBase).IsAssignableFrom(t));
            foreach (var cmd in allCommandTypes)
            {
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault();
                if (attr == null) continue;
                var assemblyName = cmd.Assembly.GetName();
                var version = assemblyName.Version.ToString();
                var name = assemblyName.Name;
                var commandAttribute = (ConsoleCommandAttribute)attr;
                var key = commandAttribute.NameSpace == ""
                    ? commandAttribute.Name.ToUpper()
                    : $"{commandAttribute.NameSpace.ToUpper()}.{commandAttribute.Name.ToUpper()}";
                commands.Add(key, new Command()
                {
                    AssemblyName = name,
                    CommandType = cmd,
                    Description = commandAttribute.Description,
                    Key = key,
                    Name = commandAttribute.Name,
                    NameSpace = commandAttribute.NameSpace,
                    Version = version
                });
            }
            return commands;
        }

        public CommandHelp GetCommandHelp(CommandInputModel command, IConsoleCommand consoleCommand, bool showSyntax = false, bool showLearn = false)
        {
            var cacheKey = (string.Join("_", command.Args) + "_" + PortalController.Instance.GetCurrentPortalSettings()?.DefaultLanguage).Replace("-", "_");
            cacheKey = $"{cacheKey}_{(showSyntax ? "1" : "0")}_{(showLearn ? "1" : "0")}}}";
            return DataCache.GetCachedData<CommandHelp>(new CacheItemArgs(cacheKey, CacheItemPriority.Low),
                c => GetCommandHelpInternal(consoleCommand, showSyntax, showLearn));
        }

        private CommandHelp GetCommandHelpInternal(IConsoleCommand consoleCommand, bool showSyntax = false, bool showLearn = false)
        {
            var commandHelp = new CommandHelp();
            if (consoleCommand != null)
            {
                var type = consoleCommand.GetType();
                var attr =
                    type.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() as
                        ConsoleCommandAttribute;
                if (attr != null)
                {
                    commandHelp.Name = attr.Name;
                    commandHelp.Description = Localization.GetString(attr.Description,
                        consoleCommand.LocalResourceFile);
                    var flagAttributes = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                        .Select(x => x.GetCustomAttributes(typeof(FlagParameterAttribute), false).FirstOrDefault())
                        .Cast<FlagParameterAttribute>().ToList();
                    if (flagAttributes.Any())
                    {
                        var options = flagAttributes.Where(attribute => attribute != null).Select(attribute => new CommandOption
                        {
                            Flag = attribute.Flag,
                            Type = attribute.Type,
                            Required = attribute.Required,
                            DefaultValue = attribute.DefaultValue,
                            Description =
                                   Localization.GetString(attribute.Description, consoleCommand.LocalResourceFile)
                        }).ToList();
                        commandHelp.Options = options;
                    }
                    commandHelp.ResultHtml = consoleCommand.ResultHtml;
                }
                else
                {
                    commandHelp.Error = Localization.GetString("Prompt_CommandNotFound", Constants.LocalResourcesFile);
                }
            }
            else if (showLearn)
            {
                commandHelp.ResultHtml = Localization.GetString("Prompt_CommandHelpSyntax", Constants.LocalResourcesFile);
            }
            else if (showSyntax)
            {
                commandHelp.ResultHtml = Localization.GetString("Prompt_CommandHelpLearn", Constants.LocalResourcesFile);
            }
            else
            {
                commandHelp.Error = Localization.GetString("Prompt_CommandNotFound", Constants.LocalResourcesFile);
            }
            return commandHelp;
        }
    }
}