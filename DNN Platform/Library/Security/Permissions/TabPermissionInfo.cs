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
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : TabPermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// TabPermissionInfo provides the Entity Layer for Tab Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    [XmlRoot("permission")]
    public class TabPermissionInfo : PermissionInfoBase, IHydratable
    {
		#region "Private Members"
		
        private int _TabID;
        //local property declarations
		private int _TabPermissionID;
		
		#endregion
		
		#region "Constructors"

         /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new TabPermissionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
       public TabPermissionInfo()
        {
            _TabPermissionID = Null.NullInteger;
            _TabID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new TabPermissionInfo
        /// </summary>
        /// <param name="permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public TabPermissionInfo(PermissionInfo permission) : this()
        {
            ModuleDefID = permission.ModuleDefID;
            PermissionCode = permission.PermissionCode;
            PermissionID = permission.PermissionID;
            PermissionKey = permission.PermissionKey;
            PermissionName = permission.PermissionName;
        }
		
		#endregion
		
		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Tab Permission ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("tabpermissionid")]
        public int TabPermissionID
        {
            get
            {
                return _TabPermissionID;
            }
            set
            {
                _TabPermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Tab ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("tabid")]
        public int TabID
        {
            get
            {
                return _TabID;
            }
            set
            {
                _TabID = value;
            }
        }
		
		#endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a TabPermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            //Call the base classes fill method to ppoulate base class proeprties
			base.FillInternal(dr);
            TabPermissionID = Null.SetNullInteger(dr["TabPermissionID"]);
            TabID = Null.SetNullInteger(dr["TabID"]);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return TabPermissionID;
            }
            set
            {
                TabPermissionID = value;
            }
        }

        #endregion
    }
}
