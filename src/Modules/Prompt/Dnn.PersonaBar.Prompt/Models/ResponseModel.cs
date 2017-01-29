namespace Dnn.PersonaBar.Prompt.Models
{
    public class ResponseModel
    {
        public bool IsError;
        public string Message;
        public string Data;
        public ResponseModel(bool err, string msg)
        {
            IsError = err;
            Message = msg;
            this.Data = "";
        }
        public ResponseModel(bool err, string msg, string data)
        {
            IsError = err;
            Message = msg;
            this.Data = data;
        }
    }
}