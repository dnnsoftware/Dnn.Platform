﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    using System;

    public class BulkPagesException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="BulkPagesException"/> class.</summary>
        /// <param name="field">The invalid field.</param>
        /// <param name="message">The message that describes the error.</param>
        public BulkPagesException(string field, string message)
            : base(message)
        {
            this.Field = field;
        }

        public string Field { get; set; }
    }
}
