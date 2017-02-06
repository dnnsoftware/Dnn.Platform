using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Reflections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Caching;

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
            return DotNetNuke.Common.Utilities.DataCache.GetCachedData<SortedDictionary<string, Command>>(new DotNetNuke.Common.Utilities.CacheItemArgs("PromptCommands", CacheItemPriority.Default), c => GetCommandsInternal());
        }
        private SortedDictionary<string, Command> GetCommandsInternal()
        {
            var res = new SortedDictionary<string, Command>();
            var typeLocator = new TypeLocator();
            var types = typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible &&
                     typeof(BaseConsoleCommand).IsAssignableFrom(t));
            foreach (var cmd in types)
            {
                var attr = cmd.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault();
                if (attr != null)
                {
                    var assy = cmd.Assembly.GetName();
                    var version = assy.Version.ToString();
                    var assyName = assy.Name;
                    var cmdAttr = (ConsoleCommandAttribute)attr;
                    var key = cmdAttr.NameSpace == "" ? cmdAttr.Name.ToUpper() : string.Format("{0}.{1}", cmdAttr.NameSpace.ToUpper(), cmdAttr.Name.ToUpper());
                    res.Add(key, new Command()
                    {
                        AssemblyName = assyName,
                        CommandAttribute = cmdAttr,
                        CommandType = cmd,
                        Description = cmdAttr.Description,
                        Key = key,
                        Name = cmdAttr.Name,
                        NameSpace = cmdAttr.NameSpace,
                        Version = version
                    });
                }
            }
            return res;
        }
    }
    public interface ICommandRepository
    {
        SortedDictionary<string, Command> GetCommands();
    }
}