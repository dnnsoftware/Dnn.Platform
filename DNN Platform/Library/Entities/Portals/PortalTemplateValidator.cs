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
