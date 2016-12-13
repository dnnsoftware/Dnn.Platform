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
    [Serializable]
    public class FolderPermissionInfo : PermissionInfoBase, IHydratable
    {
		#region "Private Members"
		
        //local property declarations
        private int _folderID;
        private string _folderPath;
        private int _folderPermissionID;
        private int _portalID;
		
		#endregion
		
		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new FolderPermissionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public FolderPermissionInfo()
        {
            _folderPermissionID = Null.NullInteger;
            _folderPath = Null.NullString;
            _portalID = Null.NullInteger;
            _folderID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new FolderPermissionInfo
        /// </summary>
        /// <param name="permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public FolderPermissionInfo(PermissionInfo permission) : this()
        {
            ModuleDefID = permission.ModuleDefID;
            PermissionCode = permission.PermissionCode;
            PermissionID = permission.PermissionID;
            PermissionKey = permission.PermissionKey;
            PermissionName = permission.PermissionName;
        }
		
		#endregion
		
		#region "Public Properties"

        [XmlIgnore]
        public int FolderPermissionID
        {
            get
            {
                return _folderPermissionID;
            }
            set
            {
                _folderPermissionID = value;
            }
        }

        [XmlIgnore]
        public int FolderID
        {
            get
            {
                return _folderID;
            }
            set
            {
                _folderID = value;
            }
        }

        [XmlIgnore]
        public int PortalID
        {
            get
            {
                return _portalID;
            }
            set
            {
                _portalID = value;
            }
        }

        [XmlElement("folderpath")]
        public string FolderPath
        {
            get
            {
                return _folderPath;
            }
            set
            {
                _folderPath = value;
            }
        }
		
		#endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a FolderPermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);
            FolderPermissionID = Null.SetNullInteger(dr["FolderPermissionID"]);
            FolderID = Null.SetNullInteger(dr["FolderID"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            FolderPath = Null.SetNullString(dr["FolderPath"]);
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
                return FolderPermissionID;
            }
            set
            {
                FolderPermissionID = value;
            }
        }

        #endregion
    }
}
