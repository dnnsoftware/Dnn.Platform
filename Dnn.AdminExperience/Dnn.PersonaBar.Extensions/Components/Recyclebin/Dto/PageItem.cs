// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Dto
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PageItem
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "childCount")]
        public int ChildrenCount { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "publishDate")]
        public string PublishDate { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "parentId")]
        public int ParentId { get; set; }

        [DataMember(Name = "level")]
        public int Level { get; set; }

        [DataMember(Name = "tabpath")]
        public string TabPath { get; set; }

        [DataMember(Name = "isspecial")]
        public bool IsSpecial { get; set; }

        [DataMember(Name = "useDefaultSkin")]
        public bool UseDefaultSkin { get; set; }

        [DataMember(Name = "lastModifiedOnDate")]
        public string LastModifiedOnDate { get; set; }

        [DataMember(Name = "friendlyLastModifiedOnDate")]
        public string FriendlyLastModifiedOnDate { get; set; }
    }

}
