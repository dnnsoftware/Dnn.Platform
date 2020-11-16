// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Profile;

    [DataContract]
    public class ProfileDefinitionDto
    {
        public ProfileDefinitionDto()
        {

        }

        public ProfileDefinitionDto(ProfilePropertyDefinition definition)
        {
            this.PropertyCategory = definition.PropertyCategory;
            this.PropertyName = definition.PropertyName;
            this.Required = definition.Required;
            this.ValidationExpression = definition.ValidationExpression;
            this.PropertyValue = definition.PropertyValue;
            this.Visible = definition.Visible;
            this.Length = definition.Length;

            var dataTypeId = definition.DataType;
            var listController = new ListController();
            var dataTypes = listController.GetListEntryInfoDictionary("DataType");
            if (dataTypes.Any(i => i.Value.EntryID == dataTypeId))
            {
                this.DataType = dataTypes.First(i => i.Value.EntryID == dataTypeId).Key.ToLowerInvariant().Substring(9);
            }
            else
            {
                this.DataType = "unknown";
            }
        }

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
    }
}
