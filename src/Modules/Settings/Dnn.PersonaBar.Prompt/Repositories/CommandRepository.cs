using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Reflections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Caching;
using Dnn.PersonaBar.Library.Prompt;

namespace Dnn.PersonaBar.Prompt.Repositories
{
    public class CommandRepository : ServiceLocator<ICommandRepository, CommandRepository>, ICommandRepository
    {
        protected override Func<ICommandRepository> GetFactory()
        {
            return () => new CommandRepository();
        }
        public SortedDictionary<string, Command> GetCommands()
        {
            return DotNetNuke.Common.Utilities.DataCache.GetCachedData<SortedDictionary<string, Command>>(new DotNetNuke.Common.Utilities.CacheItemArgs("DnnPromptCommands", CacheItemPriority.Default), c => GetCommandsInternal());
        }
        private SortedDictionary<string, Command> GetCommandsInternal()
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
                var key = commandAttribute.NameSpace == "" ? commandAttribute.Name.ToUpper() :
                    $"{commandAttribute.NameSpace.ToUpper()}.{commandAttribute.Name.ToUpper()}";
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
    }
}