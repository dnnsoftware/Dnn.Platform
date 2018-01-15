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

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationInfo class provides the Entity Layer for the 
    /// Authentication Systems.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class AuthenticationInfo : BaseEntityInfo, IHydratable
    {
		#region Private Members

        public AuthenticationInfo()
        {
            LogoffControlSrc = Null.NullString;
            LoginControlSrc = Null.NullString;
            SettingsControlSrc = Null.NullString;
            AuthenticationType = Null.NullString;
            AuthenticationID = Null.NullInteger;
        }

        #endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the ID of the Authentication System
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int AuthenticationID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the PackageID for the Authentication System
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets a flag that determines whether the Authentication System is enabled
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsEnabled { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the type (name) of the Authentication System (eg DNN, OpenID, LiveID)
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the url for the Settings Control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string SettingsControlSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the url for the Login Control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string LoginControlSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the url for the Logoff Control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string LogoffControlSrc { get; set; }

        #endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a RoleInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public virtual void Fill(IDataReader dr)
        {
            AuthenticationID = Null.SetNullInteger(dr["AuthenticationID"]);
            PackageID = Null.SetNullInteger(dr["PackageID"]);
            IsEnabled = Null.SetNullBoolean(dr["IsEnabled"]);
            AuthenticationType = Null.SetNullString(dr["AuthenticationType"]);
            SettingsControlSrc = Null.SetNullString(dr["SettingsControlSrc"]);
            LoginControlSrc = Null.SetNullString(dr["LoginControlSrc"]);
            LogoffControlSrc = Null.SetNullString(dr["LogoffControlSrc"]);

            //Fill base class fields
            FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public virtual int KeyID
        {
            get
            {
                return AuthenticationID;
            }
            set
            {
                AuthenticationID = value;
            }
        }

        #endregion
    }
}
