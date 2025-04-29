// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects;

using System.ComponentModel;
using System.Xml.Serialization;

/// <summary>CodeMirror Plugin Options.</summary>
public class CodeMirror
{
    /// <summary>Initializes a new instance of the <see cref="CodeMirror" /> class.</summary>
    public CodeMirror()
    {
        this.Theme = "default";
        this.LineNumbers = true;
        this.LineWrapping = true;
        this.MatchBrackets = true;
        this.AutoCloseTags = false;
        this.EnableSearchTools = true;
        this.EnableCodeFolding = true;
        this.EnableCodeFormatting = true;
        this.AutoFormatOnStart = false;
        this.AutoFormatOnUncomment = true;
        this.HighlightActiveLine = true;
        this.HighlightMatches = true;
        this.ShowTabs = false;
        this.ShowFormatButton = true;
        this.ShowCommentButton = true;
        this.ShowUncommentButton = true;
    }

    /// <summary>Gets or sets the theme.</summary>
    /// <value>
    /// The theme.
    /// </value>
    [XmlAttribute("theme")]
    [Description("Set this to the theme you wish to use (codemirror themes)")]
    public string Theme { get; set; }

    /// <summary>Gets or sets a value indicating whether [line numbers].</summary>
    /// <value>
    ///   <see langword="true"/> if [line numbers]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("lineNumbers")]
    [Description("Whether or not you want to show line numbers")]
    public bool LineNumbers { get; set; }

    /// <summary>Gets or sets a value indicating whether [line wrapping].</summary>
    /// <value>
    ///   <see langword="true"/> if [line wrapping]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("lineWrapping")]
    [Description("Whether or not you want to use line wrapping")]
    public bool LineWrapping { get; set; }

    /// <summary>Gets or sets a value indicating whether [match brackets].</summary>
    /// <value>
    ///   <see langword="true"/> if [match brackets]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("matchBrackets")]
    [Description("Whether or not you want to highlight matching braces")]
    public bool MatchBrackets { get; set; }

    /// <summary>Gets or sets a value indicating whether [auto close tags].</summary>
    /// <value>
    ///   <see langword="true"/> if [auto close tags]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("autoCloseTags")]
    [Description("Whether or not you want tags to automatically close themselves")]
    public bool AutoCloseTags { get; set; }

    /// <summary>Gets or sets a value indicating whether [enable search tools].</summary>
    /// <value>
    ///   <see langword="true"/> if [enable search tools]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("enableSearchTools")]
    [Description("Whether or not to enable search tools, CTRL+F (Find), CTRL+SHIFT+F (Replace), CTRL+SHIFT+R (Replace All), CTRL+G (Find Next), CTRL+SHIFT+G (Find Previous)")]
    public bool EnableSearchTools { get; set; }

    /// <summary>Gets or sets a value indicating whether [enable code folding].</summary>
    /// <value>
    ///   <see langword="true"/> if [enable code folding]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("enableCodeFolding")]
    [Description("Whether or not you wish to enable code folding (requires 'lineNumbers' to be set to 'true')")]
    public bool EnableCodeFolding { get; set; }

    /// <summary>Gets or sets a value indicating whether [enable code formatting].</summary>
    /// <value>
    /// <see langword="true"/> if [enable code formatting]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("enableCodeFormatting")]
    [Description("Whether or not to enable code formatting")]
    public bool EnableCodeFormatting { get; set; }

    /// <summary>Gets or sets a value indicating whether [auto format on start].</summary>
    /// <value>
    ///   <see langword="true"/> if [auto format on start]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("autoFormatOnStart")]
    [Description("Whether or not to automatically format code should be done every time the source view is opened")]
    public bool AutoFormatOnStart { get; set; }

    /// <summary>Gets or sets a value indicating whether [auto format on uncomment].</summary>
    /// <value>
    /// <see langword="true"/> if [auto format on uncomment]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("autoFormatOnUncomment")]
    [Description("Whether or not to automatically format code which has just been uncommented")]
    public bool AutoFormatOnUncomment { get; set; }

    /// <summary>Gets or sets a value indicating whether [highlight active line].</summary>
    /// <value>
    ///   <see langword="true"/> if [highlight active line]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("highlightActiveLine")]
    [Description("Whether or not to highlight the currently active line")]
    public bool HighlightActiveLine { get; set; }

    /// <summary>Gets or sets a value indicating whether [highlight matches].</summary>
    /// <value>
    ///   <see langword="true"/> if [highlight matches]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("highlightMatches")]
    [Description("Whether or not to highlight all matches of current word/selection")]
    public bool HighlightMatches { get; set; }

    /// <summary>Gets or sets a value indicating whether [show tabs].</summary>
    /// <value>
    ///   <see langword="true"/> if [show tabs]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("showTabs")]
    [Description("Whether or not to display tabs")]
    public bool ShowTabs { get; set; }

    /// <summary>Gets or sets a value indicating whether [show format button].</summary>
    /// <value>
    ///   <see langword="true"/> if [show format button]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("showFormatButton")]
    [Description("Whether or not to show the format button on the toolbar")]
    public bool ShowFormatButton { get; set; }

    /// <summary>Gets or sets a value indicating whether [show comment button].</summary>
    /// <value>
    ///   <see langword="true"/> if [show comment button]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("showCommentButton")]
    [Description("Whether or not to show the comment button on the toolbar")]
    public bool ShowCommentButton { get; set; }

    /// <summary>Gets or sets a value indicating whether [show uncomment button].</summary>
    /// <value>
    ///   <see langword="true"/> if [show uncomment button]; otherwise, <see langword="false"/>.
    /// </value>
    [XmlAttribute("showUncommentButton")]
    [Description("Whether or not to show the uncomment button on the toolbar")]
    public bool ShowUncommentButton { get; set; }
}
