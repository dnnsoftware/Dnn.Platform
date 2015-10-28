// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent.Repositories;
using DotNetNuke.Framework;

namespace Dnn.DynamicContent
{
    internal class FieldDefinitionChecker : ServiceLocator<IFieldDefinitionChecker, FieldDefinitionChecker>, IFieldDefinitionChecker
    {
        protected override Func<IFieldDefinitionChecker> GetFactory()
        {
            return () => new FieldDefinitionChecker();
        }

        private readonly IDynamicContentTypeManager _dynamicContentTypeManager;
        private readonly IFieldDefinitionRepository _fieldDefinitionRepository;

        public FieldDefinitionChecker()
        {
            _dynamicContentTypeManager = DynamicContentTypeManager.Instance;
            _fieldDefinitionRepository = FieldDefinitionRepository.Instance;
        }

        public bool IsValid(FieldDefinition fieldDefinition, out string errorMessage)
        {
            if (!DynamicContentTypeExists(fieldDefinition.ContentTypeId, fieldDefinition.PortalId))
            {
                errorMessage = DotNetNuke.Services.Localization.Localization.GetExceptionMessage("ContentTypeFieldDefinitionDoesNotExist", "The content type to which the field definition belongs is not valid.");
                return false;
            }

            if (fieldDefinition.IsReferenceType && !DynamicContentTypeExists(fieldDefinition.FieldTypeId, fieldDefinition.PortalId))
            {
                errorMessage = DotNetNuke.Services.Localization.Localization.GetExceptionMessage("ContentTypeDoesNotExist", "The specified content type is not valid.");
                return false;
            }

            if (DeadLoopInFieldDefinition(fieldDefinition, fieldDefinition))
            {
                errorMessage = String.Format(DotNetNuke.Services.Localization.Localization.GetExceptionMessage("FieldDefinitionInvalidDeadLoop",
                    "It is not posible to create the field with name {0} because it would create a dead loop definition."), fieldDefinition.Name);
                return false;
            }

            errorMessage = null;
            return true;
        }

        private bool DynamicContentTypeExists(int contentTypeId, int portalId)
        {
            return _dynamicContentTypeManager.GetContentType(contentTypeId, portalId, true) != null;
        }

        private bool DeadLoopInFieldDefinition(FieldDefinition fieldDefinition, FieldDefinition baseFieldDefinition)
        {
            if (fieldDefinition.IsReferenceType)
            {
                if(fieldDefinition.FieldTypeId == baseFieldDefinition.ContentTypeId)
                { 
                    return true;
                }

                foreach (var contentTypeField in _fieldDefinitionRepository.Get(fieldDefinition.FieldTypeId).OrderBy(f => f.Order).AsQueryable())
                {
                    if (DeadLoopInFieldDefinition(contentTypeField, baseFieldDefinition))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
