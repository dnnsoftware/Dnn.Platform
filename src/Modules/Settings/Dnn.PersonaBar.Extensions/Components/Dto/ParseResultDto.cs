#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [DataContract]
    public class ParseResultDto : PackageInfoDto
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; } = true;

        [DataMember(Name = "azureCompact")]
        public bool AzureCompact { get; set; }

        [DataMember(Name = "noManifest")]
        public bool NoManifest { get; set; }

        [DataMember(Name = "legacyError")]
        public string LegacyError { get; set; }

        [DataMember(Name = "hasInvalidFiles")]
        public bool HasInvalidFiles { get; set; }

        [DataMember(Name = "alreadyInstalled")]
        public bool AlreadyInstalled { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        public ParseResultDto()
        {
            
        }

        public ParseResultDto(PackageInfo package) : base(Null.NullInteger, package)
        {
            
        }

        public void Failed(string message, IList<string> logs = null)
        {
            Success = false;
            Message = message;
        }

        public void Succeed(IList<string> logs)
        {
            Success = true;
            Message = string.Empty;
        }

    }
}