using Dnn.PersonaBar.Vocabularies.Components;

namespace Dnn.PersonaBar.Vocabularies.Exceptions
{
    public class TermValidationException : VocabulariesException
    {
        public TermValidationException()
            : base(Constants.TermValidationError)
        {
            
        }
    }
}
