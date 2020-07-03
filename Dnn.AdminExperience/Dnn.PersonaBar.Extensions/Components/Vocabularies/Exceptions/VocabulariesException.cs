// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Vocabularies.Exceptions
{
    using System;

    public class VocabulariesException : ApplicationException
    {
        public VocabulariesException()
        {

        }

        public VocabulariesException(string message) : base(message)
        {

        }

        public VocabulariesException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
