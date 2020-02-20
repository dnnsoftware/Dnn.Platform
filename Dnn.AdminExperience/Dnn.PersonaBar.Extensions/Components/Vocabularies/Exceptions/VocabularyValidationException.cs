// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Vocabularies.Components;

namespace Dnn.PersonaBar.Vocabularies.Exceptions
{
    public class VocabularyValidationException : VocabulariesException
    {
        public VocabularyValidationException()
            : base(Constants.VocabularyValidationError)
        {
            
        }
    }
}
