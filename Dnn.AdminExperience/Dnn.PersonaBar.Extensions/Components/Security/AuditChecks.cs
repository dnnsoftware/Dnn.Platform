// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Security.Components.Checks;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides a set of security and configuration audit checks for a DNN installation.
    /// </summary>
    public class AuditChecks
    {
        private readonly IHostSettings hostSettings;
        private readonly IPagesController pagesController;
        private readonly IEnumerable<IAuditCheck> auditChecks;

        /// <summary>Initializes a new instance of the <see cref="AuditChecks"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IPagesController. Scheduled removal in v12.0.0.")]
        public AuditChecks()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditChecks"/> class.
        /// </summary>
        /// <param name="pagesController">Provides information about pages.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostController. Scheduled removal in v12.0.0.")]
        public AuditChecks(IPagesController pagesController)
            : this(pagesController, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AuditChecks"/> class.</summary>
        /// <param name="pagesController">Provides information about pages.</param>
        /// <param name="hostSettings">Provides information about host settings.</param>
        public AuditChecks(IPagesController pagesController, IHostSettings hostSettings)
        {
            this.pagesController = pagesController ?? Globals.DependencyProvider.GetRequiredService<IPagesController>();
            this.hostSettings = hostSettings ?? Globals.DependencyProvider.GetRequiredService<IHostSettings>();

            var checks = new List<IAuditCheck>
            {
                new CheckDebug(),
                new CheckTracing(),
                new CheckBiography(),
                new CheckSiteRegistration(),
                new CheckRarelyUsedSuperuser(),
                new CheckSuperuserOldPassword(),
                new CheckUnexpectedExtensions(),
                new CheckDefaultPage(),
                new CheckModuleHeaderAndFooter(),
                new CheckPasswordFormat(),
                new CheckDiskAcccessPermissions(),
                new CheckSqlRisk(),
                new CheckAllowableFileExtensions(),
                new CheckHiddenSystemFiles(),
                new CheckTelerikPresence(),
                new CheckUserProfilePage(this.pagesController),
            };

            if (Globals.NETFrameworkVersion <= new Version(4, 5, 1))
            {
                checks.Insert(2, new CheckViewstatemac());
            }

            var knownHostGuidCheck = new CheckKnownHostGuid(this.hostSettings);
            if (knownHostGuidCheck.ShouldAlert)
            {
                checks.Add(knownHostGuidCheck);
            }

            this.auditChecks = checks.AsReadOnly();
        }

        /// <summary>
        /// Performs all configured audit checks and returns their results.
        /// </summary>
        /// <param name="checkAll">true to execute all checks regardless of their lazy loading configuration; false to execute only checks that
        /// are not marked for lazy loading.</param>
        /// <returns>A list of CheckResult objects representing the outcome of each audit check. Each result indicates the
        /// severity and status of the corresponding check.</returns>
        public List<CheckResult> DoChecks(bool checkAll = false)
        {
            var results = new List<CheckResult>();
            foreach (var check in this.auditChecks)
            {
                try
                {
                    var result = checkAll || !check.LazyLoad ? check.Execute() : new CheckResult(SeverityEnum.Unverified, check.Id);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    var result = new CheckResult(SeverityEnum.Unverified, check.Id);
                    result.Notes.Add("An error occurred, Message: " + HttpUtility.HtmlEncode(ex.Message));
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Executes the audit check identified by the specified ID and returns the result.
        /// </summary>
        /// <param name="id">The unique identifier of the audit check to execute. The comparison is case-insensitive.</param>
        /// <returns>A <see cref="CheckResult"/> representing the outcome of the audit check. If no check with the specified ID
        /// exists, or if an error occurs during execution, the result will have a severity of <see
        /// cref="SeverityEnum.Unverified"/> and the specified ID.</returns>
        public CheckResult DoCheck(string id)
        {
            try
            {
                var check = this.auditChecks.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                return check?.Execute();
            }
            catch (Exception)
            {
                return new CheckResult(SeverityEnum.Unverified, id);
            }
        }
    }
}
