
using System.ComponentModel;
using System.Xml.Serialization;

namespace DNNConnect.CKEditorProvider.Objects
{
    /// <summary>
    /// CodeMirror Plugin Options
    /// </summary>
    public class CodeMirror
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeMirror" /> class.
        /// </summary>
        public CodeMirror()
        {
            Theme = "default";
            LineNumbers = true;
            LineWrapping = true;
            MatchBrackets = true;
            AutoCloseTags = false;
            EnableSearchTools = true;
            EnableCodeFolding = true;
            EnableCodeFormatting = true;
            AutoFormatOnStart = false;
            AutoFormatOnUncomment = true;
            HighlightActiveLine = true;
            HighlightMatches = true;
            ShowTabs = false;
            ShowFormatButton = true;
            ShowCommentButton = true;
            ShowUncommentButton = true;
        }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        [XmlAttribute("theme")]
        [Description("Set this to the theme you wish to use (codemirror themes)")]
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [line numbers].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [line numbers]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("lineNumbers")]
        [Description("Whether or not you want to show line numbers")]
        public bool LineNumbers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [line wrapping].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [line wrapping]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("lineWrapping")]
        [Description("Whether or not you want to use line wrapping")]
        public bool LineWrapping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [match brackets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [match brackets]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("matchBrackets")]
        [Description("Whether or not you want to highlight matching braces")]
        public bool MatchBrackets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto close tags].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto close tags]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoCloseTags")]
        [Description("Whether or not you want tags to automatically close themselves")]
        public bool AutoCloseTags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable search tools].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable search tools]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("enableSearchTools")]
        [Description("Whether or not to enable search tools, CTRL+F (Find), CTRL+SHIFT+F (Replace), CTRL+SHIFT+R (Replace All), CTRL+G (Find Next), CTRL+SHIFT+G (Find Previous)")]
        public bool EnableSearchTools { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable code folding].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable code folding]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("enableCodeFolding")]
        [Description("Whether or not you wish to enable code folding (requires 'lineNumbers' to be set to 'true')")]
        public bool EnableCodeFolding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable code formatting].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable code formatting]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("enableCodeFormatting")]
        [Description("Whether or not to enable code formatting")]
        public bool EnableCodeFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto format on start].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto format on start]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoFormatOnStart")]
        [Description("Whether or not to automatically format code should be done every time the source view is opened")]
        public bool AutoFormatOnStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto format on uncomment].
        /// </summary>
        /// <value>
        /// <c>true</c> if [auto format on uncomment]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoFormatOnUncomment")]
        [Description("Whether or not to automatically format code which has just been uncommented")]
        public bool AutoFormatOnUncomment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [highlight active line].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [highlight active line]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("highlightActiveLine")]
        [Description("Whether or not to highlight the currently active line")]
        public bool HighlightActiveLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [highlight matches].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [highlight matches]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("highlightMatches")]
        [Description("Whether or not to highlight all matches of current word/selection")]
        public bool HighlightMatches { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show tabs].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show tabs]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showTabs")]
        [Description("Whether or not to display tabs")]
        public bool ShowTabs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show format button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show format button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showFormatButton")]
        [Description("Whether or not to show the format button on the toolbar")]
        public bool ShowFormatButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show comment button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show comment button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showCommentButton")]
        [Description("Whether or not to show the comment button on the toolbar")]
        public bool ShowCommentButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show uncomment button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show uncomment button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showUncommentButton")]
        [Description("Whether or not to show the uncomment button on the toolbar")]
        public bool ShowUncommentButton { get; set; }
    }
}