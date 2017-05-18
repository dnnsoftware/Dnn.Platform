// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

// Influenced by http://sameproblemmorecode.blogspot.nl/2013/07/petapoco-as-its-meant-to-be-with.html
// https://github.com/luuksommers/PetaPoco

using System;
using PetaPoco;

namespace DotNetNuke.Data.PetaPoco
{
    [CLSCompliant(false)]
    public class FluentColumnMap
    {
        public ColumnInfo ColumnInfo { get; set; }
        public Func<object, object> FromDbConverter { get; set; }
        public Func<object, object> ToDbConverter { get; set; }

        public FluentColumnMap() { }
        public FluentColumnMap(ColumnInfo columnInfo) : this(columnInfo, null) { }
        public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter) : this(columnInfo, fromDbConverter, null) { }
        public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter, Func<object, object> toDbConverter)
        {
            ColumnInfo = columnInfo;
            FromDbConverter = fromDbConverter;
            ToDbConverter = toDbConverter;
        }
    }
}
