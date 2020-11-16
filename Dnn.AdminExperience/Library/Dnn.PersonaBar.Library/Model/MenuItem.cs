// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Library.Repository;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Localization;

    [DataContract]
    [Serializable]
    public class MenuItem : IHydratable
    {
        private string _parent;

        [DataMember]
        public string Parent
        {
            get
            {
                if (this.ParentId == Null.NullInteger)
                {
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(this._parent))
                {
                    var parentItem = PersonaBarRepository.Instance.GetMenuItem(this.ParentId);
                    if (parentItem != null)
                    {
                        this._parent = parentItem.Identifier;
                    }
                }

                return this._parent;
            }
        }

        [DataMember]
        public string DisplayName
        {
            get
            {
                var resourcesPath = System.IO.Path.Combine(Constants.PersonaBarModulesPath, this.Identifier, "App_LocalResources", this.ModuleName + ".resx");
                var displayName = Localization.GetString(this.ResourceKey, resourcesPath, true);
                if (Localization.ShowMissingKeys)
                {
                    if (string.IsNullOrEmpty(displayName))
                    {
                        resourcesPath = System.IO.Path.Combine(Constants.PersonaBarRelativePath, "App_LocalResources", "PersonaBar.resx");
                    }

                    displayName = Localization.GetString(this.ResourceKey, resourcesPath);
                }
                else if (string.IsNullOrEmpty(displayName))
                {
                    resourcesPath = System.IO.Path.Combine(Constants.PersonaBarRelativePath, "App_LocalResources", "PersonaBar.resx");
                    displayName = Localization.GetString(this.ResourceKey, resourcesPath);
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = this.ResourceKey;
                }

                return displayName;
            }
        }

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
        public string Settings { get; set; }

        [DataMember]
        public IList<MenuItem> Children { get; set; } = new List<MenuItem>();

        public int KeyID
        {
            get { return this.MenuId; }
            set { this.MenuId = value; }
        }

        public void Fill(IDataReader dr)
        {
            this.MenuId = Convert.ToInt32(dr["MenuId"]);
            this.Identifier = dr["Identifier"].ToString();
            this.ModuleName = dr["ModuleName"].ToString();
            this.FolderName = Null.SetNullString(dr["FolderName"]);
            this.Controller = dr["Controller"].ToString();
            this.ResourceKey = dr["ResourceKey"].ToString();
            this.Path = dr["Path"].ToString();
            this.Link = dr["Link"].ToString();
            this.CssClass = dr["CssClass"].ToString();
            this.IconFile = dr["IconFile"].ToString();
            this.AllowHost = Convert.ToBoolean(dr["AllowHost"]);
            this.Enabled = Convert.ToBoolean(dr["Enabled"]);
            this.ParentId = Null.SetNullInteger(dr["ParentId"]);
            this.Order = Null.SetNullInteger(dr["Order"]);
        }
    }
}
