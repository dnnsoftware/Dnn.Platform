// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The XmlUtils class provides Shared/Static methods for manipulating xml files.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class XmlUtils
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(XmlUtils));

        public static void AppendElement(ref XmlDocument objDoc, XmlNode objNode, string attName, string attValue, bool includeIfEmpty)
        {
            AppendElement(ref objDoc, objNode, attName, attValue, includeIfEmpty, false);
        }

        public static void AppendElement(ref XmlDocument objDoc, XmlNode objNode, string attName, string attValue, bool includeIfEmpty, bool cdata)
        {
            if (string.IsNullOrEmpty(attValue) && !includeIfEmpty)
            {
                return;
            }

            if (cdata)
            {
                objNode.AppendChild(CreateCDataElement(objDoc, attName, attValue));
            }
            else
            {
                objNode.AppendChild(CreateElement(objDoc, attName, attValue));
            }
        }

        public static XmlAttribute CreateAttribute(XmlDocument objDoc, string attName, string attValue)
        {
            XmlAttribute attribute = objDoc.CreateAttribute(attName);
            attribute.Value = attValue;
            return attribute;
        }

        public static void CreateAttribute(XmlDocument objDoc, XmlNode objNode, string attName, string attValue)
        {
            XmlAttribute attribute = objDoc.CreateAttribute(attName);
            attribute.Value = attValue;
            objNode.Attributes.Append(attribute);
        }

        public static XmlElement CreateElement(XmlDocument document, string nodeName, string nodeValue)
        {
            XmlElement element = document.CreateElement(nodeName);
            element.InnerText = nodeValue;
            return element;
        }

        public static XmlElement CreateCDataElement(XmlDocument document, string nodeName, string nodeValue)
        {
            XmlElement element = document.CreateElement(nodeName);
            element.AppendChild(document.CreateCDataSection(nodeValue));
            return element;
        }

        public static object Deserialize(Stream objStream, Type type)
        {
            object obj = Activator.CreateInstance(type);
            var tabDic = obj as Dictionary<int, TabInfo>;
            if (tabDic != null)
            {
                obj = DeSerializeDictionary<TabInfo>(objStream, "dictionary");
                return obj;
            }

            var moduleDic = obj as Dictionary<int, ModuleInfo>;
            if (moduleDic != null)
            {
                obj = DeSerializeDictionary<ModuleInfo>(objStream, "dictionary");
                return obj;
            }

            var tabPermDic = obj as Dictionary<int, TabPermissionCollection>;
            if (tabPermDic != null)
            {
                obj = DeSerializeDictionary<TabPermissionCollection>(objStream, "dictionary");
                return obj;
            }

            var modPermDic = obj as Dictionary<int, ModulePermissionCollection>;
            if (modPermDic != null)
            {
                obj = DeSerializeDictionary<ModulePermissionCollection>(objStream, "dictionary");
                return obj;
            }

            var serializer = new XmlSerializer(type);
            using (TextReader tr = new StreamReader(objStream))
            {
                obj = serializer.Deserialize(tr);
                tr.Close();
                return obj;
            }
        }

        public static Dictionary<int, TValue> DeSerializeDictionary<TValue>(Stream objStream, string rootname)
        {
            var xmlDoc = new XmlDocument { XmlResolver = null };
            xmlDoc.Load(objStream);

            var objDictionary = new Dictionary<int, TValue>();

            foreach (XmlElement xmlItem in xmlDoc.SelectNodes(rootname + "/item"))
            {
                int key = Convert.ToInt32(xmlItem.GetAttribute("key"));

                var objValue = Activator.CreateInstance<TValue>();

                // Create the XmlSerializer
                var xser = new XmlSerializer(objValue.GetType());

                // A reader is needed to read the XML document.
                var reader = new XmlTextReader(new StringReader(xmlItem.InnerXml))
                {
                    XmlResolver = null,
                    DtdProcessing = DtdProcessing.Prohibit,
                };

                // Use the Deserialize method to restore the object's state, and store it
                // in the Hashtable
                objDictionary.Add(key, (TValue)xser.Deserialize(reader));
            }

            return objDictionary;
        }

        public static Hashtable DeSerializeHashtable(string xmlSource, string rootname)
        {
            var hashTable = new Hashtable();

            if (!string.IsNullOrEmpty(xmlSource))
            {
                try
                {
                    var xmlDoc = new XmlDocument { XmlResolver = null };
                    xmlDoc.LoadXml(xmlSource);

                    foreach (XmlElement xmlItem in xmlDoc.SelectNodes(rootname + "/item"))
                    {
                        string key = xmlItem.GetAttribute("key");
                        string typeName = xmlItem.GetAttribute("type");

                        // Create the XmlSerializer
                        var xser = new XmlSerializer(Type.GetType(typeName));

                        // A reader is needed to read the XML document.
                        var reader = new XmlTextReader(new StringReader(xmlItem.InnerXml))
                        {
                            XmlResolver = null,
                            DtdProcessing = DtdProcessing.Prohibit,
                        };

                        // Use the Deserialize method to restore the object's state, and store it
                        // in the Hashtable
                        hashTable.Add(key, xser.Deserialize(reader));
                    }
                }
                catch (Exception)
                {
                    // Logger.Error(ex); /*Ignore Log because if failed on profile this will log on every request.*/
                }
            }

            return hashTable;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of an attribute.
        /// </summary>
        /// <param name="nav">Parent XPathNavigator.</param>
        /// <param name="attributeName">Thename of the Attribute.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static string GetAttributeValue(XPathNavigator nav, string attributeName)
        {
            return nav.GetAttribute(attributeName, string.Empty);
        }

        public static bool GetAttributeValueAsBoolean(XPathNavigator navigator, string attributeName, bool defaultValue)
        {
            bool boolValue = defaultValue;
            string strValue = GetAttributeValue(navigator, attributeName);
            if (!string.IsNullOrEmpty(strValue))
            {
                boolValue = Convert.ToBoolean(strValue);
            }

            return boolValue;
        }

        public static int GetAttributeValueAsInteger(XPathNavigator navigator, string attributeName, int defaultValue)
        {
            int intValue = defaultValue;
            string strValue = GetAttributeValue(navigator, attributeName);
            if (!string.IsNullOrEmpty(strValue))
            {
                intValue = Convert.ToInt32(strValue);
            }

            return intValue;
        }

        public static long GetAttributeValueAsLong(XPathNavigator navigator, string attributeName, long defaultValue)
        {
            long intValue = defaultValue;

            string strValue = GetAttributeValue(navigator, attributeName);
            if (!string.IsNullOrEmpty(strValue))
            {
                intValue = Convert.ToInt64(strValue);
            }

            return intValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="string"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <see cref="string.Empty"/> if the node doesn't exist or doesn't have a value.</returns>
        public static string GetNodeValue(XPathNavigator navigator, string path)
        {
            return GetNodeValue(navigator, path, string.Empty);
        }

        /// <summary>Gets the value of a child node as a <see cref="string"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        public static string GetNodeValue(XPathNavigator navigator, string path, string defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            var strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue) && !string.IsNullOrEmpty(defaultValue))
            {
                return defaultValue;
            }

            return strValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value will be returned.
        /// </remarks>
        public static string GetNodeValue(XmlNode objNode, string nodeName)
        {
            return GetNodeValue(objNode, nodeName, string.Empty);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string GetNodeValue(XmlNode objNode, string nodeName, string defaultValue)
        {
            string strValue = defaultValue;
            if (objNode[nodeName] != null)
            {
                strValue = objNode[nodeName].InnerText;
                if (string.IsNullOrEmpty(strValue) && !string.IsNullOrEmpty(defaultValue))
                {
                    strValue = defaultValue;
                }
            }

            return strValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value (False) will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool GetNodeValueBoolean(XmlNode objNode, string nodeName)
        {
            return GetNodeValueBoolean(objNode, nodeName, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool GetNodeValueBoolean(XmlNode objNode, string nodeName, bool defaultValue)
        {
            bool bValue = defaultValue;
            if (objNode[nodeName] != null)
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    bValue = Convert.ToBoolean(strValue);
                }
            }

            return bValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="bool"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <c>false</c> if the node doesn't exist or doesn't have a value.</returns>
        public static bool GetNodeValueBoolean(XPathNavigator navigator, string path)
        {
            return GetNodeValueBoolean(navigator, path, false);
        }

        /// <summary>Gets the value of a child node as a <see cref="bool"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        public static bool GetNodeValueBoolean(XPathNavigator navigator, string path, bool defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            return Convert.ToBoolean(strValue);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DateTime GetNodeValueDate(XmlNode objNode, string nodeName, DateTime defaultValue)
        {
            DateTime dateValue = defaultValue;
            if (objNode[nodeName] != null)
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    dateValue = Convert.ToDateTime(strValue);
                    if (dateValue.Date.Equals(Null.NullDate.Date))
                    {
                        dateValue = Null.NullDate;
                    }
                }
            }

            return dateValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="DateTime"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        public static DateTime GetNodeValueDate(XPathNavigator navigator, string path, DateTime defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            var dateValue = Convert.ToDateTime(strValue);
            if (dateValue.Date.Equals(Null.NullDate.Date))
            {
                return Null.NullDate;
            }

            return dateValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value (0) will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int GetNodeValueInt(XmlNode node, string nodeName)
        {
            return GetNodeValueInt(node, nodeName, 0);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static int GetNodeValueInt(XmlNode node, string nodeName, int defaultValue)
        {
            int intValue = defaultValue;
            if (node[nodeName] != null)
            {
                string strValue = node[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    intValue = Convert.ToInt32(strValue);
                }
            }

            return intValue;
        }

        /// <summary>Gets the value of a child node as an <see cref="int"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <c>0</c> if the node doesn't exist or doesn't have a value.</returns>
        public static int GetNodeValueInt(XPathNavigator navigator, string path)
        {
            return GetNodeValueInt(navigator, path, 0);
        }

        /// <summary>Gets the value of a child node as an <see cref="int"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        public static int GetNodeValueInt(XPathNavigator navigator, string path, int defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            return Convert.ToInt32(strValue);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value (0) will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static float GetNodeValueSingle(XmlNode node, string nodeName)
        {
            return GetNodeValueSingle(node, nodeName, 0);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of node.
        /// </summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the node does not exist or it causes any error the default value will be returned.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static float GetNodeValueSingle(XmlNode node, string nodeName, float defaultValue)
        {
            float sValue = defaultValue;
            if (node[nodeName] != null)
            {
                string strValue = node[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    sValue = Convert.ToSingle(strValue, CultureInfo.InvariantCulture);
                }
            }

            return sValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="float"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <c>0</c> if the node doesn't exist or doesn't have a value.</returns>
        public static float GetNodeValueSingle(XPathNavigator navigator, string path)
        {
            return GetNodeValueSingle(navigator, path, 0);
        }

        /// <summary>Gets the value of a child node as a <see cref="float"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        public static float GetNodeValueSingle(XPathNavigator navigator, string path, float defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            return Convert.ToSingle(strValue, CultureInfo.InvariantCulture);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlWriterSettings object.
        /// </summary>
        /// <param name="conformance">Conformance Level.</param>
        /// <returns>An XmlWriterSettings.</returns>
        /// -----------------------------------------------------------------------------
        public static XmlWriterSettings GetXmlWriterSettings(ConformanceLevel conformance)
        {
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = conformance;
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            return settings;
        }

        public static string SerializeDictionary(IDictionary source, string rootName)
        {
            string strString;
            if (source.Count != 0)
            {
                XmlSerializer xser;
                StringWriter sw;

                var xmlDoc = new XmlDocument { XmlResolver = null };
                XmlElement xmlRoot = xmlDoc.CreateElement(rootName);
                xmlDoc.AppendChild(xmlRoot);

                foreach (var key in source.Keys)
                {
                    // Create the item Node
                    XmlElement xmlItem = xmlDoc.CreateElement("item");

                    // Save the key name and the object type
                    xmlItem.SetAttribute("key", Convert.ToString(key));
                    xmlItem.SetAttribute("type", source[key].GetType().AssemblyQualifiedName);

                    // Serialize the object
                    var xmlObject = new XmlDocument { XmlResolver = null };
                    xser = new XmlSerializer(source[key].GetType());
                    sw = new StringWriter();
                    xser.Serialize(sw, source[key]);
                    xmlObject.LoadXml(sw.ToString());

                    // import and append the node to the root
                    xmlItem.AppendChild(xmlDoc.ImportNode(xmlObject.DocumentElement, true));
                    xmlRoot.AppendChild(xmlItem);
                }

                // Return the OuterXml of the profile
                strString = xmlDoc.OuterXml;
            }
            else
            {
                strString = string.Empty;
            }

            return strString;
        }

        public static void SerializeHashtable(Hashtable hashtable, XmlDocument xmlDocument, XmlNode rootNode, string elementName, string keyField, string valueField)
        {
            XmlNode nodeSetting;
            XmlNode nodeSettingName;
            XmlNode nodeSettingValue;

            string outerElementName = elementName + "s";
            string innerElementName = elementName;

            XmlNode nodeSettings = rootNode.AppendChild(xmlDocument.CreateElement(outerElementName));
            foreach (string key in hashtable.Keys)
            {
                nodeSetting = nodeSettings.AppendChild(xmlDocument.CreateElement(innerElementName));
                nodeSettingName = nodeSetting.AppendChild(xmlDocument.CreateElement(keyField));
                nodeSettingName.InnerText = key;
                nodeSettingValue = nodeSetting.AppendChild(xmlDocument.CreateElement(valueField));
                nodeSettingValue.InnerText = hashtable[key].ToString();
            }
        }

        public static void UpdateAttribute(XmlNode node, string attName, string attValue)
        {
            if (node != null)
            {
                XmlAttribute attrib = node.Attributes[attName];
                attrib.InnerText = attValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  Xml Encodes HTML.
        /// </summary>
        /// <param name = "html">The HTML to encode.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static string XMLEncode(string html)
        {
            return "<![CDATA[" + html + "]]>";
        }

        /// <summary>
        /// Removes control characters and other non-UTF-8 characters.
        /// </summary>
        /// <param name="content">The string to process.</param>
        /// <returns>A string with no control characters or entities above 0x00FD.</returns>
        public static string RemoveInvalidXmlCharacters(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            StringBuilder newString = new StringBuilder();
            foreach (var ch in content)
            {
                if (XmlConvert.IsXmlChar(ch))
                {
                    newString.Append(ch);
                }
            }

            return newString.ToString();
        }

        public static void XSLTransform(XmlDocument doc, ref StreamWriter writer, string xsltUrl)
        {
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltUrl);

            // Transform the file.
            xslt.Transform(doc, null, writer);
        }

        public static string Serialize(object obj)
        {
            string xmlObject;
            var dic = obj as IDictionary;
            if (dic != null)
            {
                xmlObject = SerializeDictionary(dic, "dictionary");
            }
            else
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                var xser = new XmlSerializer(obj.GetType());
                var sw = new StringWriter();

                xser.Serialize(sw, obj);

                xmlDoc.LoadXml(sw.GetStringBuilder().ToString());
                XmlNode xmlDocEl = xmlDoc.DocumentElement;
                xmlDocEl.Attributes.Remove(xmlDocEl.Attributes["xmlns:xsd"]);
                xmlDocEl.Attributes.Remove(xmlDocEl.Attributes["xmlns:xsi"]);

                xmlObject = xmlDocEl.OuterXml;
            }

            return xmlObject;
        }

        /// <summary>
        /// Produce an XPath literal equal to the value if possible; if not, produce
        /// an XPath expression that will match the value.
        ///
        /// Note that this function will produce very long XPath expressions if a value
        /// contains a long run of double quotes.
        /// </summary>
        /// <param name="value">The value to match.</param>
        /// <returns>If the value contains only single or double quotes, an XPath
        /// literal equal to the value.  If it contains both, an XPath expression,
        /// using concat(), that evaluates to the value.</returns>
        /// <remarks>From Stack Overflow (<see href="http://stackoverflow.com/a/1352556/2688"/>).</remarks>
        public static string XPathLiteral(string value)
        {
            // if the value contains only single or double quotes, construct
            // an XPath literal
            if (!value.Contains("\""))
            {
                return "\"" + value + "\"";
            }

            if (!value.Contains("'"))
            {
                return "'" + value + "'";
            }

            // if the value contains both single and double quotes, construct an
            // expression that concatenates all non-double-quote substrings with
            // the quotes, e.g.:
            //
            //    concat("foo", '"', "bar")
            StringBuilder sb = new StringBuilder();
            sb.Append("concat(");
            string[] substrings = value.Split('\"');
            for (int i = 0; i < substrings.Length; i++)
            {
                bool needComma = i > 0;
                if (substrings[i] != string.Empty)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("\"");
                    sb.Append(substrings[i]);
                    sb.Append("\"");
                    needComma = true;
                }

                if (i < substrings.Length - 1)
                {
                    if (needComma)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("'\"'");
                }
            }

            sb.Append(")");
            return sb.ToString();
        }

        [Obsolete("This method is obsolete. Use .Net XmlDocument.Load instead. Scheduled removal in v11.0.0.")]
        public static XmlDocument GetXMLContent(string contentUrl)
        {
            // This function reads an Xml document via a Url and returns it as an XmlDocument object
            var functionReturnValue = new XmlDocument { XmlResolver = null };
            var req = WebRequest.Create(contentUrl);
            var result = req.GetResponse();
            var objXmlReader = new XmlTextReader(result.GetResponseStream())
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            functionReturnValue.Load(objXmlReader);
            return functionReturnValue;
        }
    }
}
