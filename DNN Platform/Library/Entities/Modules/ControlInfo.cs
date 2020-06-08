// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
