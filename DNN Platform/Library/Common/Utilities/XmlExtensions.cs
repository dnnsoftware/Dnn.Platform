// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    /// <summary>
    /// The XmlExtensions class allows you to write more efficient code to manage Xml documents.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Adds an element to the specified XmlNode.
        /// </summary>
        /// <param name="node">The node to add the element to.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="elementValue">The element value.</param>
        /// <returns>
        /// The added element.
        /// </returns>
        public static XmlNode AddElement(this XmlNode node, string elementName, string elementValue)
        {
            return node.AddElement(elementName, elementValue, false);
        }

        /// <summary>
        /// Adds an element to the specified XmlNode.
        /// </summary>
        /// <param name="node">The node to add the element to.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="elementValue">The element value.</param>
        /// <param name="useCData">if set to <c>true</c> use a CData encapsulation.</param>
        /// <returns>
        /// The added element.
        /// </returns>
        public static XmlNode AddElement(this XmlNode node, string elementName, string elementValue, bool useCData)
        {
            XmlNode newElement = node.OwnerDocument.CreateElement(elementName);
            if (useCData)
            {
                XmlCDataSection cData = node.OwnerDocument.CreateCDataSection(elementValue);
                newElement.AppendChild(cData);
            }
            else
            {
                newElement.InnerText = elementValue;
            }

            node.AppendChild(newElement);
            return newElement;
        }

        /// <summary>
        /// Adds an element to the specified XmlNode using the specified namespace.
        /// </summary>
        /// <param name="node">The node to add the element to.</param>
        /// <param name="elementName">Name of the element (without the abbreviated prefix).</param>
        /// <param name="elementValue">The element value.</param>
        /// <param name="useCData">if set to <c>true</c> use a CData encapsulation.</param>
        /// <param name="namespaceUri">The namespace URI.</param>
        /// <param name="namespaceAbbr">The namespace abbreviation.</param>
        /// <returns>
        /// The added node.
        /// </returns>
        public static XmlNode AddElement(this XmlNode node, string elementName, string elementValue, bool useCData, string namespaceUri, string namespaceAbbr)
        {
            XmlNode newElement = node.OwnerDocument.CreateElement(namespaceAbbr + ":" + elementName, namespaceUri);
            if (useCData)
            {
                XmlCDataSection cData = node.OwnerDocument.CreateCDataSection(elementValue);
                newElement.AppendChild(cData);
            }
            else
            {
                newElement.InnerText = elementValue;
            }

            node.AppendChild(newElement);
            return newElement;
        }

        /// <summary>
        /// Adds an attribute to the specified node.
        /// </summary>
        /// <param name="node">The node to add the attribute to.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>The node the attribute was added to.</returns>
        public static XmlNode AddAttribute(this XmlNode node, string attributeName, string attributeValue)
        {
            XmlAttribute newAttribute = node.OwnerDocument.CreateAttribute(attributeName);
            newAttribute.InnerText = attributeValue;
            node.Attributes.Append(newAttribute);
            return node;
        }

        /// <summary>
        /// Adds an attribute to an XmlNode using the specified namespace.
        /// </summary>
        /// <param name="node">The node to add the attribtue to.</param>
        /// <param name="attributeName">Name of the attribute without the namespace abbreviation prefix.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="namespaceUri">The namespace URI.</param>
        /// <param name="namespaceAbbr">The namespace abbreviation.</param>
        /// <returns>The node the attribute was added to.</returns>
        public static XmlNode AddAttribute(this XmlNode node, string attributeName, string attributeValue, string namespaceUri, string namespaceAbbr)
        {
            XmlAttribute newAttribute = node.OwnerDocument.CreateAttribute(namespaceAbbr + ":" + attributeName, namespaceUri);
            newAttribute.InnerText = attributeValue;
            node.Attributes.Append(newAttribute);
            return node;
        }
    }
}
