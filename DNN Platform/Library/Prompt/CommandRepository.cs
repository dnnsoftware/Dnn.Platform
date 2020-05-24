// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Abstractions.Prompt;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Prompt.Attributes;
using DotNetNuke.Prompt.Output;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Caching;
using static DotNetNuke.Prompt.Common.Constants;

namespace DotNetNuke.Prompt
{
    public class CommandRepository : ServiceLocator<ICommandRepository, CommandRepository>, ICommandRepository
    {
        protected override Func<ICommandRepository> GetFactory()
        {
            return () => new CommandRepository();
        }

        public SortedDictionary<string, ICommand> GetCommands()
        {
            return
                DataCache.GetCachedData<SortedDictionary<string, ICommand>>(
                    new CacheItemArgs("NewDnnPromptCommands", CacheItemPriority.Default),
                    c => GetCommandsInternal());
        }

        private static SortedDictionary<string, ICommand> GetCommandsInternal()
        {
            var commands = new SortedDictionary<string, ICommand>();
            var typeLocator = new TypeLocator();
            var allCommandTypes = typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible &&
                     typeof(IConsoleCommand).IsAssignableFrom(t));
            foreach (var cmd in allCommandTypes)
            {
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
                var assemblyName = cmd.Assembly.GetName();
                var version = assemblyName.Version.ToString();
                var commandAttribute = (ConsoleCommandAttribute)attr;
                var key = commandAttribute.Name.ToUpper();
                var localResourceFile = ((IConsoleCommand)Activator.CreateInstance(cmd))?.LocalResourceFile;
                commands.Add(key, new Command
                {
                    Category = LocalizeString(commandAttribute.CategoryKey, localResourceFile),
                    Description = LocalizeString(commandAttribute.DescriptionKey, localResourceFile),
                    Key = key,
                    Name = commandAttribute.Name,
                    Version = version,
                    CommandType = cmd
                });
            }
            return commands;
        }

        public ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand)
        {
            var cacheKey = $"{consoleCommand.GetType().Name}-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}";
            return DataCache.GetCachedData<ICommandHelp>(new CacheItemArgs(cacheKey, CacheItemPriority.Low),
                c => GetCommandHelpInternal(consoleCommand));
        }

        private ICommandHelp GetCommandHelpInternal(IConsoleCommand consoleCommand)
        {
            var commandHelp = new CommandHelp();
            if (consoleCommand != null)
            {
                var cmd = consoleCommand.GetType();
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() as ConsoleCommandAttribute ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
                commandHelp.Name = attr.Name;
                commandHelp.Description = LocalizeString(attr.DescriptionKey, consoleCommand.LocalResourceFile);
                var commandParameters = cmd.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                    .Select(x => x.GetCustomAttributes(typeof(ConsoleCommandParameterAttribute), false).FirstOrDefault())
                    .Cast<ConsoleCommandParameterAttribute>().ToList();
                if (commandParameters.Any())
                {
                    var options = commandParameters.Where(attribute => attribute != null).Select(attribute => new CommandOption
                    {
                        Name = attribute.Name,
                        Required = attribute.Required,
                        DefaultValue = attribute.DefaultValue,
                        Description =
                               LocalizeString(attribute.DescriptionKey, consoleCommand.LocalResourceFile)
                    }).ToList();
                    commandHelp.Options = options;
                }
                commandHelp.ResultHtml = consoleCommand.ResultHtml;
            }
            else
            {
                commandHelp.Error = LocalizeString("Prompt_CommandNotFound");
            }
            return commandHelp;
        }

        private static string LocalizeString(string key, string resourcesFile = DefaultPromptResourceFile)
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
