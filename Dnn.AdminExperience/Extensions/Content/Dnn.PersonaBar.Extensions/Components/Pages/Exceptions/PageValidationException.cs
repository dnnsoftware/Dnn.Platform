using System;

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    public class PageValidationException : Exception
    {
        public string Field { get; set; }

        public PageValidationException(string field, string message) : base(message)
        {
            Field = field;
        }
    }
}