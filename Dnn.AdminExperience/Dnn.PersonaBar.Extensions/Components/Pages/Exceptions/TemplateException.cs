﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    using System;

    public class TemplateException : Exception
    {
        public TemplateException(string message) : base(message)
        {
        }
    }
}
