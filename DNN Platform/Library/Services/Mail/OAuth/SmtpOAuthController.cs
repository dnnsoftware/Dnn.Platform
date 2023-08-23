// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;

    /// <summary>Smtp OAuth controller.</summary>
    public class SmtpOAuthController : ServiceLocator<ISmtpOAuthController, SmtpOAuthController>, ISmtpOAuthController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SmtpOAuthController));

        private readonly TypeLocator typeLocator;

        /// <summary>Initializes a new instance of the <see cref="SmtpOAuthController"/> class.</summary>
        public SmtpOAuthController()
            : this(new TypeLocator())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SmtpOAuthController"/> class.</summary>
        /// <param name="typeLocator">The type locator to use to find <see cref="ISmtpOAuthProvider"/> instances.</param>
        public SmtpOAuthController(TypeLocator typeLocator)
        {
            this.typeLocator = typeLocator;
        }

        /// <inheritdoc />
        public ISmtpOAuthProvider GetOAuthProvider(string name)
        {
            return this.GetOAuthProviders().FirstOrDefault(i => i.Name == name);
        }

        /// <inheritdoc />
        public IList<ISmtpOAuthProvider> GetOAuthProviders()
        {
            return this.typeLocator.GetAllMatchingTypes(
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

        /// <inheritdoc />
        protected override Func<ISmtpOAuthController> GetFactory()
        {
            return () => new SmtpOAuthController();
        }
    }
}
