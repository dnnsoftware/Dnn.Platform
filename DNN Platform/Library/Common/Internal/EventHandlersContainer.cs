// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.Common.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.Instrumentation;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A container to hold event handlers.</summary>
    /// <typeparam name="T">The type of event handlers.</typeparam>
    internal class EventHandlersContainer<T> : ComponentBase<IEventHandlersContainer<T>, EventHandlersContainer<T>>, IEventHandlersContainer<T>
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventHandlersContainer<T>));

        [ImportMany]
        private IEnumerable<Lazy<T>> eventHandlers = new List<Lazy<T>>();

        /// <summary>Initializes a new instance of the <see cref="EventHandlersContainer{T}"/> class.</summary>
        public EventHandlersContainer()
        {
            try
            {
                if (GetCurrentStatus() != UpgradeStatus.None)
                {
                    return;
                }

                ExtensionPointManager.ComposeParts(this);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Lazy<T>> EventHandlers
        {
            get
            {
                return this.eventHandlers;
            }
        }

        private static UpgradeStatus GetCurrentStatus()
        {
            try
            {
                return Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>().Status;
            }
            catch (NullReferenceException)
            {
                return UpgradeStatus.Unknown;
            }
        }
    }
}
