// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.UI.Skins
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : SkinPackageInfo
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Handles the Business Object for Skins
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class SkinPackageInfo : BaseEntityInfo, IHydratable
    {
		#region "Private Members"

        private int _PackageID = Null.NullInteger;
        private int _PortalID = Null.NullInteger;
        private string _SkinName;
        private int _SkinPackageID = Null.NullInteger;
        private string _SkinType;
        private Dictionary<int, string> _Skins = new Dictionary<int, string>();
		
		#endregion

		#region "Public Properties"

        public int PackageID
        {
            get
            {
                return _PackageID;
            }
            set
            {
                _PackageID = value;
            }
        }

        public int SkinPackageID
        {
            get
            {
                return _SkinPackageID;
            }
            set
            {
                _SkinPackageID = value;
            }
        }

        public int PortalID
        {
            get
            {
                return _PortalID;
            }
            set
            {
                _PortalID = value;
            }
        }

        public string SkinName
        {
            get
            {
                return _SkinName;
            }
            set
            {
                _SkinName = value;
            }
        }

        [XmlIgnore]
        public Dictionary<int, string> Skins
        {
            get
            {
                return _Skins;
            }
            set
            {
                _Skins = value;
            }
        }

        public string SkinType
        {
            get
            {
                return _SkinType;
            }
            set
            {
                _SkinType = value;
            }
        }
		
		#endregion

        #region IHydratable Members

        public void Fill(IDataReader dr)
        {
            SkinPackageID = Null.SetNullInteger(dr["SkinPackageID"]);
            PackageID = Null.SetNullInteger(dr["PackageID"]);
            SkinName = Null.SetNullString(dr["SkinName"]);
            SkinType = Null.SetNullString(dr["SkinType"]);
            //Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);

            if (dr.NextResult())
            {
                while (dr.Read())
                {
                    int skinID = Null.SetNullInteger(dr["SkinID"]);
                    if (skinID > Null.NullInteger)
                    {
                        _Skins[skinID] = Null.SetNullString(dr["SkinSrc"]);
                    }
                }
            }
        }

        public int KeyID
        {
            get
            {
                return SkinPackageID;
            }
            set
            {
                SkinPackageID = value;
            }
        }

        #endregion
    }
}
