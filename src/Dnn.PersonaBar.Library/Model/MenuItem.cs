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
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using Dnn.PersonaBar.Library.Repository;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Library.Model
{
    [DataContract]
    [Serializable]
    public class MenuItem : IHydratable
    {
        private string _parent;

        [DataMember]
        public int MenuId { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public string ModuleName { get; set; }

        [DataMember]
        public string FolderName { get; set; }

        [IgnoreDataMember]
        public string Controller { get; set; }

        [DataMember]
        public string ResourceKey { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public string CssClass { get; set; }

        [DataMember]
        public string IconFile { get; set; }

        [DataMember]
        public int ParentId { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool AllowHost { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public string Parent
        {
            get
            {
                if (ParentId == Null.NullInteger)
                {
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(_parent))
                {
                    var parentItem = PersonaBarRepository.Instance.GetMenuItem(ParentId);
                    if (parentItem != null)
                    {
                        _parent = parentItem.Identifier;
                    }
                }

                return _parent;
            }
        }

        [DataMember]
        public string DisplayName
        {
            get
            {
                var resourcesPath = System.IO.Path.Combine(Constants.PersonaBarModulesPath, Identifier, "App_LocalResources", ModuleName + ".resx");
                var displayName = Localization.GetString(ResourceKey, resourcesPath, true);
                if (Localization.ShowMissingKeys)
                {
                    if (string.IsNullOrEmpty(displayName))
                    {
                        resourcesPath = System.IO.Path.Combine(Constants.PersonaBarRelativePath, "App_LocalResources", "PersonaBar.resx");
                    }
                    displayName = Localization.GetString(ResourceKey, resourcesPath);
                }
                else if (string.IsNullOrEmpty(displayName))
                {
                    resourcesPath = System.IO.Path.Combine(Constants.PersonaBarRelativePath, "App_LocalResources", "PersonaBar.resx");
                    displayName = Localization.GetString(ResourceKey, resourcesPath);
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = ResourceKey;
                }

                return displayName;
            }
        }

        [DataMember]
        public string Settings { get; set; }

        [DataMember]
        public IList<MenuItem> Children { get; set; } = new List<MenuItem>();

        public void Fill(IDataReader dr)
        {
            MenuId = Convert.ToInt32(dr["MenuId"]);
            Identifier = dr["Identifier"].ToString();
            ModuleName = dr["ModuleName"].ToString();
            FolderName = Null.SetNullString(dr["FolderName"]);
            Controller = dr["Controller"].ToString();
            ResourceKey = dr["ResourceKey"].ToString();
            Path = dr["Path"].ToString();
            Link = dr["Link"].ToString();
            CssClass = dr["CssClass"].ToString();
            IconFile = dr["IconFile"].ToString();
            AllowHost = Convert.ToBoolean(dr["AllowHost"]);
            Enabled = Convert.ToBoolean(dr["Enabled"]);
            ParentId = Null.SetNullInteger(dr["ParentId"]);
            Order = Null.SetNullInteger(dr["Order"]);
        }

        public int KeyID { get { return MenuId; } set { MenuId = value; } }
    }
}
