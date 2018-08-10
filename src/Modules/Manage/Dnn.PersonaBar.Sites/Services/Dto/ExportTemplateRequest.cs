#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using Dnn.PersonaBar.Library.DTO.Tabs;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.Sites.Services.Dto
{
    [DataContract]
    public class ExportTemplateRequest
    {
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "portalId")]
        public int PortalId { get; set; }

        [DataMember(Name = "pages")]
        public IEnumerable<TabDto> Pages { get; set; }

        [DataMember(Name = "locales")]
        public IEnumerable<string> Locales { get; set; }

        [DataMember(Name = "localizationCulture")]
        public string LocalizationCulture { get; set; }

        [DataMember(Name = "isMultilanguage")]
        public bool IsMultilanguage { get; set; }

        [DataMember(Name = "includeContent")]
        public bool IncludeContent { get; set; }

        [DataMember(Name = "includeFiles")]
        public bool IncludeFiles { get; set; }

        [DataMember(Name = "includeRoles")]
        public bool IncludeRoles { get; set; }

        [DataMember(Name = "includeProfile")]
        public bool IncludeProfile { get; set; }

        [DataMember(Name = "includeModules")]
        public bool IncludeModules { get; set; }
    }
}