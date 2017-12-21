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
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

namespace Dnn.PersonaBar.Library.Model
{
    [DataContract]
    [Serializable]
    public class PersonaBarExtension : IHydratable
    {
        [DataMember]
        public int ExtensionId { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public int MenuId { get; set; }

        [DataMember]
        public string FolderName { get; set; }

        [IgnoreDataMember]
        public string Controller { get; set; }

        [DataMember]
        public string Container { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public void Fill(IDataReader dr)
        {
            ExtensionId = Convert.ToInt32(dr["ExtensionId"]);
            Identifier = dr["Identifier"].ToString();
            FolderName = Null.SetNullString(dr["FolderName"]);
            MenuId = Convert.ToInt32(dr["MenuId"]);
            Controller = dr["Controller"].ToString();
            Container = dr["Container"].ToString();
            Path = dr["Path"].ToString();
            Order = Null.SetNullInteger(dr["Order"]);
            Enabled = Convert.ToBoolean(dr["Enabled"]);
        }

        public int KeyID { get { return ExtensionId; } set { ExtensionId = value; } }
    }
}
