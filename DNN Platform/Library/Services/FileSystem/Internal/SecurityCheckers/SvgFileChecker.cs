﻿#region Copyright
// 
// DotNetNuke® - https://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
