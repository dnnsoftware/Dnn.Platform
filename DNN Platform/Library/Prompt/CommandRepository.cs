// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The repository handles retrieving of commands from the entire DNN installation.</summary>
    public class CommandRepository : ServiceLocator<ICommandRepository, CommandRepository>, ICommandRepository
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public CommandRepository()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public CommandRepository(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
        }

        /// <summary>Initializes a new instance of the <see cref="CommandRepository"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        /// <param name="hostSettings">The host settings.</param>
        public CommandRepository(IServiceScopeFactory serviceScopeFactory, IHostSettings hostSettings)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <inheritdoc/>
        public IEnumerable<ICommand> GetCommands()
        {
            return this.CommandList().Values;
        }

        /// <inheritdoc/>
        public IConsoleCommand GetCommand(IServiceProvider serviceProvider, string commandName)
        {
            commandName = commandName.ToUpper();
            var allCommands = this.CommandList();
            if (allCommands.TryGetValue(commandName, out var command))
            {
                return (IConsoleCommand)ActivatorUtilities.CreateInstance(serviceProvider, Type.GetType(command.TypeFullName));
            }

            return null;
        }

        /// <inheritdoc/>
        public ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand)
        {
            var cacheKey = $"{consoleCommand.GetType().Name}-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}";
            return DataCache.GetCachedData<ICommandHelp>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, CacheItemPriority.Low),
                _ => GetCommandHelpInternal(consoleCommand));
        }

        /// <inheritdoc/>
        protected override Func<ICommandRepository> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<ICommandRepository>;
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

        private static CommandHelp GetCommandHelpInternal(IConsoleCommand consoleCommand)
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
                if (commandParameters.Count != 0)
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

        private SortedDictionary<string, ICommand> GetCommandsInternal()
        {
            var commands = new SortedDictionary<string, ICommand>();
            var typeLocator = new TypeLocator();
            var allCommandTypes = typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible &&
                     typeof(IConsoleCommand).IsAssignableFrom(t));

            using var serviceScope = this.serviceScopeFactory.CreateScope();
            foreach (var cmd in allCommandTypes)
            {
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() ?? new ConsoleCommandAttribute(CreateCommandFromClass(cmd.Name), Constants.CommandCategoryKeys.General, $"Prompt_{cmd.Name}_Description");
                var assemblyName = cmd.Assembly.GetName();
                var version = assemblyName.Version.ToString();
                var commandAttribute = (ConsoleCommandAttribute)attr;
                var key = commandAttribute.Name.ToUpper();

                var command = (IConsoleCommand)ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, cmd);
                var localResourceFile = command?.LocalResourceFile;

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

        private SortedDictionary<string, ICommand> CommandList()
        {
            return
                DataCache.GetCachedData<SortedDictionary<string, ICommand>>(
                    this.hostSettings,
                    new CacheItemArgs("DnnPromptCommandList", CacheItemPriority.Default),
                    _ => this.GetCommandsInternal());
        }
    }
}
