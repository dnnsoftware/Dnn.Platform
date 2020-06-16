// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal.SecurityCheckers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Security;

    public class SvgFileChecker : IFileSecurityChecker
    {
        public bool Validate(Stream fileContent)
        {
            try
            {
                string svgContent;
                using (var reader = new StreamReader(fileContent))
                {
                    svgContent = reader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(svgContent))
                {
                    return false;
                }

                return PortalSecurity.Instance.ValidateInput(svgContent, PortalSecurity.FilterFlag.NoScripting);
            }
            catch (Exception)
            {
                // when there have exception occur, just return false as not validated, no need log the error.
            }

            return false;
        }
    }
}
