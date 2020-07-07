// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.AppEvents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Dnn.PersonaBar.Library.AppEvents.Attributes;
    using Dnn.PersonaBar.Library.Common;
    using DotNetNuke.Collections;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;

    public class EventsController : ServiceLocator<IEventsController, EventsController>, IEventsController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventsController));

        private static readonly object LockThis = new object();
        private static bool _isInitialized;

        public void ApplicationStartEvent()
        {
            lock (LockThis)
            {
                if (_isInitialized)
                {
                    throw new InvalidOperationException("ApplicationStartEvent cannot be called more than once");
                }

                _isInitialized = true;
            }

            GetEventsImplements<IAppEvents>().ForEach(instance =>
            {
                try
                {
                    instance.ApplicationBegin();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(
                        "{0}.ApplicationStart threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName, e.Message, e.StackTrace);
                }
            });
        }

        public void ApplicationEndEvent()
        {
            GetEventsImplements<IAppEvents>().ForEach(instance =>
            {
                try
                {
                    instance.ApplicationEnd();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(
                        "{0}.ApplicationEnd threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName, e.Message, e.StackTrace);
                }
            });
        }

        protected override Func<IEventsController> GetFactory()
        {
            return () => new EventsController();
        }

        private static IEnumerable<T> GetEventsImplements<T>()
            where T : class
        {
            var types = GetAllEventTypes<T>();

            foreach (var type in types)
            {
                T appEventHandler;
                try
                {
                    appEventHandler = Activator.CreateInstance(type) as T;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(
                        "Unable to create {0} while calling Application start implementors.  {1}",
                        type.FullName, e.Message);
                    appEventHandler = null;
                }

                if (appEventHandler != null)
                {
                    yield return appEventHandler;
                }
            }
        }

        private static IEnumerable<Type> GetAllEventTypes<T>()
            where T : class
        {
            var typeLocator = new TypeLocator();
            return typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible &&
                     typeof(T).IsAssignableFrom(t) &&
                     (IgnoreVersionMatchCheck(t) || VersionMatched(t)));
        }

        private static bool IgnoreVersionMatchCheck(Type type)
        {
            return type.GetCustomAttributes(true).Any(a => a is IgnoreVersionMatchCheckAttribute);
        }

        private static bool VersionMatched(Type t)
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var typeVersion = t.Assembly.GetName().Version;

            var matched = currentVersion.Major == typeVersion.Major &&
                   currentVersion.Minor == typeVersion.Minor &&
                   currentVersion.Build == typeVersion.Build;

            if (!matched)
            {
                Logger.InfoFormat(
                    "Type \"{0}\"'s version ({1}) doesn't match current version({2}) so ignored",
                    t.FullName, typeVersion, currentVersion);
            }

            return matched;
        }
    }
}
