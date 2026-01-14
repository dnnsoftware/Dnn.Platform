// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>WordCount Plugin Config.</summary>
    public class WordCountConfig
    {
        /// <summary>Initializes a new instance of the <see cref="WordCountConfig" /> class.</summary>
        public WordCountConfig()
        {
            this.ShowWordCount = true;
            this.ShowCharCount = false;
        }

        /// <summary>Gets or sets a value indicating whether [show char count].</summary>
        /// <value>
        ///   <see langword="true"/> if [show char count]; otherwise, <see langword="false"/>.
        /// </value>
        [XmlAttribute("showCharCount")]
        [Description("Whether or not you want to show the Word Count.")]
        public bool ShowCharCount { get; set; }

        /// <summary>Gets or sets a value indicating whether [show word count].</summary>
        /// <value>
        ///   <see langword="true"/> if [show word count]; otherwise, <see langword="false"/>.
        /// </value>
        [XmlAttribute("showWordCount")]
        [Description("Whether or not you want to show the Char Count")]
        public bool ShowWordCount { get; set; }
    }
}
