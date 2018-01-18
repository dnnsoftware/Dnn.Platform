#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer;

#endregion

namespace DotNetNuke.UI.Skins
{
    public enum SkinParser
    {
        Localized,
        Portable
    }

    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : SkinFileProcessor
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Handles processing of a list of uploaded skin files into a working skin.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinFileProcessor
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SkinFileProcessor));
        #region "Private Members"

        private readonly string DUPLICATE_DETAIL = Util.GetLocalizedString("DuplicateSkinObject.Detail");
        private readonly string DUPLICATE_ERROR = Util.GetLocalizedString("DuplicateSkinObject.Error");
        private readonly string FILES_END = Util.GetLocalizedString("EndSkinFiles");
        private readonly string FILE_BEGIN = Util.GetLocalizedString("BeginSkinFile");
        private readonly string FILE_END = Util.GetLocalizedString("EndSkinFile");
        private readonly string INITIALIZE_PROCESSOR = Util.GetLocalizedString("StartProcessor");
        private readonly string LOAD_SKIN_TOKEN = Util.GetLocalizedString("LoadingSkinToken");
        private readonly string PACKAGE_LOAD = Util.GetLocalizedString("PackageLoad");
        private readonly string PACKAGE_LOAD_ERROR = Util.GetLocalizedString("PackageLoad.Error");
        private readonly ControlParser m_ControlFactory;
        private readonly Hashtable m_ControlList = new Hashtable();
        private readonly ObjectParser m_ObjectFactory;
        private readonly PathParser m_PathFactory = new PathParser();
        private readonly XmlDocument m_SkinAttributes = new XmlDocument { XmlResolver = null };
        private readonly string m_SkinName;
        private readonly string m_SkinPath;
        private readonly string m_SkinRoot;
        private string m_Message = "";

        #endregion

        #region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     SkinFileProcessor class constructor.
        /// </summary>
        /// <remarks>
        ///     This constructor parses a memory based skin
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public SkinFileProcessor(string ControlKey, string ControlSrc)
        {
            m_ControlList.Add(ControlKey, ControlSrc);

            //Instantiate the control parser with the list of skin objects
            m_ControlFactory = new ControlParser(m_ControlList);

            //Instantiate the object parser with the list of skin objects
            m_ObjectFactory = new ObjectParser(m_ControlList);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     SkinFileProcessor class constructor.
        /// </summary>
        /// <param name="SkinPath">File path to the portals upload directory.</param>
        /// <param name="SkinRoot">Specifies type of skin (Skins or Containers)</param>
        /// <param name="SkinName">Name of folder in which skin will reside (Zip file name)</param>
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
        /// -----------------------------------------------------------------------------
        public SkinFileProcessor(string SkinPath, string SkinRoot, string SkinName)
        {
            Message += SkinController.FormatMessage(INITIALIZE_PROCESSOR, SkinRoot + " :: " + SkinName, 0, false);

            //Save path information for future use
            m_SkinRoot = SkinRoot;
            m_SkinPath = SkinPath;
            m_SkinName = SkinName;

            //Check for and read skin package level attribute information file
            string FileName = this.SkinPath + this.SkinRoot + "\\" + this.SkinName + "\\" + SkinRoot.Substring(0, SkinRoot.Length - 1) + ".xml";
            if (File.Exists(FileName))
            {
                try
                {
                    SkinAttributes.Load(FileName);
                    Message += SkinController.FormatMessage(PACKAGE_LOAD, Path.GetFileName(FileName), 2, false);
                }
                catch (Exception ex)
                {
                    //could not load XML file
                    Logger.Error(ex);
                    Message += SkinController.FormatMessage(string.Format(PACKAGE_LOAD_ERROR, ex.Message), Path.GetFileName(FileName), 2, true);
                }
            }

            //Look at every control
            string Token;
            foreach (SkinControlInfo objSkinControl in SkinControlController.GetSkinControls().Values)
            {
                Token = objSkinControl.ControlKey.ToUpper();

                //If the control is already in the hash table
                if (m_ControlList.ContainsKey(Token))
                {
                    Message += SkinController.FormatMessage(string.Format(DUPLICATE_ERROR, objSkinControl.ControlKey.ToUpper()),
                                                            string.Format(DUPLICATE_DETAIL, m_ControlList[Token], objSkinControl.ControlSrc),
                                                            2,
                                                            true);
                }
                else
                {
                    //Add it
                    Message += SkinController.FormatMessage(string.Format(LOAD_SKIN_TOKEN, objSkinControl.ControlKey.ToUpper()), objSkinControl.ControlSrc, 2, false);
                    m_ControlList.Add(Token, objSkinControl.ControlSrc);
                }
            }

            //Instantiate the control parser with the list of skin objects
            m_ControlFactory = new ControlParser(m_ControlList);

            //Instantiate the object parser with the list of skin objects
            m_ObjectFactory = new ObjectParser(m_ControlList);
        }

        #endregion

        #region "Private Properties"

        private PathParser PathFactory
        {
            get
            {
                return m_PathFactory;
            }
        }

        private ControlParser ControlFactory
        {
            get
            {
                return m_ControlFactory;
            }
        }

        private ObjectParser ObjectFactory
        {
            get
            {
                return m_ObjectFactory;
            }
        }

        private XmlDocument SkinAttributes
        {
            get
            {
                return m_SkinAttributes;
            }
        }

        private string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }

        #endregion

        #region "Public Properties"

        public string SkinRoot
        {
            get
            {
                return m_SkinRoot;
            }
        }

        public string SkinPath
        {
            get
            {
                return m_SkinPath;
            }
        }

        public string SkinName
        {
            get
            {
                return m_SkinName;
            }
        }

        #endregion

        #region "Public Methods"

        public string ProcessFile(string FileName, SkinParser ParseOption)
        {
            string strMessage = SkinController.FormatMessage(FILE_BEGIN, Path.GetFileName(FileName), 0, false);
            var objSkinFile = new SkinFile(SkinRoot, FileName, SkinAttributes);
            switch (objSkinFile.FileExtension)
            {
                case ".htm":
                case ".html":
                    string contents = objSkinFile.Contents;
                    strMessage += ObjectFactory.Parse(ref contents);
                    strMessage += PathFactory.Parse(ref contents, PathFactory.HTMLList, objSkinFile.SkinRootPath, ParseOption);
                    strMessage += ControlFactory.Parse(ref contents, objSkinFile.Attributes);
                    objSkinFile.Contents = contents;
                    var Registrations = new ArrayList();
                    Registrations.AddRange(ControlFactory.Registrations);
                    Registrations.AddRange(ObjectFactory.Registrations);
                    strMessage += objSkinFile.PrependASCXDirectives(Registrations);
                    break;
            }
            objSkinFile.Write();
            strMessage += objSkinFile.Messages;
            strMessage += SkinController.FormatMessage(FILE_END, Path.GetFileName(FileName), 1, false);
            return strMessage;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Perform processing on list of files to generate skin.
        /// </summary>
        /// <param name="FileList">ArrayList of files to be processed.</param>
        /// <returns>HTML formatted string of informational messages.</returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string ProcessList(ArrayList FileList)
        {
            return ProcessList(FileList, SkinParser.Localized);
        }

        public string ProcessList(ArrayList FileList, SkinParser ParseOption)
        {
            foreach (string FileName in FileList)
            {
                Message += ProcessFile(FileName, ParseOption);
            }
            Message += SkinController.FormatMessage(FILES_END, SkinRoot + " :: " + SkinName, 0, false);
            return Message;
        }

        public string ProcessSkin(string SkinSource, XmlDocument SkinAttributes, SkinParser ParseOption)
        {
            var objSkinFile = new SkinFile(SkinSource, SkinAttributes);
            string contents = objSkinFile.Contents;
            Message += ControlFactory.Parse(ref contents, objSkinFile.Attributes);
            Message += objSkinFile.PrependASCXDirectives(ControlFactory.Registrations);
            return contents;
        }

        #endregion

        #region Nested type: ControlParser

        /// -----------------------------------------------------------------------------
        /// Project	 : DotNetNuke
        /// Class	 : SkinFileProcessor.ControlParser
        /// 
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Parsing functionality for token replacement in new skin files.
        /// </summary>
        /// <remarks>
        ///     This class encapsulates the data and methods necessary to appropriately
        ///     handle all the token parsing needs for new skin files (which is appropriate
        ///     only for HTML files).  The parser accomodates some ill formatting of tokens
        ///     (ignoring whitespace and casing) and allows for naming of token instances
        ///     if more than one instance of a particular control is desired on a skin.  The
        ///     proper syntax for an instance is: "[TOKEN:INSTANCE]" where the instance can
        ///     be any alphanumeric string.  Generated control ID's all take the
        ///     form of "TOKENINSTANCE".
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private class ControlParser
        {
            private readonly Hashtable m_ControlList;
            private XmlDocument m_Attributes = new XmlDocument { XmlResolver = null };
            private string m_ParseMessages = "";
            private ArrayList m_RegisterList = new ArrayList();

            private static readonly Regex FindTokenInstance =
                new Regex("\\[\\s*(?<token>\\w*)\\s*:?\\s*(?<instance>\\w*)\\s*]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     ControlParser class constructor.
            /// </summary>
            /// <remarks>
            ///     The constructor processes accepts a hashtable of skin objects to process against.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public ControlParser(Hashtable ControlList)
            {
                m_ControlList = (Hashtable)ControlList.Clone();
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Registration directives generated as a result of the Parse method.
            /// </summary>
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
            /// -----------------------------------------------------------------------------
            internal ArrayList Registrations
            {
                get
                {
                    return m_RegisterList;
                }
            }

            private MatchEvaluator Handler
            {
                get
                {
                    return TokenMatchHandler;
                }
            }

            private ArrayList RegisterList
            {
                get
                {
                    return m_RegisterList;
                }
                set
                {
                    m_RegisterList = value;
                }
            }

            private Hashtable ControlList
            {
                get
                {
                    return m_ControlList;
                }
            }

            private XmlDocument Attributes
            {
                get
                {
                    return m_Attributes;
                }
                set
                {
                    m_Attributes = value;
                }
            }

            private string Messages
            {
                get
                {
                    return m_ParseMessages;
                }
                set
                {
                    m_ParseMessages = value;
                }
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Perform parsing on the specified source file using the specified attributes.
            /// </summary>
            /// <param name="Source">Pointer to Source string to be parsed.</param>
            /// <param name="Attributes">XML document containing token attribute information (can be empty).</param>
            /// <remarks>
            ///     This procedure invokes a handler for each match of a formatted token.
            ///     The attributes are first set because they will be referenced by the
            ///     match handler.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public string Parse(ref string Source, XmlDocument Attributes)
            {
                Messages = string.Empty;
                //set the token attributes
                this.Attributes = Attributes;
                //clear register list
                RegisterList.Clear();

                //define the regular expression to match tokens

                //parse the file
                Source = FindTokenInstance.Replace(Source, Handler);
                return Messages;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Process regular expression matches.
            /// </summary>
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
            /// -----------------------------------------------------------------------------
            private string TokenMatchHandler(Match m)
            {
                string TOKEN_PROC = Util.GetLocalizedString("ProcessToken");
                string TOKEN_SKIN = Util.GetLocalizedString("SkinToken");
                string TOKEN_PANE = Util.GetLocalizedString("PaneToken");
                string TOKEN_FOUND = Util.GetLocalizedString("TokenFound");
                string TOKEN_FORMAT = Util.GetLocalizedString("TokenFormat");
                string TOKEN_NOTFOUND_INFILE = Util.GetLocalizedString("TokenNotFoundInFile");
                string CONTROL_FORMAT = Util.GetLocalizedString("ControlFormat");
                string TOKEN_NOTFOUND = Util.GetLocalizedString("TokenNotFound");

                string Token = m.Groups["token"].Value.ToUpper();
                string ControlName = Token + m.Groups["instance"].Value;

                //if the token has an instance name, use it to look for the corresponding attributes
                string AttributeNode = Token + (String.IsNullOrEmpty(m.Groups["instance"].Value) ? "" : ":" + m.Groups["instance"].Value);

                Messages += SkinController.FormatMessage(TOKEN_PROC, "[" + AttributeNode + "]", 2, false);

                //if the token is a recognized skin control
                if (ControlList.ContainsKey(Token) || Token.IndexOf("CONTENTPANE") != -1)
                {
                    string SkinControl = "";

                    if (ControlList.ContainsKey(Token))
                    {
                        Messages += SkinController.FormatMessage(TOKEN_SKIN, (string)ControlList[Token], 2, false);
                    }
                    else
                    {
                        Messages += SkinController.FormatMessage(TOKEN_PANE, Token, 2, false);
                    }

                    //f there is an attribute file
                    if (Attributes.DocumentElement != null)
                    {
                        //look for the the node of this instance of the token
                        XmlNode xmlSkinAttributeRoot = Attributes.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + AttributeNode + "]']");
                        //if the token is found
                        if (xmlSkinAttributeRoot != null)
                        {
                            Messages += SkinController.FormatMessage(TOKEN_FOUND, "[" + AttributeNode + "]", 2, false);
                            //process each token attribute
                            foreach (XmlNode xmlSkinAttribute in xmlSkinAttributeRoot.SelectNodes(".//Settings/Setting"))
                            {
                                if (!String.IsNullOrEmpty(xmlSkinAttribute.SelectSingleNode("Value").InnerText))
                                {
                                    //append the formatted attribute to the inner contents of the control statement
                                    Messages += SkinController.FormatMessage(TOKEN_FORMAT,
                                                                             xmlSkinAttribute.SelectSingleNode("Name").InnerText + "=\"" + xmlSkinAttribute.SelectSingleNode("Value").InnerText + "\"",
                                                                             2,
                                                                             false);
                                    SkinControl += " " + xmlSkinAttribute.SelectSingleNode("Name").InnerText + "=\"" + xmlSkinAttribute.SelectSingleNode("Value").InnerText.Replace("\"", "&quot;") +
                                                   "\"";
                                }
                            }
                        }
                        else
                        {
                            Messages += SkinController.FormatMessage(TOKEN_NOTFOUND_INFILE, "[" + AttributeNode + "]", 2, false);
                        }
                    }
                    if (ControlList.ContainsKey(Token))
                    {
                        //create the skin object user control tag
                        SkinControl = "dnn:" + Token + " runat=\"server\" id=\"dnn" + ControlName + "\"" + SkinControl;

                        //save control registration statement
                        string ControlRegistration = "<%@ Register TagPrefix=\"dnn\" TagName=\"" + Token + "\" Src=\"~/" + (string)ControlList[Token] + "\" %>" + Environment.NewLine;
                        if (RegisterList.Contains(ControlRegistration) == false)
                        {
                            RegisterList.Add(ControlRegistration);
                        }

                        //return the control statement
                        Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + " /&gt;", 2, false);

                        SkinControl = "<" + SkinControl + " />";
                    }
                    else
                    {
                        if (SkinControl.ToLower().IndexOf("id=") == -1)
                        {
                            SkinControl = " id=\"ContentPane\"";
                        }
                        SkinControl = "div runat=\"server\"" + SkinControl + "></div";

                        //return the control statement
                        Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + "&gt;", 2, false);

                        SkinControl = "<" + SkinControl + ">";
                    }
                    return SkinControl;
                }
                else
                {
                    //return the unmodified token
                    //note that this is currently protecting array syntax in embedded javascript
                    //should be fixed in the regular expressions but is not, currently.
                    Messages += SkinController.FormatMessage(TOKEN_NOTFOUND, "[" + m.Groups["token"].Value + "]", 2, false);
                    return "[" + m.Groups["token"].Value + "]";
                }
            }
        }

        #endregion

        #region Nested type: ObjectParser

        /// -----------------------------------------------------------------------------
        /// Project	 : DotNetNuke
        /// Class	 : SkinFileProcessor.ObjectParser
        /// 
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Parsing functionality for token replacement in new skin files.
        /// </summary>
        /// <remarks>
        ///     This class encapsulates the data and methods necessary to appropriately
        ///     handle all the object parsing needs for new skin files (which is appropriate
        ///     only for HTML files).  The parser accomodates some ill formatting of objects
        ///     (ignoring whitespace and casing) and allows for naming of object instances
        ///     if more than one instance of a particular control is desired on a skin.  The
        ///     proper syntax for an instance is: "[OBJECT:INSTANCE]" where the instance can
        ///     be any alphanumeric string.  Generated control ID's all take the
        ///     form of "OBJECTINSTANCE".
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private class ObjectParser
        {
            private readonly Hashtable m_ControlList;
            private string m_ParseMessages = "";
            private ArrayList m_RegisterList = new ArrayList();

            //define the regular expression to match objects
            private static readonly Regex FindObjectInstance =
                new Regex("\\<object(?<token>.*?)</object>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            private static readonly Regex MultiSpaceRegex = new Regex("\\s+", RegexOptions.Compiled);

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     ControlParser class constructor.
            /// </summary>
            /// <remarks>
            ///     The constructor processes accepts a hashtable of skin objects to process against.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public ObjectParser(Hashtable ControlList)
            {
                m_ControlList = (Hashtable)ControlList.Clone();
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Registration directives generated as a result of the Parse method.
            /// </summary>
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
            /// -----------------------------------------------------------------------------
            internal ArrayList Registrations
            {
                get
                {
                    return m_RegisterList;
                }
            }

            private MatchEvaluator Handler
            {
                get
                {
                    return ObjectMatchHandler;
                }
            }

            private ArrayList RegisterList
            {
                get
                {
                    return m_RegisterList;
                }
                set
                {
                    m_RegisterList = value;
                }
            }

            private Hashtable ControlList
            {
                get
                {
                    return m_ControlList;
                }
            }

            private string Messages
            {
                get
                {
                    return m_ParseMessages;
                }
                set
                {
                    m_ParseMessages = value;
                }
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Perform parsing on the specified source file.
            /// </summary>
            /// <param name="Source">Pointer to Source string to be parsed.</param>
            /// <remarks>
            ///     This procedure invokes a handler for each match of a formatted object.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public string Parse(ref string Source)
            {
                Messages = string.Empty;

                //clear register list
                RegisterList.Clear();

                //parse the file
                Source = FindObjectInstance.Replace(Source, Handler);

                return Messages;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Process regular expression matches.
            /// </summary>
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
            /// -----------------------------------------------------------------------------
            private string ObjectMatchHandler(Match m)
            {
                string OBJECT_PROC = Util.GetLocalizedString("ProcessObject");
                string OBJECT_SKIN = Util.GetLocalizedString("SkinObject");
                string OBJECT_PANE = Util.GetLocalizedString("PaneObject");
                string CONTROL_FORMAT = Util.GetLocalizedString("ControlFormat");
                string OBJECT_NOTFOUND = Util.GetLocalizedString("ObjectNotFound");

                //"token" string matches will be in the form of (" id=".." codetype=".." codebase=".." etc...><param name=".." value=".." />")
                //we need to assume properly formatted HTML - attributes will be enclosed in double quotes and there will no spaces between assignments ( ie. attribute="value" )

                //extract the embedded object attributes (" id=".." codetype=".." codebase=".." etc...")
                string EmbeddedObjectAttributes = m.Groups["token"].Value.Substring(0, m.Groups["token"].Value.IndexOf(">"));

                //split into array
                string[] Attributes = EmbeddedObjectAttributes.Split(' ');

                //declare skin object elements
                string AttributeNode = "";
                string Token = "";
                string ControlName = "";

                //iterate and process valid attributes
                string[] Attribute;
                string AttributeName;
                string AttributeValue;
                foreach (string strAttribute in Attributes)
                {
                    if (strAttribute != string.Empty)
                    {
                        Attribute = strAttribute.Split('=');
                        AttributeName = Attribute[0].Trim();
                        AttributeValue = Attribute[1].Trim().Replace("\"", "");
                        switch (AttributeName.ToLower())
                        {
                            case "id":
                                ControlName = AttributeValue;
                                break;
                            case "codetype":
                                AttributeNode = AttributeValue;
                                break;
                            case "codebase":
                                Token = AttributeValue.ToUpper();
                                break;
                        }
                    }
                }

                //process skin object
                if (AttributeNode.ToLower() == "dotnetnuke/server")
                {
                    //we have a valid skin object specification
                    Messages += SkinController.FormatMessage(OBJECT_PROC, Token, 2, false);

                    //if the embedded object is a recognized skin object
                    if (ControlList.ContainsKey(Token) || Token == "CONTENTPANE")
                    {
                        string SkinControl = "";

                        if (ControlList.ContainsKey(Token))
                        {
                            Messages += SkinController.FormatMessage(OBJECT_SKIN, (string)ControlList[Token], 2, false);
                        }
                        else
                        {
                            Messages += SkinController.FormatMessage(OBJECT_PANE, Token, 2, false);
                        }

                        //process embedded object params
                        string Parameters = m.Groups["token"].Value.Substring(m.Groups["token"].Value.IndexOf(">") + 1);
                        Parameters = Parameters.Replace("<param name=\"", "");
                        Parameters = Parameters.Replace("\" value", "");
                        Parameters = Parameters.Replace("/>", "");

                        //convert multiple spaces and carriage returns into single spaces 
                        Parameters = MultiSpaceRegex.Replace(Parameters, " ");

                        if (ControlList.ContainsKey(Token))
                        {
                            //create the skin object user control tag
                            SkinControl = "dnn:" + Token + " runat=\"server\" ";
                            if (!String.IsNullOrEmpty(ControlName))
                            {
                                SkinControl += "id=\"" + ControlName + "\" ";
                            }
                            SkinControl += Parameters;

                            //save control registration statement
                            string ControlRegistration = "<%@ Register TagPrefix=\"dnn\" TagName=\"" + Token + "\" Src=\"~/" + (string)ControlList[Token] + "\" %>" + Environment.NewLine;
                            if (RegisterList.Contains(ControlRegistration) == false)
                            {
                                RegisterList.Add(ControlRegistration);
                            }

                            //return the control statement
                            Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + " /&gt;", 2, false);
                            SkinControl = "<" + SkinControl + "/>";
                        }
                        else
                        {
                            SkinControl = "div runat=\"server\" ";
                            if (!String.IsNullOrEmpty(ControlName))
                            {
                                SkinControl += "id=\"" + ControlName + "\" ";
                            }
                            else
                            {
                                SkinControl += "id=\"ContentPane\" ";
                            }
                            SkinControl += Parameters + "></div";

                            //return the control statement
                            Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + "&gt;", 2, false);
                            SkinControl = "<" + SkinControl + ">";
                        }
                        return SkinControl;
                    }
                    else
                    {
                        //return the unmodified embedded object
                        Messages += SkinController.FormatMessage(OBJECT_NOTFOUND, Token, 2, false);
                        return "<object" + m.Groups["token"].Value + "</object>";
                    }
                }
                else
                {
                    //return unmodified embedded object
                    return "<object" + m.Groups["token"].Value + "</object>";
                }
            }
        }

        #endregion

        #region Nested type: PathParser

        /// -----------------------------------------------------------------------------
        /// Project	 : DotNetNuke
        /// Class	 : SkinFileProcessor.PathParser
        /// 
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Parsing functionality for path replacement in new skin files.
        /// </summary>
        /// <remarks>
        ///     This class encapsulates the data and methods necessary to appropriately
        ///     handle all the path replacement parsing needs for new skin files. Parsing
        ///     supported for CSS syntax and HTML syntax (which covers ASCX files also). 
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private class PathParser
        {
            private readonly string SUBST = Util.GetLocalizedString("Substituting");
            private readonly string SUBST_DETAIL = Util.GetLocalizedString("Substituting.Detail");
            private readonly ArrayList m_CSSPatterns = new ArrayList();
            private readonly ArrayList m_HTMLPatterns = new ArrayList();
            private string m_Messages = "";
            private string m_SkinPath = "";

            private const RegexOptions PatternOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled;

            //retrieve the patterns
            private static readonly Regex[] HtmlArrayPattern =
            {
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
            };

            //retrieve the patterns
            private static readonly Regex[] CssArrayPattern =
            {
                new Regex("(?<tag>\\surl\\u0028)(?<content>[^\\u0029]*)(?<endtag>\\u0029.*;)", PatternOptions)
            };


            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     List of regular expressions for processing HTML syntax.
            /// </summary>
            /// <returns>ArrayList of Regex objects formatted for the Parser method.</returns>
            /// <remarks>
            ///     Additional patterns can be added to this list (if necessary) if properly
            ///     formatted to return <tag/>, <content/> and <endtag/> groups.  For future
            ///     consideration, this list could be imported from a configuration file to
            ///     provide for greater flexibility.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public ArrayList HTMLList
            {
                get
                {
                    //if the arraylist in uninitialized
                    if (m_HTMLPatterns.Count == 0)
                    {
                        //for each pattern, create a regex object
                        m_HTMLPatterns.AddRange(HtmlArrayPattern);

                        //optimize the arraylist size since it will not change
                        m_HTMLPatterns.TrimToSize();
                    }
                    return m_HTMLPatterns;
                }
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     List of regular expressions for processing CSS syntax.
            /// </summary>
            /// <returns>ArrayList of Regex objects formatted for the Parser method.</returns>
            /// <remarks>
            ///     Additional patterns can be added to this list (if necessary) if properly
            ///     formatted to return <tag/>, <content/> and <endtag/> groups.  For future
            ///     consideration, this list could be imported from a configuration file to
            ///     provide for greater flexibility.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public ArrayList CSSList
            {
                get
                {
                    //if the arraylist in uninitialized
                    if (m_CSSPatterns.Count == 0)
                    {
                        //for each pattern, create a regex object
                        m_CSSPatterns.AddRange(CssArrayPattern);

                        //optimize the arraylist size since it will not change
                        m_CSSPatterns.TrimToSize();
                    }
                    return m_CSSPatterns;
                }
            }

            private MatchEvaluator Handler
            {
                get
                {
                    return MatchHandler;
                }
            }

            private string SkinPath
            {
                get
                {
                    return m_SkinPath;
                }
                set
                {
                    m_SkinPath = value;
                }
            }

            private SkinParser ParseOption { get; set; }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Perform parsing on the specified source file.
            /// </summary>
            /// <param name="Source">Pointer to Source string to be parsed.</param>
            /// <param name="RegexList">ArrayList of properly formatted regular expression objects.</param>
            /// <param name="SkinPath">Path to use in replacement operation.</param>
            /// <param name="ParseOption">Parse Opition.</param>
            /// <remarks>
            ///     This procedure iterates through the list of regular expression objects
            ///     and invokes a handler for each match which uses the specified path.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public string Parse(ref string Source, ArrayList RegexList, string SkinPath, SkinParser ParseOption)
            {
                m_Messages = "";

                //set path propery which is file specific
                this.SkinPath = SkinPath;
                //set parse option
                this.ParseOption = ParseOption;

                //process each regular expression
                for (int i = 0; i <= RegexList.Count - 1; i++)
                {
                    Source = ((Regex)RegexList[i]).Replace(Source, Handler);
                }
                return m_Messages;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Process regular expression matches.
            /// </summary>
            /// <param name="m">Regular expression match for path information which requires processing.</param>
            /// <returns>Properly formatted path information.</returns>
            /// <remarks>
            ///     The handler is invoked by the Regex.Replace method once for each match that
            ///     it encounters.  The returned value of the handler is substituted for the
            ///     original match.  So the handler properly formats the path information and
            ///     returns it in favor of the improperly formatted match.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            private string MatchHandler(Match m)
            {
                string strOldTag = m.Groups["tag"].Value + m.Groups["content"].Value + m.Groups["endtag"].Value;
                string strNewTag = strOldTag;

                //we do not want to process object tags to DotNetNuke widgets
                if (!m.Groups[0].Value.ToLower().Contains("codetype=\"dotnetnuke/client\""))
                {
                    switch (ParseOption)
                    {
                        case SkinParser.Localized:
                            //if the tag does not contain the localized path
                            if (strNewTag.IndexOf(SkinPath) == -1)
                            {
                                //insert the localized path
                                strNewTag = m.Groups["tag"].Value + SkinPath + m.Groups["content"].Value + m.Groups["endtag"].Value;
                            }
                            break;
                        case SkinParser.Portable:
                            //if the tag does not contain a reference to the skinpath
                            if (strNewTag.ToLower().IndexOf("<%= skinpath %>") == -1)
                            {
                                //insert the skinpath 
                                strNewTag = m.Groups["tag"].Value + "<%= SkinPath %>" + m.Groups["content"].Value + m.Groups["endtag"].Value;
                            }

                            //if the tag contains the localized path
                            if (strNewTag.IndexOf(SkinPath) != -1)
                            {
                                //remove the localized path
                                strNewTag = strNewTag.Replace(SkinPath, "");
                            }
                            break;
                    }
                }
                m_Messages += SkinController.FormatMessage(SUBST, string.Format(SUBST_DETAIL, HttpUtility.HtmlEncode(strOldTag), HttpUtility.HtmlEncode(strNewTag)), 2, false);
                return strNewTag;
            }
        }

        #endregion

        #region Nested type: SkinFile

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Utility class for processing of skin files.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private class SkinFile
        {
            private readonly string CONTROL_DIR = Util.GetLocalizedString("ControlDirective");
            private readonly string CONTROL_REG = Util.GetLocalizedString("ControlRegister");
            private readonly string FILE_FORMAT_ERROR = Util.GetLocalizedString("FileFormat.Error");
            private readonly string FILE_LOAD = Util.GetLocalizedString("SkinFileLoad");
            private readonly string FILE_LOAD_ERROR = Util.GetLocalizedString("SkinFileLoad.Error");
            private readonly string FILE_WRITE = Util.GetLocalizedString("FileWrite");
            private readonly XmlDocument m_FileAttributes;
            private readonly string m_FileExtension;
            private readonly string m_FileName;
            private readonly string m_SkinRoot;
            private readonly string m_SkinRootPath;
            private readonly string m_WriteFileName;
            private string FILE_FORMAT_DETAIL = Util.GetLocalizedString("FileFormat.Detail");
            private string m_Messages = "";

            private static readonly Regex PaneCheck1Regex = new Regex("\\s*id\\s*=\\s*\"" + Globals.glbDefaultPane + "\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            private static readonly Regex PaneCheck2Regex = new Regex("\\s*[" + Globals.glbDefaultPane + "]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            const string StrPattern = "<\\s*body[^>]*>(?<skin>.*)<\\s*/\\s*body\\s*>";
            private static readonly Regex BodyExtractionRegex = new Regex(StrPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     SkinFile class constructor.
            /// </summary>
            /// <param name="SkinContents"></param>
            /// <param name="SkinAttributes"></param>
            /// <remarks>
            ///     The constructor primes the utility class with basic file information.
            ///     It also checks for the existentce of a skinfile level attribute file
            ///     and read it in, if found.  
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public SkinFile(string SkinContents, XmlDocument SkinAttributes)
            {
                m_FileAttributes = SkinAttributes;
                Contents = SkinContents;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     SkinFile class constructor.
            /// </summary>
            /// <param name="SkinRoot"></param>
            /// <param name="FileName"></param>
            /// <param name="SkinAttributes"></param>
            /// <remarks>
            ///     The constructor primes the utility class with basic file information.
            ///     It also checks for the existentce of a skinfile level attribute file
            ///     and read it in, if found.  
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public SkinFile(string SkinRoot, string FileName, XmlDocument SkinAttributes)
            {
                //capture file information
                m_FileName = FileName;
                m_FileExtension = Path.GetExtension(FileName);
                m_SkinRoot = SkinRoot;
                m_FileAttributes = SkinAttributes;

                //determine and store path to portals skin root folder
                string strTemp = FileName.Replace(Path.GetFileName(FileName), "");
                strTemp = strTemp.Replace("\\", "/");
                m_SkinRootPath = Globals.ApplicationPath + strTemp.Substring(strTemp.ToUpper().IndexOf("/PORTALS"));

                //read file contents
                Contents = Read(FileName);

                //setup some attributes based on file extension
                switch (FileExtension)
                {
                    case ".htm":
                    case ".html":
                        //set output file name to <filename>.ASCX
                        m_WriteFileName = FileName.Replace(Path.GetExtension(FileName), ".ascx");

                        //capture warning if file does not contain a id="ContentPane" or [CONTENTPANE]
                        if (!PaneCheck1Regex.IsMatch(Contents) && !PaneCheck2Regex.IsMatch(Contents))
                        {
                            m_Messages += SkinController.FormatMessage(FILE_FORMAT_ERROR, string.Format(FILE_FORMAT_ERROR, FileName), 2, true);
                        }

                        //Check for existence of and load skin file level attribute information 
                        if (File.Exists(FileName.Replace(FileExtension, ".xml")))
                        {
                            try
                            {
                                m_FileAttributes.Load(FileName.Replace(FileExtension, ".xml"));
                                m_Messages += SkinController.FormatMessage(FILE_LOAD, FileName, 2, false);
                            }
                            catch (Exception exc) //could not load XML file
                            {
                                Logger.Error(exc);
                                m_FileAttributes = SkinAttributes;
                                m_Messages += SkinController.FormatMessage(FILE_LOAD_ERROR, FileName, 2, true);
                            }
                        }
                        break;
                    default:
                        //output file name is same as input file name
                        m_WriteFileName = FileName;
                        break;
                }
            }

            public string SkinRoot
            {
                get
                {
                    return m_SkinRoot;
                }
            }

            public XmlDocument Attributes
            {
                get
                {
                    return m_FileAttributes;
                }
            }

            public string Messages
            {
                get
                {
                    return m_Messages;
                }
            }

            public string FileName
            {
                get
                {
                    return m_FileName;
                }
            }

            public string WriteFileName
            {
                get
                {
                    return m_WriteFileName;
                }
            }

            public string FileExtension
            {
                get
                {
                    return m_FileExtension;
                }
            }

            public string SkinRootPath
            {
                get
                {
                    return m_SkinRootPath;
                }
            }

            public string Contents { get; set; }

            private string Read(string FileName)
            {
                using (var objStreamReader = new StreamReader(FileName))
                {
                    string strFileContents = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                    return strFileContents;
                }
            }

            public void Write()
            {
                //delete the file before attempting to write
                if (File.Exists(WriteFileName))
                {
                    File.Delete(WriteFileName);
                }
                m_Messages += SkinController.FormatMessage(FILE_WRITE, Path.GetFileName(WriteFileName), 2, false);
                using (var objStreamWriter = new StreamWriter(WriteFileName))
                {
                    objStreamWriter.WriteLine(Contents);
                    objStreamWriter.Flush();
                    objStreamWriter.Close();
                }
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            ///     Prepend ascx control directives to file contents.
            /// </summary>
            /// <param name="Registrations">ArrayList of registration directives.</param>
            /// <remarks>
            ///     This procedure formats the @Control directive and prepends it and all
            ///     registration directives to the file contents.
            /// </remarks>
            /// -----------------------------------------------------------------------------
            public string PrependASCXDirectives(ArrayList Registrations)
            {
                string Messages = "";
                string Prefix = "";

                //format and save @Control directive
                Match objMatch = BodyExtractionRegex.Match(Contents);
                if (objMatch.Success && !string.IsNullOrEmpty(objMatch.Groups[1].Value))
                {
                    //if the skin source is an HTML document, extract the content within the <body> tags
                    Contents = objMatch.Groups[1].Value;
                }
                if (SkinRoot == SkinController.RootSkin)
                {
                    Prefix += "<%@ Control language=\"vb\" AutoEventWireup=\"false\" Explicit=\"True\" Inherits=\"DotNetNuke.UI.Skins.Skin\" %>" + Environment.NewLine;
                }
                else if (SkinRoot == SkinController.RootContainer)
                {
                    Prefix += "<%@ Control language=\"vb\" AutoEventWireup=\"false\" Explicit=\"True\" Inherits=\"DotNetNuke.UI.Containers.Container\" %>" + Environment.NewLine;
                }
                Messages += SkinController.FormatMessage(CONTROL_DIR, HttpUtility.HtmlEncode(Prefix), 2, false);

                //add preformatted Control Registrations
                foreach (string Item in Registrations)
                {
                    Messages += SkinController.FormatMessage(CONTROL_REG, HttpUtility.HtmlEncode(Item), 2, false);
                    Prefix += Item;
                }

                //update file contents to include ascx header information
                Contents = Prefix + Contents;
                return Messages;
            }
        }

        #endregion
    }
}