namespace Dnn.PersonaBar.Prompt.Models
{
    public class ConsoleErrorResultModel : ConsoleResultModel
    {

        public ConsoleErrorResultModel()
        {
            isError = true;
            output = "Invalid syntax";
        }

        public ConsoleErrorResultModel(string errMessage)
        {
            isError = true;
            output = errMessage;
        }
    }
}