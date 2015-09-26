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
            /*this.CharLimit = "unlimited";
            this.WordLimit = "unlimited";*/
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
        /*
        /// <summary>
        /// Gets or sets the char limit.
        /// </summary>
        /// <value>
        /// The char limit.
        /// </value>
        [XmlAttribute("charLimit")]
        [Description("Option to set a maximum limit of Chars in Numbers or use 'unlimited'")]
        public string CharLimit { get; set; }

        /// <summary>
        /// Gets or sets the word limit.
        /// </summary>
        /// <value>
        /// The word limit.
        /// </value>
        [XmlAttribute("wordLimit")]
        [Description("Option to set a maximum limit of Words in Numbers or use 'unlimited'")]
        public string WordLimit { get; set; }*/
    }
}