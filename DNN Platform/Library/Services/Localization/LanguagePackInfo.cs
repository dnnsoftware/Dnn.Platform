// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Localization
{
    [Serializable]
    public class LanguagePackInfo : BaseEntityInfo, IHydratable
    {
		#region "Private Members"

        private int _DependentPackageID = Null.NullInteger;
        private int _LanguageID = Null.NullInteger;
        private int _LanguagePackID = Null.NullInteger;
        private int _PackageID = Null.NullInteger;

		#endregion

		#region "Public Properties"

        public int LanguagePackID
        {
            get
            {
                return _LanguagePackID;
            }
            set
            {
                _LanguagePackID = value;
            }
        }

        public int LanguageID
        {
            get
            {
                return _LanguageID;
            }
            set
            {
                _LanguageID = value;
            }
        }

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

        public int DependentPackageID
        {
            get
            {
                return _DependentPackageID;
            }
            set
            {
                _DependentPackageID = value;
            }
        }

        public LanguagePackType PackageType
        {
            get
            {
                if (DependentPackageID == -2)
                {
                    return LanguagePackType.Core;
                }
                else
                {
                    return LanguagePackType.Extension;
                }
            }
        }
		
		#endregion

        #region IHydratable Members

        public void Fill(IDataReader dr)
        {
            LanguagePackID = Null.SetNullInteger(dr["LanguagePackID"]);
            LanguageID = Null.SetNullInteger(dr["LanguageID"]);
            PackageID = Null.SetNullInteger(dr["PackageID"]);
            DependentPackageID = Null.SetNullInteger(dr["DependentPackageID"]);
            //Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);
        }

        public int KeyID
        {
            get
            {
                return LanguagePackID;
            }
            set
            {
                LanguagePackID = value;
            }
        }

        #endregion
    }
}
