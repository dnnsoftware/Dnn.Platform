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

    /// <summary>
    /// DNN-4016
    /// The UserAuthenticationInfo class provides the Entity Layer for the
    /// user information in the Authentication Systems.
    /// </summary>
    [Serializable]
    public class UserAuthenticationInfo : BaseEntityInfo, IHydratable
    {
        public UserAuthenticationInfo()
        {
            this.AuthenticationToken = Null.NullString;
            this.AuthenticationType = Null.NullString;
            this.UserAuthenticationID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the ID of the User Record in the Authentication System.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int UserAuthenticationID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the PackageID for the Authentication System.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int UserID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the type (name) of the Authentication System (eg DNN, OpenID, LiveID).
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the url for the Logoff Control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationToken { get; set; }

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
                return this.UserAuthenticationID;
            }

            set
            {
                this.UserAuthenticationID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a UserAuthenticationInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public virtual void Fill(IDataReader dr)
        {
            this.UserAuthenticationID = Null.SetNullInteger(dr["UserAuthenticationID"]);
            this.UserID = Null.SetNullInteger(dr["UserID"]);
            this.AuthenticationType = Null.SetNullString(dr["AuthenticationType"]);
            this.AuthenticationToken = Null.SetNullString(dr["AuthenticationToken"]);

            // Fill base class fields
            this.FillInternal(dr);
        }
    }
}
