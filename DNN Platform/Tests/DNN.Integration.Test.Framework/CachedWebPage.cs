// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;

    internal class CachedWebPage
    {
        private string[] _inputFields;

        public CachedWebPage(string token, string[] fields)
        {
            this.FetchDateTime = DateTime.Now;
            this.VerificationToken = token;
            this.InputFields = fields;
        }

        public DateTime FetchDateTime { get; private set; } // when was this loaded

        public string VerificationToken { get; private set; }

        public string[] InputFields
        {
            get { return this._inputFields.Clone() as string[]; }
            private set { this._inputFields = value; }
        }
    }
}
