// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.HttpModules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;

    using Dnn.PersonaBar.Library.AppEvents;
    using Dnn.PersonaBar.Library.Common;
    using DotNetNuke.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.UI.Skins.EventListeners;

    /// <summary>An <see cref="IHttpModule"/> which registers components for the Persona Bar.</summary>
    public class PersonaBarModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarModule));

        private static readonly object LockAppStarted = new object();
        private static bool hasAppStarted;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
            if (hasAppStarted)
            {
                return;
            }

            lock (LockAppStarted)
            {
                if (hasAppStarted)
                {
                    return;
                }

                ApplicationStart();
                hasAppStarted = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            EventsController.Instance.ApplicationEndEvent();
        }

        private static void ApplicationStart()
        {
            EventsController.Instance.ApplicationStartEvent();

            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinInit, OnSkinInit));
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinLoad, OnSkinLoad));
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinPreRender, OnSkinPreRender));
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinUnLoad, OnSkinUnLoad));
        }

        private static void OnSkinInit(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.Init(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "{0}.Init threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName,
                        ex.Message,
                        ex.StackTrace);
                }
            });
        }

        private static void OnSkinLoad(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.Load(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "{0}.Load threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName,
                        ex.Message,
                        ex.StackTrace);
                }
            });
        }

        private static void OnSkinPreRender(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.PreRender(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "{0}.PreRender threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName,
                        ex.Message,
                        ex.StackTrace);
                }
            });
        }

        private static void OnSkinUnLoad(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.UnLoad(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "{0}.UnLoad threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName,
                        ex.Message,
                        ex.StackTrace);
                }
            });
        }
    }
}
