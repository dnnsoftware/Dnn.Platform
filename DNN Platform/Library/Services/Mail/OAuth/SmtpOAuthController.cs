// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;

    /// <summary>
    /// Smtp oauth controller.
    /// </summary>
    public class SmtpOAuthController : ServiceLocator<ISmtpOAuthController, SmtpOAuthController>, ISmtpOAuthController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SmtpOAuthController));

        /// <summary>
        /// Get the oauth provider.
        /// </summary>
        /// <param name="name">provider name.</param>
        /// <returns>the oauth provider.</returns>
        public ISmtpOAuthProvider GetOAuthProvider(string name)
        {
            return this.GetOAuthProviders().FirstOrDefault(i => i.Name == name);
        }

        /// <summary>
        /// Get email smtp auth providers.
        /// </summary>
        /// <returns>smtp oauth providers list.</returns>
        public IList<ISmtpOAuthProvider> GetOAuthProviders()
        {
            var typeLocator = new TypeLocator();
            return typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     typeof(ISmtpOAuthProvider).IsAssignableFrom(t)).Select(t =>
                     {
                         try
                         {
                             return Activator.CreateInstance(t) as ISmtpOAuthProvider;
                         }
                         catch (Exception e)
                         {
                             Logger.ErrorFormat("Unable to create {0} while getting all smtp oauth providers. {1}", t.FullName, e.Message);
                             return null;
                         }
                     }).Where(i => i != null).ToList();
        }

        /// <summary>
        /// Implement the service locator interface.
        /// </summary>
        /// <returns>the controller instance.</returns>
        protected override Func<ISmtpOAuthController> GetFactory()
        {
            return () => new SmtpOAuthController();
        }
    }
}
