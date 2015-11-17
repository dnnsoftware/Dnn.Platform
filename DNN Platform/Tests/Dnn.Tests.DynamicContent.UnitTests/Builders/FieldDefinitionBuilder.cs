// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;

namespace Dnn.Tests.DynamicContent.UnitTests.Builders
{
    class FieldDefinitionBuilder
    {
        private int _fieldDefinitionId = 3;
        private int _contentTypeId = 8;
        private int _fieldTypeId = 1;
        private string _name = "First Name";
        private string _label = "First Name";
        private string _description = "First Name";
        private int _portalId = -1;
        private int _order = 3;
        private bool _isReferenceType ;
        private bool _isList;

        public FieldDefinitionBuilder WithFieldDefinitionId(int fieldDefinitionId)
        {
            _fieldDefinitionId = fieldDefinitionId;
            return this;
        }

        public FieldDefinitionBuilder WithContentTypeId(int contentTypeId)
        {
            _contentTypeId = contentTypeId;
            return this;
        }

        public FieldDefinitionBuilder WithFieldTypeId(int fieldTypeId)
        {
            _fieldTypeId = fieldTypeId;
            return this;
        }

        public FieldDefinitionBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public FieldDefinitionBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }

        public FieldDefinitionBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public FieldDefinitionBuilder WithPortalId(int portalId)
        {
            _portalId = portalId;
            return this;
        }

        public FieldDefinitionBuilder WithOrder(int order)
        {
            _order = order;
            return this;
        }

        public FieldDefinitionBuilder WithIsReferenceType(bool isReferenceType)
        {
            _isReferenceType = isReferenceType;
            return this;
        }

        public FieldDefinitionBuilder WithIsList(bool isList)
        {
            _isList = isList;
            return this;
        }
      
        public FieldDefinition Build()
        {
            return new FieldDefinition
            {
                FieldDefinitionId = _fieldDefinitionId,
                ContentTypeId =  _contentTypeId,
                FieldTypeId = _fieldTypeId,
                Name = _name,
                Label = _label,
                Description = _description,
                PortalId = _portalId,
                Order = _order,
                IsReferenceType = _isReferenceType,
                IsList = _isList
            };
        }

    }
}
