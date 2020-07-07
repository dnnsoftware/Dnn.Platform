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
            this.IsError = err;
            this.Message = msg;
            this.Data = string.Empty;
        }

        public ResponseModel(bool err, string msg, string data)
        {
            this.IsError = err;
            this.Message = msg;
            this.Data = data;
        }
    }
}
