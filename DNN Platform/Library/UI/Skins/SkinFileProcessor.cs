// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;

    public enum SkinParser
    {
        /// <summary>Localized parser.</summary>
        Localized = 0,

        /// <summary>Portable parser.</summary>
        Portable = 1,
    }

    /// <summary>    Handles processing of a list of uploaded skin files into a working skin.</summary>
    public class SkinFileProcessor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinFileProcessor));
        private readonly string dUPLICATEDETAIL = Util.GetLocalizedString("DuplicateSkinObject.Detail");
        private readonly string dUPLICATEERROR = Util.GetLocalizedString("DuplicateSkinObject.Error");
        private readonly string fILESEND = Util.GetLocalizedString("EndSkinFiles");
        private readonly string fILEBEGIN = Util.GetLocalizedString("BeginSkinFile");
        private readonly string fILEEND = Util.GetLocalizedString("EndSkinFile");
        private readonly string iNITIALIZEPROCESSOR = Util.GetLocalizedString("StartProcessor");
        private readonly string lOADSKINTOKEN = Util.GetLocalizedString("LoadingSkinToken");
        private readonly string pACKAGELOAD = Util.GetLocalizedString("PackageLoad");
        private readonly string pACKAGELOADERROR = Util.GetLocalizedString("PackageLoad.Error");
        private readonly ControlParser controlFactory;
        private readonly Hashtable controlList = new Hashtable();
        private readonly ObjectParser objectFactory;
        private readonly PathParser pathFactory = new PathParser();
        private readonly XmlDocument skinAttributes = new XmlDocument { XmlResolver = null };
        private readonly string skinName;
        private readonly string skinPath;
        private readonly string skinRoot;
        private string message = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="SkinFileProcessor"/> class.</summary>
        /// <remarks>This constructor parses a memory based skin.</remarks>
        /// <param name="controlKey">The control key.</param>
        /// <param name="controlSrc">The control source path.</param>
        public SkinFileProcessor(string controlKey, string controlSrc)
        {
            this.controlList.Add(controlKey, controlSrc);

            // Instantiate the control parser with the list of skin objects
            this.controlFactory = new ControlParser(this.controlList);

            // Instantiate the object parser with the list of skin objects
            this.objectFactory = new ObjectParser(this.controlList);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkinFileProcessor"/> class.
        ///     SkinFileProcessor class constructor.
        /// </summary>
        /// <param name="skinPath">File path to the portals upload directory.</param>
        /// <param name="skinRoot">Specifies type of skin (Skins or Containers).</param>
        /// <param name="skinName">Name of folder in which skin will reside (Zip file name).</param>
        /// <remarks>
        ///     The constructor primes the file processor with path information and
        ///     control data that should only be retrieved once.  It checks for the
        ///     existentce of a skin level attribute file and read it in, if found.
        ///     It also sorts through the complete list of controls and creates
        ///     a hashtable which contains only the skin objects and their source paths.
        ///     These are recognized by their ControlKey's which are formatted like
        ///     tokens ("[TOKEN]").  The hashtable is required for speed as it will be
        ///     processed for each token found in the source file by the Control Parser.
        /// </remarks>
        public SkinFileProcessor(string skinPath, string skinRoot, string skinName)
        {
            this.Message += SkinController.FormatMessage(this.iNITIALIZEPROCESSOR, skinRoot + " :: " + skinName, 0, false);

            // Save path information for future use
            this.skinRoot = skinRoot;
            this.skinPath = skinPath;
            this.skinName = skinName;

            // Check for and read skin package level attribute information file
            string fileName = this.SkinPath + this.SkinRoot + "\\" + this.SkinName + "\\" + skinRoot.Substring(0, skinRoot.Length - 1) + ".xml";
            if (File.Exists(fileName))
            {
                try
                {
                    this.SkinAttributes.Load(fileName);
                    this.Message += SkinController.FormatMessage(this.pACKAGELOAD, Path.GetFileName(fileName), 2, false);
                }
                catch (Exception ex)
                {
                    // could not load XML file
                    Logger.Error(ex);
                    this.Message += SkinController.FormatMessage(string.Format(this.pACKAGELOADERROR, ex.Message), Path.GetFileName(fileName), 2, true);
                }
            }

            // Look at every control
            string token;
            foreach (SkinControlInfo objSkinControl in SkinControlController.GetSkinControls().Values)
            {
                token = objSkinControl.ControlKey.ToUpper();

                // If the control is already in the hash table
                if (this.controlList.ContainsKey(token))
                {
                    this.Message += SkinController.FormatMessage(
                        string.Format(this.dUPLICATEERROR, token),
                        string.Format(this.dUPLICATEDETAIL, this.controlList[token], objSkinControl.ControlSrc),
                        2,
                        true);
                }
                else
                {
                    // Add it
                    this.Message += SkinController.FormatMessage(string.Format(this.lOADSKINTOKEN, token), objSkinControl.ControlSrc, 2, false);
                    this.controlList.Add(token, objSkinControl.ControlSrc);
                }
            }

            // Instantiate the control parser with the list of skin objects
            this.controlFactory = new ControlParser(this.controlList);

            // Instantiate the object parser with the list of skin objects
            this.objectFactory = new ObjectParser(this.controlList);
        }

        public string SkinRoot
        {
            get
            {
                return this.skinRoot;
            }
        }

        public string SkinPath
        {
            get
            {
                return this.skinPath;
            }
        }

        public string SkinName
        {
            get
            {
                return this.skinName;
            }
        }

        private PathParser PathFactory
        {
            get
            {
                return this.pathFactory;
            }
        }

        private ControlParser ControlFactory
        {
            get
            {
                return this.controlFactory;
            }
        }

        private ObjectParser ObjectFactory
        {
            get
            {
                return this.objectFactory;
            }
        }

        private XmlDocument SkinAttributes
        {
            get
            {
                return this.skinAttributes;
            }
        }

        private string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
            }
        }

        public string ProcessFile(string fileName, SkinParser parseOption)
        {
            string strMessage = SkinController.FormatMessage(this.fILEBEGIN, Path.GetFileName(fileName), 0, false);
            var objSkinFile = new SkinFile(this.SkinRoot, fileName, this.SkinAttributes);
            switch (objSkinFile.FileExtension)
            {
                case ".htm":
                case ".html":
                    string contents = objSkinFile.Contents;
                    strMessage += this.ObjectFactory.Parse(ref contents);
                    strMessage += this.PathFactory.Parse(ref contents, this.PathFactory.HtmlList, objSkinFile.SkinRootPath, parseOption);
                    strMessage += this.ControlFactory.Parse(ref contents, objSkinFile.Attributes);
                    objSkinFile.Contents = contents;
                    var registrations = new ArrayList();
                    registrations.AddRange(this.ControlFactory.Registrations);
                    registrations.AddRange(this.ObjectFactory.Registrations);
                    strMessage += objSkinFile.PrependASCXDirectives(registrations);
                    break;
            }

            objSkinFile.Write();
            strMessage += objSkinFile.Messages;
            strMessage += SkinController.FormatMessage(this.fILEEND, Path.GetFileName(fileName), 1, false);
            return strMessage;
        }

        /// <summary>    Perform processing on list of files to generate skin.</summary>
        /// <param name="fileList">ArrayList of files to be processed.</param>
        /// <returns>HTML formatted string of informational messages.</returns>
        public string ProcessList(ArrayList fileList)
        {
            return this.ProcessList(fileList, SkinParser.Localized);
        }

        public string ProcessList(ArrayList fileList, SkinParser parseOption)
        {
            foreach (string fileName in fileList)
            {
                this.Message += this.ProcessFile(fileName, parseOption);
            }

            this.Message += SkinController.FormatMessage(this.fILESEND, this.SkinRoot + " :: " + this.SkinName, 0, false);
            return this.Message;
        }

        public string ProcessSkin(string skinSource, XmlDocument skinAttributes, SkinParser parseOption)
        {
            var objSkinFile = new SkinFile(skinSource, skinAttributes);
            string contents = objSkinFile.Contents;
            this.Message += this.ControlFactory.Parse(ref contents, objSkinFile.Attributes);
            this.Message += objSkinFile.PrependASCXDirectives(this.ControlFactory.Registrations);
            return contents;
        }

        /// <summary>    Parsing functionality for token replacement in new skin files.</summary>
        /// <remarks>
        ///     This class encapsulates the data and methods necessary to appropriately
        ///     handle all the token parsing needs for new skin files (which is appropriate
        ///     only for HTML files).  The parser accommodates some ill formatting of tokens
        ///     (ignoring whitespace and casing) and allows for naming of token instances
        ///     if more than one instance of a particular control is desired on a skin.  The
        ///     proper syntax for an instance is: "[TOKEN:INSTANCE]" where the instance can
        ///     be any alphanumeric string.  Generated control ID's all take the
        ///     form of "TOKENINSTANCE".
        /// </remarks>
        private class ControlParser
        {
            private static readonly Regex FindTokenInstance =
                new Regex("\\[\\s*(?<token>\\w*)\\s*:?\\s*(?<instance>\\w*)\\s*]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            /// <summary>
            /// Initializes a new instance of the <see cref="ControlParser"/> class.
            ///     ControlParser class constructor.
            /// </summary>
            /// <remarks>
            ///     The constructor processes accepts a hashtable of skin objects to process against.
            /// </remarks>
            public ControlParser(Hashtable controlList)
            {
                this.ControlList = (Hashtable)controlList.Clone();
            }

            /// <summary>    Gets registration directives generated as a result of the Parse method.</summary>
            /// <returns>ArrayList of formatted registration directives.</returns>
            /// <remarks>
            ///     In addition to the updated file contents, the Parse method also
            ///     creates this list of formatted registration directives which can
            ///     be processed later.  They are not performed in place during the
            ///     Parse method in order to preserve the formatting of the input file
            ///     in case additional parsing might not anticipate the formatting of
            ///     those directives.  Since they are properly formatted, it is better
            ///     to exclude them from being subject to parsing.
            /// </remarks>
            internal ArrayList Registrations => this.RegisterList;

            private MatchEvaluator Handler => this.TokenMatchHandler;

            private Hashtable ControlList { get; }

            private ArrayList RegisterList { get; set; } = new ArrayList();

            private XmlDocument Attributes { get; set; } = new XmlDocument { XmlResolver = null };

            private string Messages { get; set; } = string.Empty;

            /// <summary>    Perform parsing on the specified source file using the specified attributes.</summary>
            /// <param name="source">Pointer to Source string to be parsed.</param>
            /// <param name="attributes">XML document containing token attribute information (can be empty).</param>
            /// <remarks>
            ///     This procedure invokes a handler for each match of a formatted token.
            ///     The attributes are first set because they will be referenced by the
            ///     match handler.
            /// </remarks>
            public string Parse(ref string source, XmlDocument attributes)
            {
                this.Messages = string.Empty;

                // set the token attributes
                this.Attributes = attributes;

                // clear register list
                this.RegisterList.Clear();

                // define the regular expression to match tokens

                // parse the file
                source = FindTokenInstance.Replace(source, this.Handler);
                return this.Messages;
            }

            /// <summary>    Process regular expression matches.</summary>
            /// <param name="m">Regular expression match for token which requires processing.</param>
            /// <returns>Properly formatted token.</returns>
            /// <remarks>
            ///     The handler is invoked by the Regex.Replace method once for each match that
            ///     it encounters.  The returned value of the handler is substituted for the
            ///     original match.  So the handler properly formats the replacement for the
            ///     token and returns it instead.  If an unknown token is encountered, the token
            ///     is unmodified.  This can happen if a token is used for a skin object which
            ///     has not yet been installed.
            /// </remarks>
            private string TokenMatchHandler(Match m)
            {
                string tOKEN_PROC = Util.GetLocalizedString("ProcessToken");
                string tOKEN_SKIN = Util.GetLocalizedString("SkinToken");
                string tOKEN_PANE = Util.GetLocalizedString("PaneToken");
                string tOKEN_FOUND = Util.GetLocalizedString("TokenFound");
                string tOKEN_FORMAT = Util.GetLocalizedString("TokenFormat");
                string tOKEN_NOTFOUND_INFILE = Util.GetLocalizedString("TokenNotFoundInFile");
                string cONTROL_FORMAT = Util.GetLocalizedString("ControlFormat");
                string tOKEN_NOTFOUND = Util.GetLocalizedString("TokenNotFound");

                string token = m.Groups["token"].Value.ToUpper();
                string controlName = token + m.Groups["instance"].Value;

                // if the token has an instance name, use it to look for the corresponding attributes
                string attributeNode = token + (string.IsNullOrEmpty(m.Groups["instance"].Value) ? string.Empty : ":" + m.Groups["instance"].Value);

                this.Messages += SkinController.FormatMessage(tOKEN_PROC, "[" + attributeNode + "]", 2, false);

                // if the token is a recognized skin control
                if (this.ControlList.ContainsKey(token) || token.IndexOf("CONTENTPANE") != -1)
                {
                    string skinControl = string.Empty;

                    if (this.ControlList.ContainsKey(token))
                    {
                        this.Messages += SkinController.FormatMessage(tOKEN_SKIN, (string)this.ControlList[token], 2, false);
                    }
                    else
                    {
                        this.Messages += SkinController.FormatMessage(tOKEN_PANE, token, 2, false);
                    }

                    // f there is an attribute file
                    if (this.Attributes.DocumentElement != null)
                    {
                        // look for the the node of this instance of the token
                        XmlNode xmlSkinAttributeRoot = this.Attributes.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + attributeNode + "]']");

                        // if the token is found
                        if (xmlSkinAttributeRoot != null)
                        {
                            this.Messages += SkinController.FormatMessage(tOKEN_FOUND, "[" + attributeNode + "]", 2, false);

                            // process each token attribute
                            foreach (XmlNode xmlSkinAttribute in xmlSkinAttributeRoot.SelectNodes(".//Settings/Setting"))
                            {
                                if (!string.IsNullOrEmpty(xmlSkinAttribute.SelectSingleNode("Value").InnerText))
                                {
                                    // append the formatted attribute to the inner contents of the control statement
                                    this.Messages += SkinController.FormatMessage(
                                        tOKEN_FORMAT,
                                        xmlSkinAttribute.SelectSingleNode("Name").InnerText + "=\"" + xmlSkinAttribute.SelectSingleNode("Value").InnerText + "\"",
                                        2,
                                        false);
                                    skinControl += " " + xmlSkinAttribute.SelectSingleNode("Name").InnerText + "=\"" + xmlSkinAttribute.SelectSingleNode("Value").InnerText.Replace("\"", "&quot;") +
                                                   "\"";
                                }
                            }
                        }
                        else
                        {
                            this.Messages += SkinController.FormatMessage(tOKEN_NOTFOUND_INFILE, "[" + attributeNode + "]", 2, false);
                        }
                    }

                    if (this.ControlList.ContainsKey(token))
                    {
                        // create the skin object user control tag
                        skinControl = "dnn:" + token + " runat=\"server\" id=\"dnn" + controlName + "\"" + skinControl;

                        // save control registration statement
                        string controlRegistration = "<%@ Register TagPrefix=\"dnn\" TagName=\"" + token + "\" Src=\"~/" + (string)this.ControlList[token] + "\" %>" + Environment.NewLine;
                        if (this.RegisterList.Contains(controlRegistration) == false)
                        {
                            this.RegisterList.Add(controlRegistration);
                        }

                        // return the control statement
                        this.Messages += SkinController.FormatMessage(cONTROL_FORMAT, "&lt;" + skinControl + " /&gt;", 2, false);

                        skinControl = "<" + skinControl + " />";
                    }
                    else
                    {
                        if (skinControl.IndexOf("id=", StringComparison.InvariantCultureIgnoreCase) == -1)
                        {
                            skinControl = " id=\"ContentPane\"";
                        }

                        skinControl = "div runat=\"server\"" + skinControl + "></div";

                        // return the control statement
                        this.Messages += SkinController.FormatMessage(cONTROL_FORMAT, "&lt;" + skinControl + "&gt;", 2, false);

                        skinControl = "<" + skinControl + ">";
                    }

                    return skinControl;
                }
                else
                {
                    // return the unmodified token
                    // note that this is currently protecting array syntax in embedded javascript
                    // should be fixed in the regular expressions but is not, currently.
                    this.Messages += SkinController.FormatMessage(tOKEN_NOTFOUND, "[" + m.Groups["token"].Value + "]", 2, false);
                    return "[" + m.Groups["token"].Value + "]";
                }
            }
        }

        /// <summary>    Parsing functionality for token replacement in new skin files.</summary>
        /// <remarks>
        ///     This class encapsulates the data and methods necessary to appropriately
        ///     handle all the object parsing needs for new skin files (which is appropriate
        ///     only for HTML files).  The parser accommodates some ill formatting of objects
        ///     (ignoring whitespace and casing) and allows for naming of object instances
        ///     if more than one instance of a particular control is desired on a skin.  The
        ///     proper syntax for an instance is: "[OBJECT:INSTANCE]" where the instance can
        ///     be any alphanumeric string.  Generated control ID's all take the
        ///     form of "OBJECTINSTANCE".
        /// </remarks>
        private class ObjectParser
        {
            // define the regular expression to match objects
            private static readonly Regex FindObjectInstance =
                new Regex("\\<object(?<token>.*?)</object>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            private static readonly Regex MultiSpaceRegex = new Regex("\\s+", RegexOptions.Compiled);

            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectParser"/> class.
            ///     ControlParser class constructor.
            /// </summary>
            /// <remarks>
            ///     The constructor processes accepts a hashtable of skin objects to process against.
            /// </remarks>
            public ObjectParser(Hashtable controlList)
            {
                this.ControlList = (Hashtable)controlList.Clone();
            }

            /// <summary>    Gets registration directives generated as a result of the Parse method.</summary>
            /// <returns>ArrayList of formatted registration directives.</returns>
            /// <remarks>
            ///     In addition to the updated file contents, the Parse method also
            ///     creates this list of formatted registration directives which can
            ///     be processed later.  They are not performed in place during the
            ///     Parse method in order to preserve the formatting of the input file
            ///     in case additional parsing might not anticipate the formatting of
            ///     those directives.  Since they are properly formatted, it is better
            ///     to exclude them from being subject to parsing.
            /// </remarks>
            internal ArrayList Registrations => this.RegisterList;

            private MatchEvaluator Handler => this.ObjectMatchHandler;

            private Hashtable ControlList { get; }

            private ArrayList RegisterList { get; set; } = new ArrayList();

            private string Messages { get; set; } = string.Empty;

            /// <summary>    Perform parsing on the specified source file.</summary>
            /// <param name="source">Pointer to Source string to be parsed.</param>
            /// <remarks>
            ///     This procedure invokes a handler for each match of a formatted object.
            /// </remarks>
            public string Parse(ref string source)
            {
                this.Messages = string.Empty;

                // clear register list
                this.RegisterList.Clear();

                // parse the file
                source = FindObjectInstance.Replace(source, this.Handler);

                return this.Messages;
            }

            /// <summary>    Process regular expression matches.</summary>
            /// <param name="m">Regular expression match for object which requires processing.</param>
            /// <returns>Properly formatted token.</returns>
            /// <remarks>
            ///     The handler is invoked by the Regex.Replace method once for each match that
            ///     it encounters.  The returned value of the handler is substituted for the
            ///     original match.  So the handler properly formats the replacement for the
            ///     object and returns it instead.  If an unknown object is encountered, the object
            ///     is unmodified.  This can happen if an object is a client-side object or
            ///     has not yet been installed.
            /// </remarks>
            private string ObjectMatchHandler(Match m)
            {
                string oBJECT_PROC = Util.GetLocalizedString("ProcessObject");
                string oBJECT_SKIN = Util.GetLocalizedString("SkinObject");
                string oBJECT_PANE = Util.GetLocalizedString("PaneObject");
                string cONTROL_FORMAT = Util.GetLocalizedString("ControlFormat");
                string oBJECT_NOTFOUND = Util.GetLocalizedString("ObjectNotFound");

                // "token" string matches will be in the form of (" id=".." codetype=".." codebase=".." etc...><param name=".." value=".." />")
                // we need to assume properly formatted HTML - attributes will be enclosed in double quotes and there will no spaces between assignments ( ie. attribute="value" )

                // extract the embedded object attributes (" id=".." codetype=".." codebase=".." etc...")
                string embeddedObjectAttributes = m.Groups["token"].Value.Substring(0, m.Groups["token"].Value.IndexOf(">"));

                // split into array
                string[] attributes = embeddedObjectAttributes.Split(' ');

                // declare skin object elements
                string attributeNode = string.Empty;
                string token = string.Empty;
                string controlName = string.Empty;

                // iterate and process valid attributes
                string[] attribute;
                string attributeName;
                string attributeValue;
                foreach (string strAttribute in attributes)
                {
                    if (strAttribute != string.Empty)
                    {
                        attribute = strAttribute.Split('=');
                        attributeName = attribute[0].Trim();
                        attributeValue = attribute[1].Trim().Replace("\"", string.Empty);
                        switch (attributeName.ToLowerInvariant())
                        {
                            case "id":
                                controlName = attributeValue;
                                break;
                            case "codetype":
                                attributeNode = attributeValue;
                                break;
                            case "codebase":
                                token = attributeValue.ToUpper();
                                break;
                        }
                    }
                }

                // process skin object
                if (attributeNode.Equals("dotnetnuke/server", StringComparison.InvariantCultureIgnoreCase))
                {
                    // we have a valid skin object specification
                    this.Messages += SkinController.FormatMessage(oBJECT_PROC, token, 2, false);

                    // if the embedded object is a recognized skin object
                    if (this.ControlList.ContainsKey(token) || token == "CONTENTPANE")
                    {
                        string skinControl = string.Empty;

                        if (this.ControlList.ContainsKey(token))
                        {
                            this.Messages += SkinController.FormatMessage(oBJECT_SKIN, (string)this.ControlList[token], 2, false);
                        }
                        else
                        {
                            this.Messages += SkinController.FormatMessage(oBJECT_PANE, token, 2, false);
                        }

                        // process embedded object params
                        string parameters = m.Groups["token"].Value.Substring(m.Groups["token"].Value.IndexOf(">") + 1);
                        parameters = parameters.Replace("<param name=\"", string.Empty);
                        parameters = parameters.Replace("\" value", string.Empty);
                        parameters = parameters.Replace("/>", string.Empty);

                        // convert multiple spaces and carriage returns into single spaces
                        parameters = MultiSpaceRegex.Replace(parameters, " ");

                        if (this.ControlList.ContainsKey(token))
                        {
                            // create the skin object user control tag
                            skinControl = "dnn:" + token + " runat=\"server\" ";
                            if (!string.IsNullOrEmpty(controlName))
                            {
                                skinControl += "id=\"" + controlName + "\" ";
                            }

                            skinControl += parameters;

                            // save control registration statement
                            string controlRegistration = "<%@ Register TagPrefix=\"dnn\" TagName=\"" + token + "\" Src=\"~/" + (string)this.ControlList[token] + "\" %>" + Environment.NewLine;
                            if (this.RegisterList.Contains(controlRegistration) == false)
                            {
                                this.RegisterList.Add(controlRegistration);
                            }

                            // return the control statement
                            this.Messages += SkinController.FormatMessage(cONTROL_FORMAT, "&lt;" + skinControl + " /&gt;", 2, false);
                            skinControl = "<" + skinControl + "/>";
                        }
                        else
                        {
                            skinControl = "div runat=\"server\" ";
                            if (!string.IsNullOrEmpty(controlName))
                            {
                                skinControl += "id=\"" + controlName + "\" ";
                            }
                            else
                            {
                                skinControl += "id=\"ContentPane\" ";
                            }

                            skinControl += parameters + "></div";

                            // return the control statement
                            this.Messages += SkinController.FormatMessage(cONTROL_FORMAT, "&lt;" + skinControl + "&gt;", 2, false);
                            skinControl = "<" + skinControl + ">";
                        }

                        return skinControl;
                    }
                    else
                    {
                        // return the unmodified embedded object
                        this.Messages += SkinController.FormatMessage(oBJECT_NOTFOUND, token, 2, false);
                        return "<object" + m.Groups["token"].Value + "</object>";
                    }
                }
                else
                {
                    // return unmodified embedded object
                    return "<object" + m.Groups["token"].Value + "</object>";
                }
            }
        }

        /// <summary>    Parsing functionality for path replacement in new skin files.</summary>
        /// <remarks>
        ///     This class encapsulates the data and methods necessary to appropriately
        ///     handle all the path replacement parsing needs for new skin files. Parsing
        ///     supported for CSS syntax and HTML syntax (which covers ASCX files also).
        /// </remarks>
        private class PathParser
        {
            private const RegexOptions PatternOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled;

            // retrieve the patterns
            private static readonly Regex[] HtmlArrayPattern =
            [
                new Regex("(?<tag><head[^>]*?\\sprofile\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><object[^>]*?\\s(?:codebase|data|usemap)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><img[^>]*?\\s(?:src|longdesc|usemap)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><input[^>]*?\\s(?:src|usemap)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><iframe[^>]*?\\s(?:src|longdesc)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><(?:td|th|table|body)[^>]*?\\sbackground\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><(?:script|bgsound|embed|xml|frame)[^>]*?\\ssrc\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><(?:base|link|a|area)[^>]*?\\shref\\s*=\\s*\")(?!https://|http://|\\\\|[~/]|javascript:|mailto:)(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><(?:blockquote|ins|del|q)[^>]*?\\scite\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><(?:param\\s+name\\s*=\\s*\"(?:movie|src|base)\")[^>]*?\\svalue\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions),
                new Regex("(?<tag><embed[^>]*?\\s(?:src)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)", PatternOptions)
            ];

            // retrieve the patterns
            private static readonly Regex[] CssArrayPattern =
            [
                new Regex("(?<tag>\\surl\\u0028)(?<content>[^\\u0029]*)(?<endtag>\\u0029.*;)", PatternOptions)
            ];

            private readonly string subst = Util.GetLocalizedString("Substituting");
            private readonly string substDetail = Util.GetLocalizedString("Substituting.Detail");
            private readonly ArrayList cssPatterns = new ArrayList();
            private readonly ArrayList htmlPatterns = new ArrayList();
            private string messages = string.Empty;

            /// <summary>    Gets list of regular expressions for processing HTML syntax.</summary>
            /// <returns>ArrayList of Regex objects formatted for the Parser method.</returns>
            /// <remarks>
            ///     Additional patterns can be added to this list (if necessary) if properly
            ///     formatted to return <tag/>, <content/> and <endtag/> groups.  For future
            ///     consideration, this list could be imported from a configuration file to
            ///     provide for greater flexibility.
            /// </remarks>
            public ArrayList HtmlList
            {
                get
                {
                    // if the arraylist in uninitialized
                    if (this.htmlPatterns.Count == 0)
                    {
                        // for each pattern, create a regex object
                        this.htmlPatterns.AddRange(HtmlArrayPattern);

                        // optimize the arraylist size since it will not change
                        this.htmlPatterns.TrimToSize();
                    }

                    return this.htmlPatterns;
                }
            }

            /// <summary>    Gets list of regular expressions for processing CSS syntax.</summary>
            /// <returns>ArrayList of Regex objects formatted for the Parser method.</returns>
            /// <remarks>
            ///     Additional patterns can be added to this list (if necessary) if properly
            ///     formatted to return <tag/>, <content/> and <endtag/> groups.  For future
            ///     consideration, this list could be imported from a configuration file to
            ///     provide for greater flexibility.
            /// </remarks>
            public ArrayList CSSList
            {
                get
                {
                    // if the arraylist in uninitialized
                    if (this.cssPatterns.Count == 0)
                    {
                        // for each pattern, create a regex object
                        this.cssPatterns.AddRange(CssArrayPattern);

                        // optimize the arraylist size since it will not change
                        this.cssPatterns.TrimToSize();
                    }

                    return this.cssPatterns;
                }
            }

            private MatchEvaluator Handler => this.MatchHandler;

            private string SkinPath { get; set; } = string.Empty;

            private SkinParser ParseOption { get; set; }

            /// <summary>    Perform parsing on the specified source file.</summary>
            /// <param name="source">Pointer to Source string to be parsed.</param>
            /// <param name="regexList">ArrayList of properly formatted regular expression objects.</param>
            /// <param name="skinPath">Path to use in replacement operation.</param>
            /// <param name="parseOption">Parse Option.</param>
            /// <remarks>
            ///     This procedure iterates through the list of regular expression objects
            ///     and invokes a handler for each match which uses the specified path.
            /// </remarks>
            public string Parse(ref string source, ArrayList regexList, string skinPath, SkinParser parseOption)
            {
                this.messages = string.Empty;

                // set path property which is file specific
                this.SkinPath = skinPath;

                // set parse option
                this.ParseOption = parseOption;

                // process each regular expression
                for (int i = 0; i <= regexList.Count - 1; i++)
                {
                    source = ((Regex)regexList[i]).Replace(source, this.Handler);
                }

                return this.messages;
            }

            /// <summary>    Process regular expression matches.</summary>
            /// <param name="m">Regular expression match for path information which requires processing.</param>
            /// <returns>Properly formatted path information.</returns>
            /// <remarks>
            ///     The handler is invoked by the Regex.Replace method once for each match that
            ///     it encounters.  The returned value of the handler is substituted for the
            ///     original match.  So the handler properly formats the path information and
            ///     returns it in favor of the improperly formatted match.
            /// </remarks>
            private string MatchHandler(Match m)
            {
                string strOldTag = m.Groups["tag"].Value + m.Groups["content"].Value + m.Groups["endtag"].Value;
                string strNewTag = strOldTag;

                // we do not want to process object tags to DotNetNuke widgets
                if (!m.Groups[0].Value.ToLowerInvariant().Contains("codetype=\"dotnetnuke/client\""))
                {
                    switch (this.ParseOption)
                    {
                        case SkinParser.Localized:
                            // if the tag does not contain the localized path
                            if (strNewTag.IndexOf(this.SkinPath) == -1)
                            {
                                // insert the localized path
                                strNewTag = m.Groups["tag"].Value + this.SkinPath + m.Groups["content"].Value + m.Groups["endtag"].Value;
                            }

                            break;
                        case SkinParser.Portable:
                            // if the tag does not contain a reference to the skinpath
                            if (strNewTag.IndexOf("<%= skinpath %>", StringComparison.InvariantCultureIgnoreCase) == -1)
                            {
                                // insert the skinpath
                                strNewTag = m.Groups["tag"].Value + "<%= SkinPath %>" + m.Groups["content"].Value + m.Groups["endtag"].Value;
                            }

                            // if the tag contains the localized path
                            if (strNewTag.IndexOf(this.SkinPath) != -1)
                            {
                                // remove the localized path
                                strNewTag = strNewTag.Replace(this.SkinPath, string.Empty);
                            }

                            break;
                    }
                }

                this.messages += SkinController.FormatMessage(this.subst, string.Format(this.substDetail, HttpUtility.HtmlEncode(strOldTag), HttpUtility.HtmlEncode(strNewTag)), 2, false);
                return strNewTag;
            }
        }

        /// <summary>    Utility class for processing of skin files.</summary>
        private class SkinFile
        {
            private const string StrPattern = "<\\s*body[^>]*>(?<skin>.*)<\\s*/\\s*body\\s*>";

            private static readonly Regex PaneCheck1Regex = new Regex("\\s*id\\s*=\\s*\"" + Globals.glbDefaultPane + "\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            private static readonly Regex PaneCheck2Regex = new Regex("\\s*[" + Globals.glbDefaultPane + "]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            private static readonly Regex BodyExtractionRegex = new Regex(StrPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

            private readonly string cONTROLDIR = Util.GetLocalizedString("ControlDirective");
            private readonly string cONTROLREG = Util.GetLocalizedString("ControlRegister");
            private readonly string fILEFORMATERROR = Util.GetLocalizedString("FileFormat.Error");
            private readonly string fILELOAD = Util.GetLocalizedString("SkinFileLoad");
            private readonly string fILELOADERROR = Util.GetLocalizedString("SkinFileLoad.Error");
            private readonly string fILEWRITE = Util.GetLocalizedString("FileWrite");
            private readonly XmlDocument fileAttributes;
            private readonly string fileExtension;
            private readonly string fileName;
            private readonly string skinRoot;
            private readonly string skinRootPath;
            private readonly string writeFileName;
            private string fILEFORMATDETAIL = Util.GetLocalizedString("FileFormat.Detail");
            private string messages = string.Empty;

            /// <summary>Initializes a new instance of the <see cref="SkinFile"/> class.</summary>
            /// <param name="skinContents">The contents of the skin file.</param>
            /// <param name="skinAttributes">The attributes to merge into the skin file.</param>
            /// <remarks>
            ///     The constructor primes the utility class with basic file information.
            ///     It also checks for the existence of a skinfile level attribute file
            ///     and read it in, if found.
            /// </remarks>
            public SkinFile(string skinContents, XmlDocument skinAttributes)
            {
                this.fileAttributes = skinAttributes;
                this.Contents = skinContents;
            }

            /// <summary>Initializes a new instance of the <see cref="SkinFile"/> class.</summary>
            /// <param name="skinRoot">The root path for the skin.</param>
            /// <param name="fileName">The name of the skin file.</param>
            /// <param name="skinAttributes">The attributes to merge into the skin file.</param>
            /// <remarks>
            ///     The constructor primes the utility class with basic file information.
            ///     It also checks for the existence of a skinfile level attribute file
            ///     and read it in, if found.
            /// </remarks>
            public SkinFile(string skinRoot, string fileName, XmlDocument skinAttributes)
            {
                // capture file information
                this.fileName = fileName;
                this.fileExtension = Path.GetExtension(fileName);
                this.skinRoot = skinRoot;
                this.fileAttributes = skinAttributes;

                // determine and store path to portals skin root folder
                string strTemp = fileName.Replace(Path.GetFileName(fileName), string.Empty);
                strTemp = strTemp.Replace("\\", "/");
                this.skinRootPath = Globals.ApplicationPath + strTemp.Substring(strTemp.ToUpper().IndexOf("/PORTALS"));

                // read file contents
                this.Contents = this.Read(fileName);

                // setup some attributes based on file extension
                switch (this.FileExtension)
                {
                    case ".htm":
                    case ".html":
                        // set output file name to <filename>.ASCX
                        this.writeFileName = fileName.Replace(Path.GetExtension(fileName), ".ascx");

                        // capture warning if file does not contain a id="ContentPane" or [CONTENTPANE]
                        if (!PaneCheck1Regex.IsMatch(this.Contents) && !PaneCheck2Regex.IsMatch(this.Contents))
                        {
                            this.messages += SkinController.FormatMessage(this.fILEFORMATERROR, string.Format(this.fILEFORMATERROR, fileName), 2, true);
                        }

                        // Check for existence of and load skin file level attribute information
                        if (File.Exists(fileName.Replace(this.FileExtension, ".xml")))
                        {
                            try
                            {
                                this.fileAttributes.Load(fileName.Replace(this.FileExtension, ".xml"));
                                this.messages += SkinController.FormatMessage(this.fILELOAD, fileName, 2, false);
                            }
                            catch (Exception exc)
                            {
                                // could not load XML file
                                Logger.Error(exc);
                                this.fileAttributes = skinAttributes;
                                this.messages += SkinController.FormatMessage(this.fILELOADERROR, fileName, 2, true);
                            }
                        }

                        break;
                    default:
                        // output file name is same as input file name
                        this.writeFileName = fileName;
                        break;
                }
            }

            public string SkinRoot
            {
                get
                {
                    return this.skinRoot;
                }
            }

            public XmlDocument Attributes
            {
                get
                {
                    return this.fileAttributes;
                }
            }

            public string Messages
            {
                get
                {
                    return this.messages;
                }
            }

            public string FileName
            {
                get
                {
                    return this.fileName;
                }
            }

            public string WriteFileName
            {
                get
                {
                    return this.writeFileName;
                }
            }

            public string FileExtension
            {
                get
                {
                    return this.fileExtension;
                }
            }

            public string SkinRootPath
            {
                get
                {
                    return this.skinRootPath;
                }
            }

            public string Contents { get; set; }

            public void Write()
            {
                // delete the file before attempting to write
                if (File.Exists(this.WriteFileName))
                {
                    File.Delete(this.WriteFileName);
                }

                this.messages += SkinController.FormatMessage(this.fILEWRITE, Path.GetFileName(this.WriteFileName), 2, false);
                using (var objStreamWriter = new StreamWriter(this.WriteFileName))
                {
                    objStreamWriter.WriteLine(this.Contents);
                    objStreamWriter.Flush();
                    objStreamWriter.Close();
                }
            }

            /// <summary>    Prepend ascx control directives to file contents.</summary>
            /// <param name="registrations">ArrayList of registration directives.</param>
            /// <remarks>
            ///     This procedure formats the @Control directive and prepends it and all
            ///     registration directives to the file contents.
            /// </remarks>
            public string PrependASCXDirectives(ArrayList registrations)
            {
                string messages = string.Empty;
                string prefix = string.Empty;

                // format and save @Control directive
                Match objMatch = BodyExtractionRegex.Match(this.Contents);
                if (objMatch.Success && !string.IsNullOrEmpty(objMatch.Groups[1].Value))
                {
                    // if the skin source is an HTML document, extract the content within the <body> tags
                    this.Contents = objMatch.Groups[1].Value;
                }

                if (this.SkinRoot == SkinController.RootSkin)
                {
                    prefix += "<%@ Control language=\"vb\" AutoEventWireup=\"false\" Explicit=\"True\" Inherits=\"DotNetNuke.UI.Skins.Skin\" %>" + Environment.NewLine;
                }
                else if (this.SkinRoot == SkinController.RootContainer)
                {
                    prefix += "<%@ Control language=\"vb\" AutoEventWireup=\"false\" Explicit=\"True\" Inherits=\"DotNetNuke.UI.Containers.Container\" %>" + Environment.NewLine;
                }

                messages += SkinController.FormatMessage(this.cONTROLDIR, HttpUtility.HtmlEncode(prefix), 2, false);

                // add preformatted Control Registrations
                foreach (string item in registrations)
                {
                    messages += SkinController.FormatMessage(this.cONTROLREG, HttpUtility.HtmlEncode(item), 2, false);
                    prefix += item;
                }

                // update file contents to include ascx header information
                this.Contents = prefix + this.Contents;
                return messages;
            }

            private string Read(string fileName)
            {
                using (var objStreamReader = new StreamReader(fileName))
                {
                    string strFileContents = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                    return strFileContents;
                }
            }
        }
    }
}
