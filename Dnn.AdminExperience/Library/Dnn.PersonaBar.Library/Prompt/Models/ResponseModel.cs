// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Prompt.Models;

using System.Diagnostics.CodeAnalysis;

public class ResponseModel
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public bool IsError;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string Message;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string Data;

    /// <summary>Initializes a new instance of the <see cref="ResponseModel"/> class.</summary>
    /// <param name="err"></param>
    /// <param name="msg"></param>
    public ResponseModel(bool err, string msg)
    {
        this.IsError = err;
        this.Message = msg;
        this.Data = string.Empty;
    }

    /// <summary>Initializes a new instance of the <see cref="ResponseModel"/> class.</summary>
    /// <param name="err"></param>
    /// <param name="msg"></param>
    /// <param name="data"></param>
    public ResponseModel(bool err, string msg, string data)
    {
        this.IsError = err;
        this.Message = msg;
        this.Data = data;
    }
}
