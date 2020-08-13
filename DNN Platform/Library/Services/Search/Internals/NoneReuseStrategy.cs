using System;
using System.Collections.Generic;
using System.Linq;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;

namespace DotNetNuke.Services.Search.Internals
{
    internal class NoneReuseStrategy : ReuseStrategy
    {
        public override TokenStreamComponents GetReusableComponents(Analyzer analyzer, string fieldName)
        {
            return null;
        }

        public override void SetReusableComponents(Analyzer analyzer, string fieldName, TokenStreamComponents components)
        {
        }
    }
}
