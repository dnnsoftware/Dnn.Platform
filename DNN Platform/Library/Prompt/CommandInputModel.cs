// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Abstractions.Prompt;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetNuke.Prompt
{
    public class CommandInputModel : ICommandInputModel
    {
        public string CmdLine { get; set; }
        public int CurrentPage { get; set; }
        public string[] Args
        {
            get
            {
                var tokenEx = new Regex("[^\\s\"]+|\"[^\"]*\"");
                // Matches (1 or more chars that are NOT space or ") or (" any # of chars not a " followed by a ")
                return tokenEx.Matches(CmdLine).Cast<Match>().Select(m => m.Value.Replace("\"", "")).ToArray();
            }
        }
    }
}
