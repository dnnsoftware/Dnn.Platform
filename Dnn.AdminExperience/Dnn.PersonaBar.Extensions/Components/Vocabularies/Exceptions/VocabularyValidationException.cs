// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
