namespace Dnn.PersonaBar.Prompt.Models
{
    /// <summary>
    /// Standard response object sent to client
    /// </summary>
    public class ConsoleResultModel
    {
        // the returned result - text or HTML
        public string output;
        // is the output an error message?
        public bool isError;
        // is the Output HTML?
        public bool isHtml;
        // should the client reload after processing the command
        public bool mustReload = false;
        // the response contains data to be formatted by the client
        public object data;
        // optionally tell the client in what order the fields should be displayed
        public string[] fieldOrder;

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