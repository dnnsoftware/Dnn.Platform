// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Base class od XmlValidator.
    /// </summary>
    public class XmlValidatorBase
    {
        private readonly XmlSchemaSet _schemaSet;
        private ArrayList _errs;
        private XmlTextReader _reader;

        public XmlValidatorBase()
        {
            this._errs = new ArrayList();
            this._schemaSet = new XmlSchemaSet();
        }

        /// <summary>
        /// Gets the schema set.
        /// </summary>
        public XmlSchemaSet SchemaSet
        {
            get
            {
                return this._schemaSet;
            }
        }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public ArrayList Errors
        {
            get
            {
                return this._errs;
            }

            set
            {
                this._errs = value;
            }
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            // There is a bug here which I haven't been able to fix.
            // If the XML Instance does not include a reference to the
            // schema, then the validation fails.  If the reference exists
            // the the validation works correctly.

            // Create a validating reader
            var settings = new XmlReaderSettings();
            settings.Schemas = this._schemaSet;
            settings.ValidationType = ValidationType.Schema;

            // Set the validation event handler.
            settings.ValidationEventHandler += this.ValidationCallBack;
            XmlReader vreader = XmlReader.Create(this._reader, settings);

            // Read and validate the XML data.
            while (vreader.Read())
            {
            }

            vreader.Close();
            return this._errs.Count == 0;
        }

        /// <summary>
        /// Validates the specified XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <returns></returns>
        public virtual bool Validate(Stream xmlStream)
        {
            xmlStream.Seek(0, SeekOrigin.Begin);
            this._reader = new XmlTextReader(xmlStream)
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            return this.IsValid();
        }

        /// <summary>
        /// Validates the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public virtual bool Validate(string filename)
        {
            this._reader = new XmlTextReader(filename)
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            return this.IsValid();
        }

        /// <summary>
        /// Validations the call back.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
        protected void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            this._errs.Add(args.Message);
        }
    }
}
