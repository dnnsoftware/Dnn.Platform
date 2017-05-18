﻿#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
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
#endregion

using System;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Modules;

namespace Dnn.ExportImport.Components.Entities
{
    [Serializable]
    [TableName("ExportImportJobLogs")]
    [PrimaryKey("JobLogId")]
    public class ExportImportJobLog : IHydratable
    {
        public int JobLogId { get; set; }
        public int JobId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int Level { get; set; }
        public DateTime CreatedOnDate { get; set; }

        public int KeyID
        {
            get { return JobLogId; }
            set { JobLogId = value; }
        }

        public void Fill(IDataReader dr)
        {
            JobLogId = Null.SetNullInteger(dr[nameof(JobLogId)]);
            JobId = Null.SetNullInteger(dr[nameof(JobId)]);
            Name = Null.SetNullString(dr[nameof(Name)]);
            Value = Null.SetNullString(dr[nameof(Value)]);
            Level = Null.SetNullInteger(dr[nameof(Level)]);
            CreatedOnDate = Null.SetNullDateTime(dr[nameof(CreatedOnDate)]);
        }
    }
}