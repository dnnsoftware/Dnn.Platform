﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using DotNetNuke.Common;

#endregion

namespace DotNetNuke.Entities.Portals
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PortalTemplateValidator Class is used to validate the Portal Template
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class PortalTemplateValidator : XmlValidatorBase
    {
        public bool Validate(string xmlFilename, string schemaFileName)
        {
            this.SchemaSet.Add("", schemaFileName);
            return this.Validate(xmlFilename);
        }
    }
}
