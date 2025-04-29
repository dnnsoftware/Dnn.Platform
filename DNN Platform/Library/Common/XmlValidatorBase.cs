// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common;

using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;

/// <summary>Base class of XmlValidator.</summary>
public class XmlValidatorBase
{
    private readonly XmlSchemaSet schemaSet;
    private ArrayList errs;
    private XmlTextReader reader;

    /// <summary>Initializes a new instance of the <see cref="XmlValidatorBase"/> class.</summary>
    public XmlValidatorBase()
    {
        this.errs = new ArrayList();
        this.schemaSet = new XmlSchemaSet();
    }

    /// <summary>Gets the schema set.</summary>
    public XmlSchemaSet SchemaSet
    {
        get
        {
            return this.schemaSet;
        }
    }

    /// <summary>Gets or sets the errors.</summary>
    /// <value>The errors.</value>
    public ArrayList Errors
    {
        get
        {
            return this.errs;
        }

        set
        {
            this.errs = value;
        }
    }

    /// <summary>Determines whether this instance is valid.</summary>
    /// <returns><see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.</returns>
    public bool IsValid()
    {
        // There is a bug here which I haven't been able to fix.
        // If the XML Instance does not include a reference to the
        // schema, then the validation fails.  If the reference exists
        // the the validation works correctly.

        // Create a validating reader
        var settings = new XmlReaderSettings();
        settings.Schemas = this.schemaSet;
        settings.ValidationType = ValidationType.Schema;

        // Set the validation event handler.
        settings.ValidationEventHandler += this.ValidationCallBack;
        XmlReader vreader = XmlReader.Create(this.reader, settings);

        // Read and validate the XML data.
        while (vreader.Read())
        {
        }

        vreader.Close();
        return this.errs.Count == 0;
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

    /// <summary>Validations the call back.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
    protected void ValidationCallBack(object sender, ValidationEventArgs args)
    {
        this.errs.Add(args.Message);
    }
}
