// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationInfo class provides the Entity Layer for the
    /// Authentication Systems.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class AuthenticationInfo : BaseEntityInfo, IHydratable
    {
        public AuthenticationInfo()
        {
            this.LogoffControlSrc = Null.NullString;
            this.LoginControlSrc = Null.NullString;
            this.SettingsControlSrc = Null.NullString;
            this.AuthenticationType = Null.NullString;
            this.AuthenticationID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the ID of the Authentication System.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int AuthenticationID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the PackageID for the Authentication System.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and Sets a flag that determines whether the Authentication System is enabled.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsEnabled { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the type (name) of the Authentication System (eg DNN, OpenID, LiveID).
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the url for the Settings Control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string SettingsControlSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the url for the Login Control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string LoginControlSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the url for the Logoff Control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string LogoffControlSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public virtual int KeyID
        {
            get
            {
                return this.AuthenticationID;
            }

            set
            {
                this.AuthenticationID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a RoleInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public virtual void Fill(IDataReader dr)
        {
            this.AuthenticationID = Null.SetNullInteger(dr["AuthenticationID"]);
            this.PackageID = Null.SetNullInteger(dr["PackageID"]);
            this.IsEnabled = Null.SetNullBoolean(dr["IsEnabled"]);
            this.AuthenticationType = Null.SetNullString(dr["AuthenticationType"]);
            this.SettingsControlSrc = Null.SetNullString(dr["SettingsControlSrc"]);
            this.LoginControlSrc = Null.SetNullString(dr["LoginControlSrc"]);
            this.LogoffControlSrc = Null.SetNullString(dr["LogoffControlSrc"]);

            // Fill base class fields
            this.FillInternal(dr);
        }
    }
}
