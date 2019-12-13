// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Entities.Host;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckAllowableFileExtensions : IAuditCheck
    {
        public string Id => "CheckAllowableFileExtensions";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            try
            {
                if (Host.AllowedExtensionWhitelist.IsAllowedExtension("asp")
                        || Host.AllowedExtensionWhitelist.IsAllowedExtension("aspx")
                        || Host.AllowedExtensionWhitelist.IsAllowedExtension("php"))
                {
                    result.Severity = SeverityEnum.Failure;
                    result.Notes.Add("Extensions: " + Host.AllowedExtensionWhitelist.ToDisplayString());
                }
                else
                {
                    result.Severity = SeverityEnum.Pass;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }
    }
}
