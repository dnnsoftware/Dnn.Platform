#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Data;
using System.Xml;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class	 : ControlInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ControlInfo provides a base class for Module Controls and SkinControls
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public abstract class ControlInfo : BaseEntityInfo
    {
        protected ControlInfo()
        {
            SupportsPartialRendering = Null.NullBoolean;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Control Key
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string ControlKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Control Source
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
		public string ControlSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets a flag that determines whether the control support the AJAX
        /// Update Panel
        /// </summary>
        /// <returns>A Boolean</returns>
        /// -----------------------------------------------------------------------------
		public bool SupportsPartialRendering { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ControlInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        protected override void FillInternal(IDataReader dr)
        {
			//Call EntityBaseInfo's implementation
            base.FillInternal(dr);
            ControlKey = Null.SetNullString(dr["ControlKey"]);
            ControlSrc = Null.SetNullString(dr["ControlSrc"]);
            SupportsPartialRendering = Null.SetNullBoolean(dr["SupportsPartialRendering"]);
        }

        protected void ReadXmlInternal(XmlReader reader)
        {
            switch (reader.Name)
            {
                case "controlKey":
                    ControlKey = reader.ReadElementContentAsString();
                    break;
                case "controlSrc":
                    ControlSrc = reader.ReadElementContentAsString();
                    break;
                case "supportsPartialRendering":
                    string elementvalue = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(elementvalue))
                    {
                        SupportsPartialRendering = bool.Parse(elementvalue);
                    }
                    break;
            }
        }

        protected void WriteXmlInternal(XmlWriter writer)
        {
			//write out properties
            writer.WriteElementString("controlKey", ControlKey);
            writer.WriteElementString("controlSrc", ControlSrc);
            writer.WriteElementString("supportsPartialRendering", SupportsPartialRendering.ToString());
        }
    }
}
