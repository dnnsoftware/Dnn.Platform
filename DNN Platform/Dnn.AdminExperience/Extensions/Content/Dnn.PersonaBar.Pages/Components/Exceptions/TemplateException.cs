using System;

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    public class TemplateException : Exception
    {
        public TemplateException(string message): base(message)
        {
        }
    }
}