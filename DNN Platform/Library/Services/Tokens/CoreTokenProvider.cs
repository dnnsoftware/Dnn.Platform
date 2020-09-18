// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens 
{
    public class CoreTokenProvider : TokenProvider 
    {
        public override bool ContainsTokens(string content, TokenContext context) 
        {
            return false; // already determined by BaseCustomTokenReplace
        }

        public override string Tokenize(string content, TokenContext context) 
        {
            var tokenizer = new TokenReplace { TokenContext = context };
            return tokenizer.ReplaceEnvironmentTokens(content);
        }
    }
}
