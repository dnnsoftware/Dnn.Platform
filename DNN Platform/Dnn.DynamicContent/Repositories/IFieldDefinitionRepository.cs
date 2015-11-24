// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Dnn.DynamicContent.Repositories
{
    internal interface IFieldDefinitionRepository
    {
        void Add(FieldDefinition field);

        void Delete(FieldDefinition field);

        IEnumerable<FieldDefinition> Get();

        IEnumerable<FieldDefinition> Get<TScope>(TScope scope);

        void Update(FieldDefinition field);

        void Move(int contentTypeId, int sourceIndex, int targetIndex);
    }
}
