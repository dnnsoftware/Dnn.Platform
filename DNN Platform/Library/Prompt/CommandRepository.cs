// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Caching;

using DotNetNuke.Abstractions.Prompt;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Services.Localization;

/// <summary>The repository handles retrieving of commands from the entire DNN installation.</summary>
public class CommandRepository : ServiceLocator<ICommandRepository, CommandRepository>, ICommandRepository
{
    /// <inheritdoc/>
    public IEnumerable<ICommand> GetCommands()
    {
        return this.CommandList().Values;
    }

    /// <inheritdoc/>
    public IConsoleCommand GetCommand(string commandName)
    {
        commandName = commandName.ToUpper();
        var allCommands = this.CommandList();
        if (allCommands.ContainsKey(commandName))
        {
            return (IConsoleCommand)Activator.CreateInstance(Type.GetType(allCommands[commandName].TypeFullName));
        }

        return null;
    }

    /// <inheritdoc/>
    public ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand)
    {
        var cacheKey = $"{consoleCommand.GetType().Name}-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}";
        return DataCache.GetCachedData<ICommandHelp>(
            new CacheItemArgs(cacheKey, CacheItemPriority.Low),
            c => this.GetCommandHelpInternal(consoleCommand));
    }

    /// <inheritdoc/>
    protected override Func<ICommandRepository> GetFactory()
    {
        return () => new CommandRepository();
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
            var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
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
                TypeFullName = cmd.AssemblyQualifiedName,
            });
        }

        return commands;
    }

    private static string LocalizeString(string key, string resourcesFile = Constants.DefaultPromptResourceFile)
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

    private SortedDictionary<string, ICommand> CommandList()
    {
        return
            DataCache.GetCachedData<SortedDictionary<string, ICommand>>(
                new CacheItemArgs("DnnPromptCommandList", CacheItemPriority.Default),
                c => GetCommandsInternal());
    }

    private ICommandHelp GetCommandHelpInternal(IConsoleCommand consoleCommand)
    {
        var commandHelp = new CommandHelp();
        if (consoleCommand != null)
        {
            var cmd = consoleCommand.GetType();
            var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() as ConsoleCommandAttribute ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
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
                        LocalizeString(attribute.DescriptionKey, consoleCommand.LocalResourceFile),
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
}
