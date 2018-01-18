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

using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;

#endregion

namespace DotNetNuke.Common
{
	/// <summary>
	/// Base class od XmlValidator
	/// </summary>
    public class XmlValidatorBase
    {
        private readonly XmlSchemaSet _schemaSet;
        private ArrayList _errs;
        private XmlTextReader _reader;

        public XmlValidatorBase()
        {
            _errs = new ArrayList();
            _schemaSet = new XmlSchemaSet();
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
                return _errs;
            }
            set
            {
                _errs = value;
            }
        }

		/// <summary>
		/// Gets the schema set.
		/// </summary>
        public XmlSchemaSet SchemaSet
        {
            get
            {
                return _schemaSet;
            }
        }

		/// <summary>
		/// Validations the call back.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
        protected void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            _errs.Add(args.Message);
        }

		/// <summary>
		/// Determines whether this instance is valid.
		/// </summary>
		/// <returns>
		///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
		/// </returns>
        public bool IsValid()
        {
			//There is a bug here which I haven't been able to fix.
            //If the XML Instance does not include a reference to the
            //schema, then the validation fails.  If the reference exists
            //the the validation works correctly.

            //Create a validating reader
            var settings = new XmlReaderSettings();
            settings.Schemas = _schemaSet;
            settings.ValidationType = ValidationType.Schema;
            //Set the validation event handler.
            settings.ValidationEventHandler += ValidationCallBack;
            XmlReader vreader = XmlReader.Create(_reader, settings);
            //Read and validate the XML data.
            while (vreader.Read())
            {
            }
            vreader.Close();
            return (_errs.Count == 0);
        }

		/// <summary>
		/// Validates the specified XML stream.
		/// </summary>
		/// <param name="xmlStream">The XML stream.</param>
		/// <returns></returns>
        public virtual bool Validate(Stream xmlStream)
        {
            xmlStream.Seek(0, SeekOrigin.Begin);
            _reader = new XmlTextReader(xmlStream) { DtdProcessing = DtdProcessing.Prohibit };
            return IsValid();
        }

		/// <summary>
		/// Validates the specified filename.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
        public virtual bool Validate(string filename)
        {
            _reader = new XmlTextReader(filename) { DtdProcessing = DtdProcessing.Prohibit };
            return IsValid();
        }
    }
}