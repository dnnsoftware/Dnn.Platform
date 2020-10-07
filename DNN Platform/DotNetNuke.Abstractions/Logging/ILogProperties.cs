// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging
{
    using System;
    using System.Collections;
    using System.Xml;

    /// <summary>
    /// add xml docs.
    /// </summary>
    public interface ILogProperties : IList, ICollection, IEnumerable, ICloneable
    {
        /// <summary>
        /// Gets the Summary.
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// Deserialize.
        /// </summary>
        /// <param name="content">content.</param>
        void Deserialize(string content);

        /// <summary>
        /// Reads the Xml data.
        /// </summary>
        /// <param name="reader">The reader.</param>
        void ReadXml(XmlReader reader);

        /// <summary>
        /// Serialize.
        /// </summary>
        /// <returns>The result.</returns>
        string Serialize();

        /// <summary>
        /// Writes the xml.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        void WriteXml(XmlWriter writer);
    }
}
