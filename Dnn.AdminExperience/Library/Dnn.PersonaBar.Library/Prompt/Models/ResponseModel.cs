// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.PersonaBar.Library.Prompt.Models
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
            Data = "";
        }
        public ResponseModel(bool err, string msg, string data)
        {
            IsError = err;
            Message = msg;
            Data = data;
        }
    }
}
