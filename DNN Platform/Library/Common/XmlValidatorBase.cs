// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>Base class of XmlValidator.</summary>
    public class XmlValidatorBase : IDisposable
    {
        private XmlTextReader reader;

        /// <summary>Initializes a new instance of the <see cref="XmlValidatorBase"/> class.</summary>
        public XmlValidatorBase()
        {
            this.Errors = new ArrayList();
            this.SchemaSet = new XmlSchemaSet();
        }

        /// <summary>Gets the schema set.</summary>
        public XmlSchemaSet SchemaSet { get; }

        /// <summary>Gets or sets the errors.</summary>
        /// <value>The errors.</value>
        public ArrayList Errors { get; set; }

        /// <summary>Determines whether this instance is valid.</summary>
        /// <returns><see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.</returns>
        public bool IsValid()
        {
            // There is a bug here which I haven't been able to fix.
            // If the XML Instance does not include a reference to the
            // schema, then the validation fails.  If the reference exists
            // the validation works correctly.

            // Create a validating reader
            var settings = new XmlReaderSettings();
            settings.Schemas = this.SchemaSet;
            settings.ValidationType = ValidationType.Schema;

            // Set the validation event handler.
            settings.ValidationEventHandler += this.ValidationCallBack;
            XmlReader vreader = XmlReader.Create(this.reader, settings);

            // Read and validate the XML data.
            while (vreader.Read())
            {
            }

            vreader.Close();
            return this.Errors.Count == 0;
        }

        /// <summary>Validates the specified XML stream.</summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <returns><see langword="true"/> if the XML is valid, otherwise <see langword="false"/>.</returns>
        public virtual bool Validate(Stream xmlStream)
        {
            xmlStream.Seek(0, SeekOrigin.Begin);
            this.reader = new XmlTextReader(xmlStream)
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            return this.IsValid();
        }

        /// <summary>Validates the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <returns><see langword="true"/> if the XML is valid, otherwise <see langword="false"/>.</returns>
        public virtual bool Validate(string filename)
        {
            this.reader = new XmlTextReader(filename)
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            return this.IsValid();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.reader?.Dispose();
            }
        }

        /// <summary>Validations the call back.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
        protected void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            this.Errors.Add(args.Message);
        }
    }
}
