// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
