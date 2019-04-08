using System.ComponentModel;
using System.Xml.Serialization;

namespace DNNConnect.CKEditorProvider.Objects
{

    /// <summary>
    /// WordCount Plugin Config
    /// </summary>
    public class WordCountConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WordCountConfig" /> class.
        /// </summary>
        public WordCountConfig()
        {
            ShowWordCount = true;
            ShowCharCount = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show char count].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show char count]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showCharCount")]
        [Description("Whether or not you want to show the Word Count.")]
        public bool ShowCharCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show word count].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show word count]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showWordCount")]
        [Description("Whether or not you want to show the Char Count")]
        public bool ShowWordCount { get; set; }
    }
}