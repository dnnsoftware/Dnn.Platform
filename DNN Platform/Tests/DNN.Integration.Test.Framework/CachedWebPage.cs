// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DNN.Integration.Test.Framework
{
    internal class CachedWebPage
    {
        private string[] _inputFields;

        public CachedWebPage(string token, string[] fields)
        {
            FetchDateTime = DateTime.Now;
            VerificationToken = token;
            InputFields = fields;
        }

        public DateTime FetchDateTime { get; private set; } // when was this loaded
        public string VerificationToken { get; private set; }

        public string[] InputFields
        {
            get { return _inputFields.Clone() as string[]; }
            private set { _inputFields = value; }
        }
    }
}
