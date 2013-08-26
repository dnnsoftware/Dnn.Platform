#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Runtime.Serialization;

namespace DotNetNuke.Services.Subscriptions.Entities
{
    [DataContract]
    public class TemplateSettings
    {
        /// <summary>Header of a Subscription Digest notification</summary>
        [DataMember(Name = "digestHeader")]
        public string DigestHeader { get; set; }

        /// <summary>Footer of a Subscription Digest notification</summary>
        [DataMember(Name = "digestFooter")]
        public string DigestFooter { get; set; }

        /// <summary>Summary for an entry in a Digest notification</summary>
        [DataMember(Name = "digestSummary")]
        public string DigestSummary { get; set; }

        /// <summary>Digest subject line</summary>
        [DataMember(Name = "digestSubject")]
        public string DigestSubject { get; set; }

        /// <summary>Subject line for instant notifications</summary>
        [DataMember(Name = "instantSubject")]
        public string InstantSubject { get; set; }

        /// <summary>The header content included in an instant update notification</summary>
        [DataMember(Name = "instantHeader")]
        public string InstantHeader { get; set; }

        /// <summary>The footer included in an instant notification</summary>
        [DataMember(Name = "instantFooter")]
        public string InstantFooter { get; set; }

        /// <summary>Template for QueueItem notification header</summary>
        [DataMember(Name = "itemHeader")]
        public string ItemHeader { get; set; }

        /// <summary>Template for QueueItem subscription notification footer</summary>
        [DataMember(Name = "itemFooter")]
        public string ItemFooter { get; set; }

        /// <summary>Template for the contents of a digest QueueItem subscription notification message</summary>
        [DataMember(Name = "item")]
        public string Item { get; set; }

        /// <summary>Header for a list of tags associated with a QueueItem</summary>
        [DataMember(Name = "termsHeader")]
        public string TermsHeader { get; set; }

        /// <summary>Footer for a list of tags associated with a QueueItem</summary>
        [DataMember(Name = "termsFooter")]
        public string TermsFooter { get; set; }

        /// <summary>Template for an actual Taxonomy term associated with a QueueItem, sandwiched between Header and Footer</summary>
        [DataMember(Name = "term")]
        public string Term { get; set; }

    }
}