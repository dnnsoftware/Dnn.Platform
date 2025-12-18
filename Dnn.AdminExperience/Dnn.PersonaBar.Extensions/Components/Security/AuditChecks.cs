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

    using DotNetNuke.Common;

    public class AuditChecks
    {
        private readonly IEnumerable<IAuditCheck> auditChecks;

        /// <summary>Initializes a new instance of the <see cref="AuditChecks"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IPagesController. Scheduled removal in v12.0.0.")]
        public AuditChecks()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AuditChecks"/> class.</summary>
        /// <param name="pagesController">The pages controller.</param>
        public AuditChecks(IPagesController pagesController)
        {
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
                new CheckUserProfilePage(pagesController),
            };

            if (Globals.NETFrameworkVersion <= new Version(4, 5, 1))
            {
                checks.Insert(2, new CheckViewstatemac());
            }

            this.auditChecks = checks.AsReadOnly();
        }

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

        public CheckResult DoCheck(string id)
        {
            try
            {
                var check = this.auditChecks.FirstOrDefault(c => c.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
                return check?.Execute();
            }
            catch (Exception)
            {
                return new CheckResult(SeverityEnum.Unverified, id);
            }
        }
    }
}
