// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Security;

namespace DotNetNuke.Services.FileSystem.Internal.SecurityCheckers
{
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
                //when there have exception occur, just return false as not validated, no need log the error.
            }

            return false;
        }
    }
}
