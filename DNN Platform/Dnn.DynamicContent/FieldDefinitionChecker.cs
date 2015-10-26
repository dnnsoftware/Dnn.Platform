// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.Framework;

namespace Dnn.DynamicContent
{
    class FieldDefinitionChecker : ServiceLocator<IFieldDefinitionChecker, FieldDefinitionChecker>, IFieldDefinitionChecker
    {
        protected override Func<IFieldDefinitionChecker> GetFactory()
        {
            return () => new FieldDefinitionChecker();
        }

        public bool IsValid(FieldDefinition fieldDefinition, out string errorMessage)
        {
            //TODO: check that the content type exists
            if (DeadLoopInFieldDefinition(fieldDefinition, fieldDefinition))
            {
                errorMessage = String.Format(DotNetNuke.Services.Localization.Localization.GetExceptionMessage("FieldDefinitionInvalidDeadLoop",
                    "It is not posible to create the field with name {0} because it would create a dead loop definition."), fieldDefinition.Name);
                return false;
            }

            errorMessage = null;
            return true;
        }

        private bool DeadLoopInFieldDefinition(FieldDefinition fieldDefinition, FieldDefinition baseFieldDefinition)
        {
            if (fieldDefinition.IsReferenceType)
            {
                if(fieldDefinition.FieldTypeId == baseFieldDefinition.ContentTypeId)
                { 
                    return true;
                }

                foreach (var contentTypeField in FieldDefinitionManager.Instance.GetFieldDefinitions(fieldDefinition.FieldTypeId))
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
