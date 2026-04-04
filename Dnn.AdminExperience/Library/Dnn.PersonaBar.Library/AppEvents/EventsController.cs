// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.AppEvents
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Dnn.PersonaBar.Library.AppEvents.Attributes;
    using Dnn.PersonaBar.Library.Common;
    using DotNetNuke.Collections;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.Logging;

    /// <summary>The default <see cref="IEventsController"/> implementation.</summary>
    public partial class EventsController : ServiceLocator<IEventsController, EventsController>, IEventsController
    {
        private static readonly ILogger Logger = DnnLoggingController.GetLogger<EventsController>();

        private static readonly object LockThis = new object();
        private static bool isInitialized;

        /// <inheritdoc />
        public void ApplicationStartEvent()
        {
            lock (LockThis)
            {
                if (isInitialized)
                {
                    throw new InvalidOperationException("ApplicationStartEvent cannot be called more than once");
                }

                isInitialized = true;
            }

            GetEventsImplements<IAppEvents>().ForEach(instance =>
            {
                try
                {
                    instance.ApplicationBegin();
                }
                catch (Exception e)
                {
                    Logger.EventsControllerApplicationStartThrewAnException(e, instance.GetType().FullName);
                }
            });
        }

        /// <inheritdoc />
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
                    Logger.EventsControllerApplicationEndThrewAnException(e, instance.GetType().FullName);
                }
            });
        }

        /// <inheritdoc />
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
                    Logger.EventsControllerUnableToCreateAppEventHandler(e, type.FullName);
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
                t => t is { IsClass: true, IsAbstract: false, IsVisible: true, } &&
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
                Logger.EventsControllerVersionMismatch(t.FullName, typeVersion, currentVersion);
            }

            return matched;
        }
    }
}
