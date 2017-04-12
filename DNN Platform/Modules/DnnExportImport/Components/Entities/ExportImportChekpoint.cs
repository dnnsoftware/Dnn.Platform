#region Copyright
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
    [TableName("ExportImportCheckpoints")]
    [PrimaryKey("CheckpointId")]
    public class ExportImportChekpoint : IHydratable
    {
        private double _progress;

        public int CheckpointId { get; set; }
        public int JobId { get; set; }
        public string AssemblyName { get; set; }
        public string Category { get; set; }
        public int Stage { get; set; } // all stages start from 0 and increase
        public string StageData { get; set; } // discretionary data
        public DateTime StartDate { get; set; }
        public DateTime LastUpdateDate { get; set; }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _progress = value;
            }
        }

        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }

        public int KeyID
        {
            get { return CheckpointId; }
            set { CheckpointId = value; }
        }

        public void Fill(IDataReader dr)
        {
            CheckpointId = Null.SetNullInteger(dr[nameof(CheckpointId)]);
            JobId = Null.SetNullInteger(dr[nameof(JobId)]);
            Category = Null.SetNullString(dr[nameof(Category)]);
            Stage = Null.SetNullInteger(dr[nameof(Stage)]);
            StageData = Null.SetNullString(dr[nameof(StageData)]);
            Progress = Null.SetNullInteger(dr[nameof(Progress)]);
            TotalItems = Null.SetNullInteger(dr[nameof(TotalItems)]);
            ProcessedItems = Null.SetNullInteger(dr[nameof(ProcessedItems)]);
            StartDate = Null.SetNullDateTime(dr[nameof(StartDate)]);
            LastUpdateDate = Null.SetNullDateTime(dr[nameof(LastUpdateDate)]);
        }
    }
}