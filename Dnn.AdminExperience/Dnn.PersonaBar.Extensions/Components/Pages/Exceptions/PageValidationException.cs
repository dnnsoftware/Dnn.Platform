﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    public class PageValidationException : Exception
    {
        public string Field { get; set; }

        public PageValidationException(string field, string message) : base(message)
        {
            this.Field = field;
        }
    }
}
