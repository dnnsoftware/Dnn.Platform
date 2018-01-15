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