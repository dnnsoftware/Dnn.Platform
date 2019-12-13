using System;

namespace Dnn.PersonaBar.Vocabularies.Exceptions
{
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
