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
                return this._LanguagePackID;
            }
            set
            {
                this._LanguagePackID = value;
            }
        }

        public int LanguageID
        {
            get
            {
                return this._LanguageID;
            }
            set
            {
                this._LanguageID = value;
            }
        }

        public int PackageID
        {
            get
            {
                return this._PackageID;
            }
            set
            {
                this._PackageID = value;
            }
        }

        public int DependentPackageID
        {
            get
            {
                return this._DependentPackageID;
            }
            set
            {
                this._DependentPackageID = value;
            }
        }

        public LanguagePackType PackageType
        {
            get
            {
                if (this.DependentPackageID == -2)
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
            this.LanguagePackID = Null.SetNullInteger(dr["LanguagePackID"]);
            this.LanguageID = Null.SetNullInteger(dr["LanguageID"]);
            this.PackageID = Null.SetNullInteger(dr["PackageID"]);
            this.DependentPackageID = Null.SetNullInteger(dr["DependentPackageID"]);
            //Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);
        }

        public int KeyID
        {
            get
            {
                return this.LanguagePackID;
            }
            set
            {
                this.LanguagePackID = value;
            }
        }

        #endregion
    }
}
