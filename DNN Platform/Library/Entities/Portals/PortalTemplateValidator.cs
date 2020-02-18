// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            SchemaSet.Add("", schemaFileName);
            return Validate(xmlFilename);
        }
    }
}
