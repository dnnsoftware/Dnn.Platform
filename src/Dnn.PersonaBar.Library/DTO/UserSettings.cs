#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Library.DTO
{
    /// <summary>
    /// Persona Bar User Settings
    /// </summary>
    [DataContract]
    public class UserSettings
    {
        [DataMember(Name = "expandPersonaBar")]
        public bool ExpandPersonaBar{ get; set; }

        [DataMember(Name = "activePath")]
        public string ActivePath { get; set; }

        [DataMember(Name = "activeIdentifier")]
        public string ActiveIdentifier { get; set; }

        [DataMember(Name = "expandTasksPane")]
        public bool ExpandTasksPane { get; set; }

        [DataMember(Name = "comparativeTerm")]
        public string ComparativeTerm { get; set; }

        [DataMember(Name = "endDate")]
        public DateTime EndDate { get; set; }

        [DataMember(Name = "legends")]
        public string[] Legends { get; set; }

        [DataMember(Name = "period")]
        public string Period { get; set; }

        [DataMember(Name = "startDate")]
        public DateTime StartDate { get; set; }
    }
}