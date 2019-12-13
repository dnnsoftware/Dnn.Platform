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
