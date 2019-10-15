using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Profile;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    [DataContract]
    public class ProfileDefinitionDto
    {
        [DataMember(Name = "propertyCategory")]
        public string PropertyCategory { get; set; }

        [DataMember(Name = "dataType")]
        public string DataType { get; set; }

        [DataMember(Name = "propertyName")]
        public string PropertyName { get; set; }

        [DataMember(Name = "required")]
        public bool Required { get; set; }

        [DataMember(Name = "validationExpression")]
        public string ValidationExpression { get; set; }

        [DataMember(Name = "propertyValue")]
        public string PropertyValue { get; set; }

        [DataMember(Name = "visible")]
        public bool Visible { get; set; }

        [DataMember(Name = "length")]
        public int Length { get; set; }

        public ProfileDefinitionDto()
        {
            
        }

        public ProfileDefinitionDto(ProfilePropertyDefinition definition)
        {
            PropertyCategory = definition.PropertyCategory;
            PropertyName = definition.PropertyName;
            Required = definition.Required;
            ValidationExpression = definition.ValidationExpression;
            PropertyValue = definition.PropertyValue;
            Visible = definition.Visible;
            Length = definition.Length;

            var dataTypeId = definition.DataType;
            var listController = new ListController();
            var dataTypes = listController.GetListEntryInfoDictionary("DataType");
            if (dataTypes.Any(i => i.Value.EntryID == dataTypeId))
            {
                DataType = dataTypes.First(i => i.Value.EntryID == dataTypeId).Key.ToLowerInvariant().Substring(9);
            }
            else
            {
                DataType = "unknown";
            }
        }
    }
}