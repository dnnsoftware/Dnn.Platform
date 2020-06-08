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
