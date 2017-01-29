namespace Dnn.PersonaBar.Prompt.Models
{
    public class ConsoleResultModel
    {
        // the returned result
        public string output;
        // is the output an error message
        public bool isError;
        // is the Output HTML?
        public bool isHtml;
        // should the client reload after processing the command
        public bool mustReload = false;
        // the response contains data to be formatted by the client
        public object data;

        public ConsoleResultModel()
        {
            //
        }

        public ConsoleResultModel(string output)
        {
            this.output = output;
        }
    }
}