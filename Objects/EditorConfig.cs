using System.ComponentModel;
using System.Xml.Serialization;

using DNNConnect.CKEditorProvider.Constants;

namespace DNNConnect.CKEditorProvider.Objects
{

    /// <summary>
    /// Editor Configuration Settings
    /// </summary>
    public class EditorConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorConfig" /> class.
        /// </summary>
        public EditorConfig()
        {
            this.AllowedContent = "false";
            this.AutoGrow_BottomSpace = 0;
            this.AutoGrow_MaxHeight = 0;
            this.AutoGrow_MinHeight = 200;
            this.AutoGrow_OnStartup = false;
            this.AutoParagraph = true;
            this.AutoSave_Delay = 25;
            this.AutoUpdateElement = true;
            this.BaseFloatZIndex = 10000;
            this.BasicEntities = true;
            this.BlockedKeystrokes = "[ CKEDITOR.CTRL + 66, CKEDITOR.CTRL + 73, CKEDITOR.CTRL + 85 ]";
            this.BrowserContextMenuOnCtrl = true;
            this.Clipboard_DefaultContentType = "html";
            this.CodeMirror = new CodeMirror();
            this.ColorButton_Colors = "00923E,F8C100,28166F";
            this.ColorButton_EnableMore = true;
            this.DataIndentationChars = "\t";
            this.DefaultLanguage = "en";
            this.DefaultLinkType = LinkType.url;
            this.Dialog_BackgroundCoverColor = "white";
            this.Dialog_BackgroundCoverOpacity = 0.5;
            this.Dialog_ButtonsOrder = "OS";
            this.Dialog_MagnetDistance = 20;
            this.Dialog_StartupFocusTab = false;
            this.DisableNativeSpellChecker = true;
            this.DisableNativeTableHandles = true;
            this.DisableObjectResizing = false;
            this.DisableReadonlyStyling = false;
            this.Div_WrapTable = false;
            this.DocType = "<!DOCTYPE html>";
            this.EnableTabKeyTools = true;
            this.EnterMode = EnterModus.P;
            this.Entities = true;
            this.Entities_additional = "#39";
            this.Entities_Greek = false;
            this.Entities_Latin = false;
            this.Entities_ProcessNumerical = false;
            this.ExtraPlugins = "dnnpages,wordcount,notification";
            this.FileBrowserWindowFeatures = "location=no,menubar=no,toolbar=no,dependent=yes,minimizable=no,modal=yes,alwaysRaised=yes,resizable=yes,scrollbars=yes";
            this.FileBrowserWindowHeight = "70%";
            this.FileBrowserWindowWidth = "80%";
            this.FillEmptyBlocks = true;
            this.FlashAddEmbedTag = false;
            this.FlashConvertOnEdit = false;
            this.FlashEmbedTagOnly = false;
            this.FloatSpaceDockedOffsetX = 0;
            this.FloatSpaceDockedOffsetY = 0;
            this.FloatSpacePinnedOffsetX = 0;
            this.FloatSpacePinnedOffsetY = 0;
            this.FontSize_Sizes = "12px;2.3em;130%;larger;x-small";
            this.Font_Names = "Arial;Times New Roman;Verdana";
            this.ForceEnterMode = false;
            this.ForcePasteAsPlainText = false;
            this.ForceSimpleAmpersand = false;
            this.Format_Tags = "p;h1;h2;h3;h4;h5;h6;pre;address;div";
            this.FullPage = false;
            this.Height = "200";
            this.HtmlEncodeOutput = false;
            this.IgnoreEmptyParagraph = true;
            this.Image_PreviewText = "Lorem ipsum dolor...";
            this.Image_RemoveLinkByEmptyURL = true;
            this.IndentOffset = 40;
            this.IndentUnit = "px";
            this.LinkShowAdvancedTab = true;
            this.LinkShowTargetTab = true;
            this.Magicline_Color = "#FF0000";
            this.Magicline_HoldDistance = "0.5";
            this.Magicline_PutEverywhere = false;
            this.Magicline_TriggerOffset = 30;
            this.Menu_SubMenuDelay = 400;
            this.Menu_Groups = "clipboard,tablecell,tablecellproperties,tablerow,tablecolumn,table,anchor,link,image,flash,checkbox,radio,textfield,hiddenfield,imagebutton,button,select,textarea,div";
            this.PasteFromWordCleanupFile = string.Empty;
            this.PasteFromWordNumberedHeadingToList = false;
            this.PasteFromWordPromptCleanup = false;
            this.PasteFromWordRemoveFontStyles = true;
            this.PasteFromWordRemoveStyles = true;
            this.ProtectedSource = @"[( /<i class[\s\S]*?>[\s\S]*?<\/i>/gi ),( /<span class[\s\S]*?>[\s\S]*?<\/span>/gi ),( /<em class[\s\S]*?>[\s\S]*?<\/em>/gi ),( /<button class[\s\S]*?>[\s\S]*?<\/button>/gi )]";
            this.ReadOnly = false;
            this.RemoveFormatAttributes = "class,style,lang,width,height,align,hspace,valign";
            this.RemoveFormatTags = "b,big,code,del,dfn,em,font,i,ins,kbd,q,samp,small,span,strike,strong,sub,sup,tt,u,var";
            this.Resize_Dir = "both";
            this.Resize_Enabled = true;
            this.Resize_MaxHeight = 600;
            this.Resize_MaxWidth = 3000;
            this.Resize_MinHeight = 250;
            this.Resize_MinWidth = 750;
            this.ShiftEnterMode = EnterModus.BR;
            this.Skin = "moono";
            this.Smiley_columns = 8;
            this.SourceAreaTabSize = 20;
            this.StartupFocus = false;
            this.StartupMode = "wysiwyg";
            this.StartupOutlineBlocks = false;
            this.StartupShowBorders = true;
            this.TabIndex = 0;
            this.TabSpaces = 0;
            this.Templates = "default";
            this.Templates_ReplaceContent = true;
            this.ToolbarCanCollapse = false;
            this.ToolbarGroupCycling = true;
            this.ToolbarLocation = ToolBarLocation.Top;
            this.ToolbarStartupExpanded = true;
            this.UndoStackSize = 20;
            this.UseComputedState = true;
            this.Width = "99%";
            this.WordCount = new WordCountConfig();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allowed content].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allowed content]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("allowedContent")]
        [Description("Allowed content rules. This setting is used when instantiating CKEDITOR.editor.filter.")]
        public string AllowedContent { get; set; }

        /// <summary>
        /// Gets or sets the auto grow_ bottom space.
        /// </summary>
        /// <value>
        /// The auto grow_ bottom space.
        /// </value>
        [XmlAttribute("autoGrow_bottomSpace")]
        [Description("Extra height in pixel to leave between the bottom boundary of content with document size when auto resizing.")]
        public int AutoGrow_BottomSpace { get; set; }

        /// <summary>
        /// Gets or sets the height of the auto grow_ max.
        /// </summary>
        /// <value>
        /// The height of the auto grow_ max.
        /// </value>
        [XmlAttribute("autoGrow_maxHeight")]
        [Description("The maximum height that the editor can reach using the AutoGrow feature. Zero means unlimited.")]
        public int AutoGrow_MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the height of the auto grow_ min.
        /// </summary>
        /// <value>
        /// The height of the auto grow_ min.
        /// </value>
        [XmlAttribute("autoGrow_minHeight")]
        [Description("The minimum height that the editor can reach using the AutoGrow feature.")]
        public int AutoGrow_MinHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto grow_ on startup].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto grow_ on startup]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoGrow_onStartup")]
        [Description("Whether to have the auto grow happen on editor creation.")]
        public bool AutoGrow_OnStartup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto paragraph].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto paragraph]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoParagraph")]
        [Description("Whether automatically create wrapping blocks around inline contents inside document body, this helps to ensure the integrality of the block enter mode.")]
        public bool AutoParagraph { get; set; }

        /// <summary>
        /// Gets or sets the auto save delay.
        /// </summary>
        /// <value>
        /// The auto save delay.
        /// </value>
        [XmlAttribute("autosave_delay")]
        [Description("Auto-save time delay (in seconds)")]
        public int AutoSave_Delay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto update element].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto update element]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoUpdateElement")]
        [Description("Whether the replaced element is to be updated automatically when posting the form containing the editor.")]
        public bool AutoUpdateElement { get; set; }

        /// <summary>
        /// Gets or sets the index of the base float Z.
        /// </summary>
        /// <value>
        /// The index of the base float Z.
        /// </value>
        [XmlAttribute("baseFloatZIndex")]
        [Description("The base Z-index for floating dialog windows and popups.")]
        public int BaseFloatZIndex { get; set; }

        /// <summary>
        /// Gets or sets the base HREF.
        /// </summary>
        /// <value>
        /// The base HREF.
        /// </value>
        [XmlAttribute("baseHref")]
        [Description("The base href URL used to resolve relative and absolute URLs in the editor content.")]
        public string BaseHref { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [basic entities].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [basic entities]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("basicEntities")]
        [Description("Whether to escape basic HTML entities in the document, including: nbsp, gt, lt, amp")]
        public bool BasicEntities { get; set; }

        /// <summary>
        /// Gets or sets the blocked keystrokes.
        /// </summary>
        /// <value>
        /// The blocked keystrokes.
        /// </value>
        [XmlAttribute("blockedKeystrokes")]
        [Description("The keystrokes that are blocked by default as the browser implementation is buggy. These default keystrokes are handled by the editor.")]
        public string BlockedKeystrokes { get; set; }

        /// <summary>
        /// Gets or sets the body class.
        /// </summary>
        /// <value>
        /// The body class.
        /// </value>
        [XmlAttribute("bodyClass")]
        [Description("Sets the class attribute to be used on the body element of the editing area. This can be useful when you intend to reuse the original CSS file you are using on your live website and want to assign the editor the same class as the section that will include the contents. In this way class-specific CSS rules will be enabled.")]
        public string BodyClass { get; set; }

        /// <summary>
        /// Gets or sets the body id.
        /// </summary>
        /// <value>
        /// The body id.
        /// </value>
        [XmlAttribute("bodyId")]
        [Description("Sets the id attribute to be used on the body element of the editing area. This can be useful when you intend to reuse the original CSS file you are using on your live website and want to assign the editor the same ID as the section that will include the contents. In this way ID-specific CSS rules will be enabled.")]
        public string BodyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [browser context menu on CTRL].
        /// </summary>
        /// <value>
        /// <c>true</c> if [browser context menu on CTRL]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("browserContextMenuOnCtrl")]
        [Description("Whether to show the browser native context menu when the Ctrl or Meta (Mac) key is pressed on opening the context menu with the right mouse button click or the Menu key.")]
        public bool BrowserContextMenuOnCtrl { get; set; }

        /// <summary>
        /// Gets or sets the type of the clipboard_ default content.
        /// </summary>
        /// <value>
        /// The type of the clipboard_ default content.
        /// </value>
        [XmlAttribute("clipboard_defaultContentType")]
        [Description("The default content type is used when pasted data cannot be clearly recognized as HTML or text.")]
        public string Clipboard_DefaultContentType { get; set; }

        /// <summary>
        /// Gets or sets the code mirror.
        /// </summary>
        /// <value>
        /// The code mirror.
        /// </value>
        public CodeMirror CodeMirror { get; set; }

        /// <summary>
        /// Gets or sets the color button_ back style.
        /// </summary>
        /// <value>
        /// The color button_ back style.
        /// </value>
        [XmlAttribute("colorButton_backStyle")]
        [Description("Stores the style definition that applies the text background color.")]
        public string ColorButton_BackStyle { get; set; }

        /// <summary>
        /// Gets or sets the color button_ colors.
        /// </summary>
        /// <value>
        /// The color button_ colors.
        /// </value>
        [XmlAttribute("colorButton_colors")]
        [Description("Defines the colors to be displayed in the color selectors. This is a string containing hexadecimal notation for HTML colors, without the '#' prefix.")]
        public string ColorButton_Colors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [color button_ enable more].
        /// </summary>
        /// <value>
        /// <c>true</c> if [color button_ enable more]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("colorButton_enableMore")]
        [Description("Whether to enable the More Colors button in the color selectors.")]
        public bool ColorButton_EnableMore { get; set; }

        /// <summary>
        /// Gets or sets the color button_ fore style.
        /// </summary>
        /// <value>
        /// The color button_ fore style.
        /// </value>
        [XmlAttribute("colorButton_foreStyle")]
        [Description("Stores the style definition that applies the text foreground color.")]
        public string ColorButton_ForeStyle { get; set; }

        /// <summary>
        /// Gets or sets the contents CSS.
        /// </summary>
        /// <value>
        /// The contents CSS.
        /// </value>
        [XmlAttribute("contentsCss")]
        [Description("The CSS file(s) to be used to apply style to the contents. It should reflect the CSS used in the final pages where the contents are to be used.")]
        public string ContentsCss { get; set; }

        /// <summary>
        /// Gets or sets the contents lang direction.
        /// </summary>
        /// <value>
        /// The contents lang direction.
        /// </value>
        [XmlAttribute("contentsLangDirection")]
        [Description("The writting direction of the language used to write the editor contents.")]
        public LanguageDirection ContentsLangDirection { get; set; }

        /// <summary>
        /// Gets or sets the contents language.
        /// </summary>
        /// <value>
        /// The contents language.
        /// </value>
        [XmlAttribute("contentsLanguage")]
        [Description("Language code of the writting language which is used to author the editor contents.")]
        public string ContentsLanguage { get; set; }

        /// <summary>
        /// Gets or sets the core styles_ bold.
        /// </summary>
        /// <value>
        /// The core styles_ bold.
        /// </value>
        [XmlAttribute("coreStyles_bold")]
        [Description("The style definition that applies the bold style to the text.")]
        public string CoreStyles_Bold { get; set; }

        /// <summary>
        /// Gets or sets the core styles_ italic.
        /// </summary>
        /// <value>
        /// The core styles_ italic.
        /// </value>
        [XmlAttribute("coreStyles_italic")]
        [Description("The style definition that applies the italics style to the text.")]
        public string CoreStyles_Italic { get; set; }

        /// <summary>
        /// Gets or sets the core styles_ strike.
        /// </summary>
        /// <value>
        /// The core styles_ strike.
        /// </value>
        [XmlAttribute("coreStyles_strike")]
        [Description("The style definition that applies the strike-through style to the text.")]
        public string CoreStyles_Strike { get; set; }

        /// <summary>
        /// Gets or sets the core styles_ subscript.
        /// </summary>
        /// <value>
        /// The core styles_ subscript.
        /// </value>
        [XmlAttribute("coreStyles_subscript")]
        [Description("The style definition that applies the subscript style to the text.")]
        public string CoreStyles_Subscript { get; set; }

        /// <summary>
        /// Gets or sets the core styles_ superscript.
        /// </summary>
        /// <value>
        /// The core styles_ superscript.
        /// </value>
        [XmlAttribute("coreStyles_superscript")]
        [Description("The style definition that applies the superscript style to the text.")]
        public string CoreStyles_Superscript { get; set; }

        /// <summary>
        /// Gets or sets the core styles_ underline.
        /// </summary>
        /// <value>
        /// The core styles_ underline.
        /// </value>
        [XmlAttribute("coreStyles_underline")]
        [Description("The style definition that applies the underline style to the text.")]
        public string CoreStyles_Underline { get; set; }

        /// <summary>
        /// Gets or sets the custom config.
        /// </summary>
        /// <value>
        /// The custom config.
        /// </value>
        [XmlAttribute("customConfig")]
        [Description("The URL path for the custom configuration file to be loaded. If not overloaded with inline configuration, it defaults to the config.js file present in the root of the CKEditor installation directory.")]
        public string CustomConfig { get; set; }

        /// <summary>
        /// Gets or sets the data indentation chars.
        /// </summary>
        /// <value>
        /// The data indentation chars.
        /// </value>
        [XmlAttribute("dataIndentationChars")]
        [Description("The characters to be used for indenting the HTML produced by the editor.")]
        public string DataIndentationChars { get; set; }

        /// <summary>
        /// Gets or sets the default language.
        /// </summary>
        /// <value>
        /// The default language.
        /// </value>
        [XmlAttribute("defaultLanguage")]
        [Description("The language to be used if the language setting is left empty and it is not possible to localize the editor to the user language.")]
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// Gets or sets the default link type
        /// </summary>
        /// <value>
        /// The enter default link type
        /// </value>
        [XmlAttribute("defaultLinkType")]
        [Description("Sets the Default Link Type for the Link Dialog ")]
        public LinkType DefaultLinkType { get; set; }

        /// <summary>
        /// Gets or sets the developer tools_ styles.
        /// </summary>
        /// <value>
        /// The developer tools_ styles.
        /// </value>
        [XmlAttribute("devtools_styles")]
        [Description("A setting that stores CSS rules to be injected into the page with styles to be applied to the tooltip element.")]
        public string Devtools_Styles { get; set; }

        /// <summary>
        /// Gets or sets the color of the dialog_ background cover.
        /// </summary>
        /// <value>
        /// The color of the dialog_ background cover.
        /// </value>
        [XmlAttribute("dialog_backgroundCoverColor")]
        [Description("The color of the dialog background cover. It should be a valid CSS color string.")]
        public string Dialog_BackgroundCoverColor { get; set; }

        /// <summary>
        /// Gets or sets the dialog_ background cover opacity.
        /// </summary>
        /// <value>
        /// The dialog_ background cover opacity.
        /// </value>
        [XmlAttribute("dialog_backgroundCoverOpacity")]
        [Description("The opacity of the dialog background cover. It should be a number within the range 0.0 to 1.0.")]
        public double Dialog_BackgroundCoverOpacity { get; set; }

        /// <summary>
        /// Gets or sets the dialog_ buttons order.
        /// </summary>
        /// <value>
        /// The dialog_ buttons order.
        /// </value>
        [XmlAttribute("dialog_buttonsOrder")]
        [Description("The guideline to follow when generating the dialog buttons.")]
        public string Dialog_ButtonsOrder { get; set; }

        /// <summary>
        /// Gets or sets the dialog_ magnet distance.
        /// </summary>
        /// <value>
        /// The dialog_ magnet distance.
        /// </value>
        [XmlAttribute("dialog_magnetDistance")]
        [Description("The distance of magnetic borders used in moving and resizing dialogs, measured in pixels.")]
        public int Dialog_MagnetDistance { get; set; }

        /// <summary>
        /// Gets or sets the dialog_ no confirm cancel.
        /// </summary>
        /// <value>
        /// The dialog_ no confirm cancel.
        /// </value>
        [XmlAttribute("dialog_noConfirmCancel")]
        [Description("Tells if user should not be asked to confirm close, if any dialog field was modified. By default it is set to false meaning that the confirmation dialog will be shown.")]
        public int Dialog_NoConfirmCancel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [dialog_ startup focus tab].
        /// </summary>
        /// <value>
        /// <c>true</c> if [dialog_ startup focus tab]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("dialog_startupFocusTab")]
        [Description("If the dialog has more than one tab, put focus into the first tab as soon as dialog is opened.")]
        public bool Dialog_StartupFocusTab { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable native spell checker].
        /// </summary>
        /// <value>
        /// <c>true</c> if [disable native spell checker]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("disableNativeSpellChecker")]
        [Description("Disables the built-in words spell checker if browser provides one.")]
        public bool DisableNativeSpellChecker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable native table handles].
        /// </summary>
        /// <value>
        /// <c>true</c> if [disable native table handles]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("disableNativeTableHandles")]
        [Description("Disables the table tools offered natively by the browser (currently Firefox only) to make quick table editing operations, like adding or deleting rows and columns.")]
        public bool DisableNativeTableHandles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable object resizing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [disable object resizing]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("disableObjectResizing")]
        [Description("Disables the ability of resize objects (image and tables) in the editing area.")]
        public bool DisableObjectResizing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable readonly styling].
        /// </summary>
        /// <value>
        /// <c>true</c> if [disable readonly styling]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("disableReadonlyStyling")]
        [Description("Disables inline styling on read-only elements.")]
        public bool DisableReadonlyStyling { get; set; }

        /// <summary>
        /// Gets or sets the content of the dis allowed.
        /// </summary>
        /// <value>
        /// The content of the dis allowed.
        /// </value>
        [XmlAttribute("disallowedContent")]
        [Description("Disallowed content rules. They have precedence over allowed content rules. Read more in the Disallowed Content guide")]
        public string DisAllowedContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [div_ wrap table].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [div_ wrap table]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("div_wrapTable")]
        [Description("Whether to wrap the whole table instead of indivisual cells when created DIV in table cell.")]
        public bool Div_WrapTable { get; set; }

        /// <summary>
        /// Gets or sets the type of the doc.
        /// </summary>
        /// <value>
        /// The type of the doc.
        /// </value>
        [XmlAttribute("docType")]
        [Description("Sets the DOCTYPE to be used when loading the editor content as HTML.")]
        public string DocType { get; set; }

        /// <summary>
        /// Gets or sets the email protection.
        /// </summary>
        /// <value>
        /// The email protection.
        /// </value>
        [XmlAttribute("emailProtection")]
        [Description("The e-mail address anti-spam protection option. The protection will be applied when creating or modifying e-mail links through the editor interface.")]
        public string EmailProtection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable tab key tools].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable tab key tools]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("enableTabKeyTools")]
        [Description("Allow context-sensitive tab key behaviors.")]
        public bool EnableTabKeyTools { get; set; }

        /// <summary>
        /// Gets or sets the enter mode.
        /// </summary>
        /// <value>
        /// The enter mode.
        /// </value>
        [XmlAttribute("enterMode")]
        [Description("Sets the behavior of the Enter key. It also determines other behavior rules of the editor, like whether the br element is to be used as a paragraph separator when indenting text. ")]
        public EnterModus EnterMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EditorConfig" /> is entities.
        /// </summary>
        /// <value>
        ///   <c>true</c> if entities; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("entities")]
        [Description("Whether to use HTML entities in the output.")]
        public bool Entities { get; set; }

        /// <summary>
        /// Gets or sets the entities_additional.
        /// </summary>
        /// <value>
        /// The entities_additional.
        /// </value>
        [XmlAttribute("entities_additional")]
        [Description("A comma separated list of additional entities to be used. Entity names or numbers must be used in a form that excludes the '&amp;' prefix and the ';' ending.")]
        public string Entities_additional { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [entities_ greek].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [entities_ greek]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("entities_greek")]
        [Description("Whether to convert some symbols, mathematical symbols, and Greek letters to HTML entities. This may be more relevant for users typing text written in Greek. The list of entities can be found in the [W3C HTML 4.01 Specification, section 24.3.1(http://www.w3.org/TR/html4/sgml/entities.html#h-24.3.1).")]
        public bool Entities_Greek { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [entities_ latin].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [entities_ latin]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("entities_latin")]
        [Description("Whether to convert some Latin characters (Latin alphabet No. 1, ISO 8859-1) to HTML entities. The list of entities can be found in the W3C HTML 4.01 Specification, section 24.2.1.")]
        public bool Entities_Latin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [entities_ process numerical].
        /// </summary>
        /// <value>
        /// <c>true</c> if [entities_ process numerical]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("entities_processNumerical")]
        [Description("Whether to convert all remaining characters not included in the ASCII character table to their relative decimal numeric representation of HTML entity. When set to force, it will convert all entities into this format.")]
        public bool Entities_ProcessNumerical { get; set; }

        /// <summary>
        /// Gets or sets the content of the extra allowed.
        /// </summary>
        /// <value>
        /// The content of the extra allowed.
        /// </value>
        [XmlAttribute("extraAllowedContent")]
        [Description("This option makes it possible to set additional allowed content rules for CKEDITOR.editor.filter.")]
        public string ExtraAllowedContent { get; set; }

        /// <summary>
        /// Gets or sets the extra plugins.
        /// </summary>
        /// <value>
        /// The extra plugins.
        /// </value>
        [XmlAttribute("extraPlugins")]
        [Description("A list of additional plugins to be loaded. This setting makes it easier to add new plugins without having to touch plugins setting.")]
        public string ExtraPlugins { get; set; }

        /// <summary>
        /// Gets or sets the file browser browse URL.
        /// </summary>
        /// <value>
        /// The file browser browse URL.
        /// </value>
        [XmlAttribute("filebrowserBrowseUrl")]
        [Description("The location of an external file browser that should be launched when the Browse Server button is pressed. If configured, the Browse Server button will appear in the Link, Image, and Flash dialog windows")]
        public string FileBrowserBrowseUrl { get; set; }

        /// <summary>
        /// Gets or sets the file browser flash browse URL.
        /// </summary>
        /// <value>
        /// The file browser flash browse URL.
        /// </value>
        [XmlAttribute("filebrowserFlashBrowseUrl")]
        [Description("The location of an external file browser that should be launched when the Browse Server button is pressed in the Flash dialog window.")]
        public string FileBrowserFlashBrowseUrl { get; set; }

        /// <summary>
        /// Gets or sets the file browser flash upload URL.
        /// </summary>
        /// <value>
        /// The file browser flash upload URL.
        /// </value>
        [XmlAttribute("filebrowserFlashUploadUrl")]
        [Description("The location of the script that handles file uploads in the Flash dialog window.")]
        public string FileBrowserFlashUploadUrl { get; set; }

        /// <summary>
        /// Gets or sets the file browser image browse URL.
        /// </summary>
        /// <value>
        /// The file browser image browse URL.
        /// </value>
        [XmlAttribute("filebrowserImageBrowseUrl")]
        [Description("The location of an external file browser that should be launched when the Browse Server button is pressed in the Image dialog window.")]
        public string FileBrowserImageBrowseUrl { get; set; }

        /// <summary>
        /// Gets or sets the file browser image browse link URL.
        /// </summary>
        /// <value>
        /// The file browser image browse link URL.
        /// </value>
        [XmlAttribute("filebrowserImageBrowseLinkUrl")]
        [Description("The location of an external file browser that should be launched when the Browse Server button is pressed in the Image dialog window.")]
        public string FilebrowserImageBrowseLinkUrl { get; set; }

        /// <summary>
        /// Gets or sets the file browser image upload URL.
        /// </summary>
        /// <value>
        /// The file browser image upload URL.
        /// </value>
        [XmlAttribute("filebrowserImageUploadUrl")]
        [Description("The location of the script that handles file uploads in the Image dialog window.")]
        public string FileBrowserImageUploadUrl { get; set; }

        /// <summary>
        /// Gets or sets the file browser upload URL.
        /// </summary>
        /// <value>
        /// The file browser upload URL.
        /// </value>
        [XmlAttribute("filebrowserUploadUrl")]
        [Description("The location of the script that handles file uploads. If set, the Upload tab will appear in the Link, Image, and Flash dialog windows.")]
        public string FileBrowserUploadUrl { get; set; }

        /// <summary>
        /// Gets or sets the file browser window features.
        /// </summary>
        /// <value>
        /// The file browser window features.
        /// </value>
        [XmlAttribute("filebrowserWindowFeatures")]
        [Description("The features to use in the file browser popup window.")]
        public string FileBrowserWindowFeatures { get; set; }

        /// <summary>
        /// Gets or sets the height of the file browser window.
        /// </summary>
        /// <value>
        /// The height of the file browser window.
        /// </value>
        [XmlAttribute("filebrowserWindowHeight")]
        [Description("The height of the file browser popup window. It can be a number denoting a value in pixels or a percent string.")]
        public string FileBrowserWindowHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the file browser window.
        /// </summary>
        /// <value>
        /// The width of the file browser window.
        /// </value>
        [XmlAttribute("filebrowserWindowWidth")]
        [Description("The width of the file browser popup window. It can be a number denoting a value in pixels or a percent string.")]
        public string FileBrowserWindowWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [fill empty blocks].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fill empty blocks]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("fillEmptyBlocks")]
        [Description("Whether a filler text (non-breaking space entity) will be inserted into empty block elements in HTML output, this is used to render block elements properly with line-height When a function is instead specified, it'll be passed a CKEDITOR.htmlParser.element to decide whether adding the filler text by expecting a boolean return value.")]
        public bool FillEmptyBlocks { get; set; }

        /// <summary>
        /// Gets or sets the find_ highlight.
        /// </summary>
        /// <value>
        /// The find_ highlight.
        /// </value>
        [XmlAttribute("find_highlight")]
        [Description("Defines the style to be used to highlight results with the find dialog.")]
        public string Find_Highlight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [flash add embed tag].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [flash add embed tag]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("flashAddEmbedTag")]
        [Description("Add <embed> tag as alternative: <object><embed></embed></object>.")]
        public bool FlashAddEmbedTag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [flash convert on edit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [flash convert on edit]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("flashConvertOnEdit")]
        [Description("Use flashEmbedTagOnly and flashAddEmbedTag values on edit.")]
        public bool FlashConvertOnEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [flash embed tag only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [flash embed tag only]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("flashEmbedTagOnly")]
        [Description("Save as <embed> tag only. This tag is unrecommended.")]
        public bool FlashEmbedTagOnly { get; set; }

        /// <summary>
        /// Gets or sets the float space docked offset X.
        /// </summary>
        /// <value>
        /// The float space docked offset X.
        /// </value>
        [XmlAttribute("floatSpaceDockedOffsetX")]
        [Description("Along with floatSpaceDockedOffsetY it defines the amount of offset (in pixels) between float space and the editable left/right boundaries when space element is docked at either side of the editable.")]
        public int FloatSpaceDockedOffsetX { get; set; }

        /// <summary>
        /// Gets or sets the float space docked offset Y.
        /// </summary>
        /// <value>
        /// The float space docked offset Y.
        /// </value>
        [XmlAttribute("floatSpaceDockedOffsetY")]
        [Description("Along with floatSpaceDockedOffsetX it defines the amount of offset (in pixels) between float space and the editable top/bottom boundaries when space element is docked at either side of the editable.")]
        public int FloatSpaceDockedOffsetY { get; set; }

        /// <summary>
        /// Gets or sets the float space pinned offset X.
        /// </summary>
        /// <value>
        /// The float space pinned offset X.
        /// </value>
        [XmlAttribute("floatSpacePinnedOffsetX")]
        [Description("Along with floatSpacePinnedOffsetY it defines the amount of offset (in pixels) between float space and the view port boundaries when space element is pinned.")]
        public int FloatSpacePinnedOffsetX { get; set; }

        /// <summary>
        /// Gets or sets the float space pinned offset Y.
        /// </summary>
        /// <value>
        /// The float space pinned offset Y.
        /// </value>
        [XmlAttribute("floatSpacePinnedOffsetY")]
        [Description("Along with floatSpacePinnedOffsetX it defines the amount of offset (in pixels) between float space and the view port boundaries when space element is pinned.")]
        public int FloatSpacePinnedOffsetY { get; set; }

        /// <summary>
        /// Gets or sets the font size_ default label.
        /// </summary>
        /// <value>
        /// The font size_ default label.
        /// </value>
        [XmlAttribute("fontSize_defaultLabel")]
        [Description("")]
        public string FontSize_DefaultLabel { get; set; }

        /// <summary>
        /// Gets or sets the font size_ sizes.
        /// </summary>
        /// <value>
        /// The font size_ sizes.
        /// </value>
        [XmlAttribute("fontSize_sizes")]
        [Description("The list of fonts size to be displayed in the Font Size combo in the toolbar. Entries are separated by semi-colons (';').")]
        public string FontSize_Sizes { get; set; }

        /// <summary>
        /// Gets or sets the font size_ style.
        /// </summary>
        /// <value>
        /// The font size_ style.
        /// </value>
        [XmlAttribute("fontSize_style")]
        [Description("The style definition to be used to apply the font size in the text.")]
        public string FontSize_Style { get; set; }

        /// <summary>
        /// Gets or sets the font_ default label.
        /// </summary>
        /// <value>
        /// The font_ default label.
        /// </value>
        [XmlAttribute("font_defaultLabel")]
        [Description("The text to be displayed in the Font combo is none of the available values matches the current cursor position or text selection.")]
        public string Font_DefaultLabel { get; set; }

        /// <summary>
        /// Gets or sets the font_ names.
        /// </summary>
        /// <value>
        /// The font_ names.
        /// </value>
        [XmlAttribute("font_names")]
        [Description("The list of fonts names to be displayed in the Font combo in the toolbar. Entries are separated by semi-colons (';'), while it's possible to have more than one font for each entry, in the HTML way (separated by comma).")]
        public string Font_Names { get; set; }

        /// <summary>
        /// Gets or sets the font_ style.
        /// </summary>
        /// <value>
        /// The font_ style.
        /// </value>
        [XmlAttribute("font_style")]
        [Description("The style definition to be used to apply the font in the text.")]
        public string Font_Style { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force enter mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [force enter mode]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("forceEnterMode")]
        [Description("Force the use of enterMode as line break regardless of the context. If, for example, enterMode is set to CKEDITOR.ENTER_P, pressing the Enter key inside a <div> element will create a new paragraph with <p> instead of a <div>")]
        public bool ForceEnterMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force paste as plain text].
        /// </summary>
        /// <value>
        /// <c>true</c> if [force paste as plain text]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("forcePasteAsPlainText")]
        [Description("Whether to force all pasting operations to insert on plain text into the editor, loosing any formatting information possibly available in the source text.")]
        public bool ForcePasteAsPlainText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force simple ampersand].
        /// </summary>
        /// <value>
        /// <c>true</c> if [force simple ampersand]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("forceSimpleAmpersand")]
        [Description("Whether to force using '&' instead of '&amp;' in elements attributes values, it's not recommended to change this setting for compliance with the W3C XHTML 1.0 standards (C.12, XHTML 1.0).")]
        public bool ForceSimpleAmpersand { get; set; }

        /// <summary>
        /// Gets or sets the format_ address.
        /// </summary>
        /// <value>
        /// The format_ address.
        /// </value>
        [XmlAttribute("format_address")]
        [Description("The style definition to be used to apply the 'Address' format.")]
        public string Format_Address { get; set; }

        /// <summary>
        /// Gets or sets the format_div.
        /// </summary>
        /// <value>
        /// The format_div.
        /// </value>
        [XmlAttribute("format_Div")]
        [Description("The style definition to be used to apply the 'Normal (DIV)' format.")]
        public string Format_div { get; set; }

        /// <summary>
        /// Gets or sets the format_ h1.
        /// </summary>
        /// <value>
        /// The format_ h1.
        /// </value>
        [XmlAttribute("format_h1")]
        [Description("The style definition to be used to apply the 'Heading 1' format.")]
        public string Format_H1 { get; set; }

        /// <summary>
        /// Gets or sets the format_ h2.
        /// </summary>
        /// <value>
        /// The format_ h2.
        /// </value>
        [XmlAttribute("format_h2")]
        [Description("The style definition to be used to apply the 'Heading 2' format.")]
        public string Format_H2 { get; set; }

        /// <summary>
        /// Gets or sets the format_ h3.
        /// </summary>
        /// <value>
        /// The format_ h3.
        /// </value>
        [XmlAttribute("format_h3")]
        [Description("The style definition to be used to apply the 'Heading 3' format.")]
        public string Format_H3 { get; set; }

        /// <summary>
        /// Gets or sets the format_ h4.
        /// </summary>
        /// <value>
        /// The format_ h4.
        /// </value>
        [XmlAttribute("format_h4")]
        [Description("The style definition to be used to apply the 'Heading 4' format.")]
        public string Format_H4 { get; set; }

        /// <summary>
        /// Gets or sets the format_ h5.
        /// </summary>
        /// <value>
        /// The format_ h5.
        /// </value>
        [XmlAttribute("format_h5")]
        [Description("The style definition to be used to apply the 'Heading 5' format.")]
        public string Format_H5 { get; set; }

        /// <summary>
        /// Gets or sets the format_ h6.
        /// </summary>
        /// <value>
        /// The format_ h6.
        /// </value>
        [XmlAttribute("format_h6")]
        [Description("The style definition to be used to apply the 'Heading 6' format.")]
        public string Format_H6 { get; set; }

        /// <summary>
        /// Gets or sets the format_p.
        /// </summary>
        /// <value>
        /// The format_p.
        /// </value>
        [XmlAttribute("format_p")]
        [Description("The style definition to be used to apply the 'Normal' format.")]
        public string Format_p { get; set; }

        /// <summary>
        /// Gets or sets the format_pre.
        /// </summary>
        /// <value>
        /// The format_pre.
        /// </value>
        [XmlAttribute("format_pre")]
        [Description("The style definition to be used to apply the 'Formatted' format.")]
        public string Format_pre { get; set; }

        /// <summary>
        /// Gets or sets the format_ tags.
        /// </summary>
        /// <value>
        /// The format_ tags.
        /// </value>
        [XmlAttribute("format_tags")]
        [Description("A list of semi colon separated style names (by default tags) representing the style definition for each entry to be displayed in the Format combo in the toolbar. Each entry must have its relative definition configuration in a setting named 'format_(tagName)'. For example, the 'p' entry has its definition taken from config.format_p")]
        public string Format_Tags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [full page].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [full page]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("fullPage")]
        [Description("Indicates whether the contents to be edited are being input as a full HTML page. A full page includes the <html>, <head>, and <body> elements. The final output will also reflect this setting, including the <body> contents only if this setting is disabled.")]
        public bool FullPage { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        [XmlAttribute("height")]
        [Description("The height of the editing area (that includes the editor content). This can be an integer, for pixel sizes, or any CSS-defined length unit.")]
        public string Height { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [HTML encode output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [HTML encode output]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("htmlEncodeOutput")]
        [Description("Whether to escape HTML when the editor updates the original input element.")]
        public bool HtmlEncodeOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [ignore empty paragraph].
        /// </summary>
        /// <value>
        /// <c>true</c> if [ignore empty paragraph]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("ignoreEmptyParagraph")]
        [Description("Padding text to set off the image in preview area.")]
        public bool IgnoreEmptyParagraph { get; set; }

        /// <summary>
        /// Gets or sets the image_ preview text.
        /// </summary>
        /// <value>
        /// The image_ preview text.
        /// </value>
        [XmlAttribute("image_previewText")]
        [Description("")]
        public string Image_PreviewText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [image_ remove link by empty URL].
        /// </summary>
        /// <value>
        /// <c>true</c> if [image_ remove link by empty URL]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("image_removeLinkByEmptyURL")]
        [Description("Whether to remove links when emptying the link URL field in the image dialog.")]
        public bool Image_RemoveLinkByEmptyURL { get; set; }

        /// <summary>
        /// Gets or sets the indent classes.
        /// </summary>
        /// <value>
        /// The indent classes.
        /// </value>
        [XmlAttribute("indentClasses")]
        [Description("List of classes to use for indenting the contents. If it's null, no classes will be used and instead the indentUnit and indentOffset properties will be used.")]
        public string IndentClasses { get; set; }

        /// <summary>
        /// Gets or sets the indent offset.
        /// </summary>
        /// <value>
        /// The indent offset.
        /// </value>
        [XmlAttribute("indentOffset")]
        [Description("Size of each indentation step.")]
        public int IndentOffset { get; set; }

        /// <summary>
        /// Gets or sets the indent unit.
        /// </summary>
        /// <value>
        /// The indent unit.
        /// </value>
        [XmlAttribute("indentUnit")]
        [Description("Unit for the indentation style.")]
        public string IndentUnit { get; set; }

        /// <summary>
        /// Gets or sets the justify classes.
        /// </summary>
        /// <value>
        /// The justify classes.
        /// </value>
        [XmlAttribute("justifyClasses")]
        [Description("List of classes to use for aligning the contents. If it's null, no classes will be used and instead the corresponding CSS values will be used.")]
        public string JustifyClasses { get; set; }

        /// <summary>
        /// Gets or sets the keystrokes.
        /// </summary>
        /// <value>
        /// The keystrokes.
        /// </value>
        [XmlAttribute("keystrokes")]
        [Description("A list associating keystrokes to editor commands. Each element in the list is an array where the first item is the keystroke, and the second is the name of the command to be executed.")]
        public string Keystrokes { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        [XmlAttribute("language")]
        [Description("The user interface language localization to use. If left empty, the editor will automatically be localized to the user language. If the user language is not supported, the language specified in the defaultLanguage configuration setting is used.")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [link java script links allowed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [link java script links allowed]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("linkJavaScriptLinksAllowed")]
        [Description("Whether JavaScript code is allowed as a href attribute in an anchor tag. With this option enabled it is possible to create links")]
        public bool LinkJavaScriptLinksAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [link show advanced tab].
        /// </summary>
        /// <value>
        /// <c>true</c> if [link show advanced tab]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("linkShowAdvancedTab")]
        [Description("Whether to show the Advanced tab in the Link dialog window.")]
        public bool LinkShowAdvancedTab { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [link show target tab].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [link show target tab]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("linkShowTargetTab")]
        [Description("Whether to show the Target tab in the Link dialog window.")]
        public bool LinkShowTargetTab { get; set; }

        /// <summary>
        /// Gets or sets the color of the magic line.
        /// </summary>
        /// <value>
        /// The color of the magic line.
        /// </value>
        [XmlAttribute("magicline_color")]
        [Description("Defines box color. The color may be adjusted to enhance readability.")]
        public string Magicline_Color { get; set; }

        /// <summary>
        /// Gets or sets the magic line hold distance.
        /// </summary>
        /// <value>
        /// The magic line hold distance.
        /// </value>
        [XmlAttribute("magicline_holdDistance")]
        [Description("Defines the distance between mouse pointer and the box, within which the box stays revealed and no other focus space is offered to be accessed. The value is relative to magicline_triggerOffset.")]
        public string Magicline_HoldDistance { get; set; }

        /// <summary>
        /// Gets or sets the magic line keystroke next.
        /// </summary>
        /// <value>
        /// The magic line keystroke next.
        /// </value>
        [XmlAttribute("magicline_keystrokeNext")]
        [Description("Defines default keystroke that access the closest unreachable focus space after the caret (start of the selection). If there's no any focus space, selection remains.")]
        public int Magicline_KeystrokeNext { get; set; }

        /// <summary>
        /// Gets or sets the magic line keystroke previous.
        /// </summary>
        /// <value>
        /// The magic line keystroke previous.
        /// </value>
        [XmlAttribute("magicline_keystrokePrevious")]
        [Description("Defines default keystroke that access the closest unreachable focus space before the caret (start of the selection). If there's no any focus space, selection remains.")]
        public int Magicline_KeystrokePrevious { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [magic line put everywhere].
        /// </summary>
        /// <value>
        /// <c>true</c> if [magic line put everywhere]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("magicline_putEverywhere")]
        [Description("Activates plugin mode that considers all focus spaces between CKEDITOR.dtd.$block elements as accessible by the box.")]
        public bool Magicline_PutEverywhere { get; set; }

        /// <summary>
        /// Gets or sets the magic line trigger offset.
        /// </summary>
        /// <value>
        /// The magic line trigger offset.
        /// </value>
        [XmlAttribute("magicline_triggerOffset")]
        [Description("Sets the default vertical distance between element edge and mouse pointer that causes the box to appear. The distance is expressed in pixels (px).")]
        public int Magicline_TriggerOffset { get; set; }

        /// <summary>
        /// Gets or sets the menu_ groups.
        /// </summary>
        /// <value>
        /// The menu_ groups.
        /// </value>
        [XmlAttribute("menu_groups")]
        [Description("A comma separated list of items group names to be displayed in the context menu. The order of items will reflect the order specified in this list if no priority was defined in the groups.")]
        public string Menu_Groups { get; set; }

        /// <summary>
        /// Gets or sets the menu_ sub menu delay.
        /// </summary>
        /// <value>
        /// The menu_ sub menu delay.
        /// </value>
        [XmlAttribute("menu_subMenuDelay")]
        [Description("The amount of time, in milliseconds, the editor waits before displaying submenu options when moving the mouse over options that contain submenus, like the 'Cell Properties' entry for tables.")]
        public int Menu_SubMenuDelay { get; set; }

        /// <summary>
        /// Gets or sets the new page HTML.
        /// </summary>
        /// <value>
        /// The new page HTML.
        /// </value>
        [XmlAttribute("newpage_html")]
        [Description("The HTML to load in the editor when the 'new page' command is executed.")]
        public string Newpage_Html { get; set; }

        /// <summary>
        /// Gets or sets the oEmbed max Width Setting.
        /// </summary>
        [XmlAttribute("oembed_maxWidth")]
        [Description("Maximum Width for the embeded Content.")]
        public int Oembed_MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the oEmbed max Height Setting.
        /// </summary>
        [XmlAttribute("oembed_maxHeight")]
        [Description("Maximum Height for the embeded Content.")]
        public int Oembed_MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the oEmbed wrapper class Setting.
        /// </summary>
        [XmlAttribute("oembed_WrapperClass")]
        [Description("Wrapper Css Class for the Div element around the embeded Content.")]
        public string Oembed_WrapperClass { get; set; }

        /// <summary>
        /// Gets or sets the paste from word cleanup file.
        /// </summary>
        /// <value>
        /// The paste from word cleanup file.
        /// </value>
        [XmlAttribute("pasteFromWordCleanupFile")]
        [Description("The file that provides the MS Word cleanup function for pasting operations.")]
        public string PasteFromWordCleanupFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [paste from word numbered heading to list].
        /// </summary>
        /// <value>
        /// <c>true</c> if [paste from word numbered heading to list]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("pasteFromWordNumberedHeadingToList")]
        [Description("Whether to transform MS Word outline numbered headings into lists.")]
        public bool PasteFromWordNumberedHeadingToList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [paste from word prompt cleanup].
        /// </summary>
        /// <value>
        /// <c>true</c> if [paste from word prompt cleanup]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("pasteFromWordPromptCleanup")]
        [Description("Whether to prompt the user about the clean up of content being pasted from MS Word.")]
        public bool PasteFromWordPromptCleanup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [paste from word remove font styles].
        /// </summary>
        /// <value>
        /// <c>true</c> if [paste from word remove font styles]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("pasteFromWordRemoveFontStyles")]
        [Description("Whether to ignore all font related formatting styles")]
        public bool PasteFromWordRemoveFontStyles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [paste from word remove styles].
        /// </summary>
        /// <value>
        /// <c>true</c> if [paste from word remove styles]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("pasteFromWordRemoveStyles")]
        [Description("Whether to remove element styles that can't be managed with the editor. Note that this doesn't handle the font specific styles, which depends on the pasteFromWordRemoveFontStyles setting instead.")]
        public bool PasteFromWordRemoveStyles { get; set; }

        /// <summary>
        /// Gets or sets the plugins.
        /// </summary>
        /// <value>
        /// The plugins.
        /// </value>
        [XmlAttribute("plugins")]
        [Description("Comma separated list of plugins to be used for an editor instance, besides, the actual plugins that to be loaded could be still affected by two other settings: extraPlugins and removePlugins.")]
        public string Plugins { get; set; }

        /// <summary>
        /// Gets or sets the protected source.
        /// </summary>
        /// <value>
        /// The protected source.
        /// </value>
        [XmlAttribute("protectedSource")]
        [Description("List of regular expressions to be executed on input HTML, indicating HTML source code that when matched, must not be available in the WYSIWYG mode for editing.")]
        public string ProtectedSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [read only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [read only]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("readOnly")]
        [Description("If true, makes the editor start in read-only state. Otherwise, it will check if the linked <textarea> element has the disabled attribute.")]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the remove buttons.
        /// </summary>
        /// <value>
        /// The remove buttons.
        /// </value>
        [XmlAttribute("removeButtons")]
        [Description("List of toolbar button names that must not be rendered. This will work as well for non-button toolbar items, like the Font combos.")]
        public string RemoveButtons { get; set; }

        /// <summary>
        /// Gets or sets the remove dialog tabs.
        /// </summary>
        /// <value>
        /// The remove dialog tabs.
        /// </value>
        [XmlAttribute("removeDialogTabs")]
        [Description("The dialog contents to removed. It's a string composed by dialog name and tab name with a colon between them.")]
        public string RemoveDialogTabs { get; set; }

        /// <summary>
        /// Gets or sets the remove format attributes.
        /// </summary>
        /// <value>
        /// The remove format attributes.
        /// </value>
        [XmlAttribute("removeFormatAttributes")]
        [Description("A comma separated list of elements attributes to be removed when executing the remove format command.")]
        public string RemoveFormatAttributes { get; set; }

        /// <summary>
        /// Gets or sets the remove format tags.
        /// </summary>
        /// <value>
        /// The remove format tags.
        /// </value>
        [XmlAttribute("removeFormatTags")]
        [Description("A comma separated list of elements to be removed when executing the remove format command. Note that only inline elements are allowed.")]
        public string RemoveFormatTags { get; set; }

        /// <summary>
        /// Gets or sets the remove plugins.
        /// </summary>
        /// <value>
        /// The remove plugins.
        /// </value>
        [XmlAttribute("removePlugins")]
        [Description("A list of plugins that must not be loaded. This setting makes it possible to avoid loading some plugins defined in the plugins setting, without having to touch it.")]
        public string RemovePlugins { get; set; }

        /// <summary>
        /// Gets or sets the resize directory.
        /// </summary>
        /// <value>
        /// The resize directory.
        /// </value>
        [XmlAttribute("resize_dir")]
        [Description("The dimensions for which the editor resizing is enabled. Possible values are both, vertical, and horizontal.")]
        public string Resize_Dir { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [resize_ enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [resize_ enabled]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("resize_enabled")]
        [Description("Whether to enable the resizing feature. If this feature is disabled, the resize handle will not be visible.")]
        public bool Resize_Enabled { get; set; }

        /// <summary>
        /// Gets or sets the height of the resize_ max.
        /// </summary>
        /// <value>
        /// The height of the resize_ max.
        /// </value>
        [XmlAttribute("resize_maxHeight")]
        [Description("The maximum editor height, in pixels, when resizing the editor interface by using the resize handle.")]
        public int Resize_MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the resize_ max.
        /// </summary>
        /// <value>
        /// The width of the resize_ max.
        /// </value>
        [XmlAttribute("resize_maxWidth")]
        [Description("The maximum editor width, in pixels, when resizing the editor interface by using the resize handle.")]
        public int Resize_MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the resize_ min.
        /// </summary>
        /// <value>
        /// The height of the resize_ min.
        /// </value>
        [XmlAttribute("resize_minHeight")]
        [Description("The minimum editor height, in pixels, when resizing the editor interface by using the resize handle. Note: It falls back to editor's actual height if it is smaller than the default value.")]
        public int Resize_MinHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the resize_ min.
        /// </summary>
        /// <value>
        /// The width of the resize_ min.
        /// </value>
        [XmlAttribute("resize_minWidth")]
        [Description("The minimum editor width, in pixels, when resizing the editor interface by using the resize handle. Note: It falls back to editor's actual width if it is smaller than the default value.")]
        public int Resize_MinWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [SCAYT auto startup].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [SCAYT auto startup]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("scayt_autoStartup")]
        [Description("If enabled (set to true), turns on SCAYT automatically after loading the editor.")]
        public bool Scayt_AutoStartup { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT context commands.
        /// </summary>
        /// <value>
        /// The SCAYT context commands.
        /// </value>
        [XmlAttribute("scayt_contextCommands")]
        [Description("Customizes the display of SCAYT context menu commands ('Add Word', 'Ignore' and 'Ignore All'). This must be a string with one or more of the following words separated by a pipe character,")]
        public string Scayt_ContextCommands { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT context menu items order.
        /// </summary>
        /// <value>
        /// The SCAYT context menu items order.
        /// </value>
        [XmlAttribute("scayt_contextMenuItemsOrder")]
        [Description("Defines the order SCAYT context menu items by groups. This must be a string with one or more of the following words separated by a pipe character")]
        public string Scayt_ContextMenuItemsOrder { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT custom dictionary ids.
        /// </summary>
        /// <value>
        /// The SCAYT custom dictionary ids.
        /// </value>
        [XmlAttribute("scayt_customDictionaryIds")]
        [Description("Links SCAYT to custom dictionaries. This is a string containing dictionary IDs separared by commas (','). Available only for the licensed version.")]
        public string Scayt_CustomDictionaryIds { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT customer id.
        /// </summary>
        /// <value>
        /// The SCAYT customer id.
        /// </value>
        [XmlAttribute("scayt_customerid")]
        [Description("Sets the customer ID for SCAYT. Required for migration from free, ad-supported version to paid, ad-free version.")]
        public string Scayt_Customerid { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT max suggestions.
        /// </summary>
        /// <value>
        /// The SCAYT max suggestions.
        /// </value>
        [XmlAttribute("scayt_maxSuggestions")]
        [Description("Defines the number of SCAYT suggestions to show in the main context menu.")]
        public int Scayt_MaxSuggestions { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT more suggestions.
        /// </summary>
        /// <value>
        /// The SCAYT more suggestions.
        /// </value>
        [XmlAttribute("scayt_moreSuggestions")]
        [Description("Enables/disables the 'More Suggestions' sub-menu in the context menu. Possible values are 'on' and 'off'.")]
        public string Scayt_MoreSuggestions { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT s lang.
        /// </summary>
        /// <value>
        /// The SCAYT s lang.
        /// </value>
        [XmlAttribute("scayt_sLang")]
        [Description("Sets the default spell checking language for SCAYT.")]
        public string Scayt_sLang { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT source URL.
        /// </summary>
        /// <value>
        /// The SCAYT source URL.
        /// </value>
        [XmlAttribute("scayt_srcUrl")]
        [Description("Sets the URL to SCAYT core. Required to switch to the licensed version of SCAYT application.")]
        public string Scayt_SrcUrl { get; set; }

        /// <summary>
        /// Gets or sets the SCAYT UI tabs.
        /// </summary>
        /// <value>
        /// The SCAYT UI tabs.
        /// </value>
        [XmlAttribute("scayt_uiTabs")]
        [Description("Sets the visibility of particular tabs in the SCAYT dialog window and toolbar button")]
        public string Scayt_uiTabs { get; set; }

        /// <summary>
        /// Gets or sets the name of the SCAYT user dictionary.
        /// </summary>
        /// <value>
        /// The name of the SCAYT user dictionary.
        /// </value>
        [XmlAttribute("scayt_userDictionaryName")]
        [Description("Makes it possible to activate a custom dictionary in SCAYT. The user dictionary name must be used. Available only for the licensed version.")]
        public string Scayt_UserDictionaryName { get; set; }

        /// <summary>
        /// Gets or sets the shift enter mode.
        /// </summary>
        /// <value>
        /// The shift enter mode.
        /// </value>
        [XmlAttribute("shiftEnterMode")]
        [Description("Similarly to the enterMode setting, it defines the behavior of the Shift+Enter key combination.")]
        public EnterModus ShiftEnterMode { get; set; }

        /// <summary>
        /// Gets or sets the skin.
        /// </summary>
        /// <value>
        /// The skin.
        /// </value>
        [XmlAttribute("skin")]
        [Description("The editor skin name. Note that is is not possible to have editors with different skin settings in the same page. In such case, just one of the skins will be used for all editors.")]
        public string Skin { get; set; }

        /// <summary>
        /// Gets or sets the smiley_ descriptions.
        /// </summary>
        /// <value>
        /// The smiley_ descriptions.
        /// </value>
        [XmlAttribute("smiley_columns")]
        [Description("The number of columns to be generated by the smilies matrix.")]
        public int Smiley_columns { get; set; }

        /// <summary>
        /// Gets or sets the smiley_ descriptions.
        /// </summary>
        /// <value>
        /// The smiley_ descriptions.
        /// </value>
        [XmlAttribute("smiley_descriptions")]
        [Description("The description to be used for each of the smileys defined in the smiley_images setting. Each entry in this array list must match its relative pair in the smiley_images setting.")]
        public string Smiley_Descriptions { get; set; }

        /// <summary>
        /// Gets or sets the smiley_ images.
        /// </summary>
        /// <value>
        /// The smiley_ images.
        /// </value>
        [XmlAttribute("smiley_images")]
        [Description("The file names for the smileys to be displayed. These files must be contained inside the URL path defined with the smiley_path setting.")]
        public string Smiley_Images { get; set; }

        /// <summary>
        /// Gets or sets the smiley_ path.
        /// </summary>
        /// <value>
        /// The smiley_ path.
        /// </value>
        [XmlAttribute("smiley_path")]
        [Description("The base path used to build the URL for the smiley images. It must end with a slash.")]
        public string Smiley_Path { get; set; }

        /// <summary>
        /// Gets or sets the size of the source area tab.
        /// </summary>
        /// <value>
        /// The size of the source area tab.
        /// </value>
        [XmlAttribute("sourceAreaTabSize")]
        [Description("Controls CSS tab-size property of the sourcearea view.")]
        public int SourceAreaTabSize { get; set; }

        /// <summary>
        /// Gets or sets the special chars.
        /// </summary>
        /// <value>
        /// The special chars.
        /// </value>
        [XmlAttribute("specialChars")]
        [Description("The list of special characters visible in the 'Special Character' dialog window.")]
        public string SpecialChars { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [startup focus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [startup focus]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("startupFocus")]
        [Description("Sets whether the editable should have the focus when editor is loading for the first time.")]
        public bool StartupFocus { get; set; }

        /// <summary>
        /// Gets or sets the startup mode.
        /// </summary>
        /// <value>
        /// The startup mode.
        /// </value>
        [XmlAttribute("startupMode")]
        [Description("The mode to load at the editor startup. It depends on the plugins loaded. By default, the wysiwyg and source modes are available.")]
        public string StartupMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [startup outline blocks].
        /// </summary>
        /// <value>
        /// <c>true</c> if [startup outline blocks]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("startupOutlineBlocks")]
        [Description("Whether to automaticaly enable the show block command when the editor loads.")]
        public bool StartupOutlineBlocks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [startup show borders].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [startup show borders]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("startupShowBorders")]
        [Description("Whether to automatically enable the 'show borders' command when the editor loads.")]
        public bool StartupShowBorders { get; set; }

        /// <summary>
        /// Gets or sets the styles combo_ styles set.
        /// </summary>
        /// <value>
        /// The styles combo_ styles set.
        /// </value>
        [XmlAttribute("stylesSet")]
        [Description("The 'styles definition set' to use in the editor. They will be used in the styles combo and the style selector of the div container.")]
        public string StylesSet { get; set; }

        /// <summary>
        /// Gets or sets the index of the tab.
        /// </summary>
        /// <value>
        /// The index of the tab.
        /// </value>
        [XmlAttribute("tabIndex")]
        [Description("The editor tabindex value.")]
        public int TabIndex { get; set; }

        /// <summary>
        /// Gets or sets the tab spaces.
        /// </summary>
        /// <value>
        /// The tab spaces.
        /// </value>
        [XmlAttribute("tabSpaces")]
        [Description("Intructs the editor to add a number of spaces to the text when hitting the TAB key. If set to zero, the TAB key will be used to move the cursor focus to the next element in the page, out of the editor focus.")]
        public int TabSpaces { get; set; }

        /// <summary>
        /// Gets or sets the templates.
        /// </summary>
        /// <value>
        /// The templates.
        /// </value>
        [XmlAttribute("templates")]
        [Description("The templates definition set to use. It accepts a list of names separated by comma. It must match definitions loaded with the templates_files setting.")]
        public string Templates { get; set; }

        /// <summary>
        /// Gets or sets the templates_ files.
        /// </summary>
        /// <value>
        /// The templates_ files.
        /// </value>
        [XmlAttribute("templates_files")]
        [Description("The list of templates definition files to load.")]
        public string Templates_Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [templates_ replace content].
        /// </summary>
        /// <value>
        /// <c>true</c> if [templates_ replace content]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("templates_replaceContent")]
        [Description("Whether the 'Replace actual contents' checkbox is checked by default in the Templates dialog.")]
        public bool Templates_ReplaceContent { get; set; }

        /// <summary>
        /// Gets or sets the toolbar location.
        /// </summary>
        /// <value>
        /// The toolbar location.
        /// </value>
        [XmlAttribute("toolbar")]
        [Description("The toolbox (alias toolbar) definition. It is a toolbar name or an array of toolbars (strips), each one being also an array, containing a list of UI items.")]
        public string Toolbar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [toolbar can collapse].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [toolbar can collapse]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("toolbarCanCollapse")]
        [Description("Whether the toolbar can be collapsed by the user. If disabled, the collapser button will not be displayed.")]
        public bool ToolbarCanCollapse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [toolbar group cycling].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [toolbar group cycling]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("toolbarGroupCycling")]
        [Description("When enabled, makes the arrow keys navigation cycle within the current toolbar group. Otherwise the arrows will move through all items available in the toolbar. The TAB key will still be used to quickly jump among the toolbar groups.")]
        public bool ToolbarGroupCycling { get; set; }

        /// <summary>
        /// Gets or sets the toolbar groups.
        /// </summary>
        /// <value>
        /// The toolbar groups.
        /// </value>
        [XmlAttribute("toolbarGroups")]
        [Description("The toolbar groups definition.")]
        public string ToolbarGroups { get; set; }

        /// <summary>
        /// Gets or sets the toolbar location.
        /// </summary>
        /// <value>
        /// The toolbar location.
        /// </value>
        [XmlAttribute("toolbarLocation")]
        [Description("The UI space to which rendering the toolbar. For the default editor implementation.")]
        public ToolBarLocation ToolbarLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [toolbar startup expanded].
        /// </summary>
        /// <value>
        /// <c>true</c> if [toolbar startup expanded]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("toolbarStartupExpanded")]
        [Description("Whether the toolbar must start expanded when the editor is loaded.")]
        public bool ToolbarStartupExpanded { get; set; }

        /// <summary>
        /// Gets or sets the color of the UI.
        /// </summary>
        /// <value>
        /// The color of the UI.
        /// </value>
        [XmlAttribute("uiColor")]
        [Description("The base user interface color to be used by the editor. Not all skins are compatible with this setting.")]
        public string UIColor { get; set; }

        /// <summary>
        /// Gets or sets the size of the undo stack.
        /// </summary>
        /// <value>
        /// The size of the undo stack.
        /// </value>
        [XmlAttribute("undoStackSize")]
        [Description("The number of undo steps to be saved. The higher this setting value the more memory is used for it.")]
        public int UndoStackSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use computed state].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use computed state]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("useComputedState")]
        [Description("Indicates that some of the editor features, like alignment and text direction, should use the computed value of the feature to indicate its on/off state instead of using the real value If enabled in a Left-To-Right written document, the 'Left Justify' alignment button will be shown as active, even if the alignment style is not explicitly applied to the current paragraph in the editor.")]
        public bool UseComputedState { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        [XmlAttribute("width")]
        [Description("The editor UI outer width. This can be an integer, for pixel sizes, or any CSS-defined unit. Unlike the height setting, this one will set the outer width of the entire editor UI, not for the editing area only.")]
        public string Width { get; set; }

        /// <summary>
        /// Gets or sets the word count.
        /// </summary>
        /// <value>
        /// The word count.
        /// </value>
        public WordCountConfig WordCount { get; set; }
    }
}