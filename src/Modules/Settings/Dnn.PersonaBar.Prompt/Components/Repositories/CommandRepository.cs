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
using System.Text.RegularExpressions;

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
                     typeof(IConsoleCommand).IsAssignableFrom(t));
            foreach (var cmd in allCommandTypes)
            {
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.GeneralCategory, $"Prompt_{cmd.Name}_Description");
                var assemblyName = cmd.Assembly.GetName();
                var version = assemblyName.Version.ToString();
                var commandAttribute = (ConsoleCommandAttribute)attr;
                var key = commandAttribute.Name.ToUpper();
                var localResourceFile = ((IConsoleCommand)Activator.CreateInstance(cmd))?.LocalResourceFile;
                commands.Add(key, new Command
                {
                    Category = LocalizeString(commandAttribute.Category, localResourceFile),
                    Description = LocalizeString(commandAttribute.Description, localResourceFile),
                    Key = key,
                    Name = commandAttribute.Name,
                    Version = version,
                    CommandType = cmd
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
                var cmd = consoleCommand.GetType();
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() as ConsoleCommandAttribute ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.GeneralCategory, $"Prompt_{cmd.Name}_Description");
                commandHelp.Name = attr.Name;
                commandHelp.Description = LocalizeString(attr.Description, consoleCommand.LocalResourceFile);
                var flagAttributes = cmd.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
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
                               LocalizeString(attribute.Description, consoleCommand.LocalResourceFile)
                    }).ToList();
                    commandHelp.Options = options;
                }
                commandHelp.ResultHtml = consoleCommand.ResultHtml;
            }
            else if (showLearn)
            {
                commandHelp.ResultHtml = LocalizeString("Prompt_CommandHelpLearn");
            }
            else if (showSyntax)
            {
                commandHelp.ResultHtml = LocalizeString("Prompt_CommandHelpSyntax");
            }
            else
            {
                commandHelp.Error = LocalizeString("Prompt_CommandNotFound");
            }
            return commandHelp;
        }

        private static string LocalizeString(string key, string resourcesFile = Constants.LocalResourcesFile)
        {
            var localizedText = Localization.GetString(key, resourcesFile);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }

        private static string CreateCommandFromClass(string className)
        {
            var camelCasedParts = SplitCamelCase(className);
            return string.Join("-", camelCasedParts.Select(x => x.ToLower()));
        }
        private static string[] SplitCamelCase(string source)
        {
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }
    }
}