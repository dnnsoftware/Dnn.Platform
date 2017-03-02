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

using System.Data;
using PlatformDataProvider = DotNetNuke.Data.DataProvider;

namespace Dnn.ExportImport.Components.Providers
{
    public class DataProvider
    {
        #region Shared/Static Methods

        private static readonly DataProvider Provider;

        static DataProvider()
        {
            Provider = new DataProvider();
        }

        public static DataProvider Instance()
        {
            return Provider;
        }

        private DataProvider()
        {
            // so it can't be instantiated outside this class
        }

        #endregion

        #region Public Methods

        public int AddNewJob(int portalId, int userId, JobType jobType, string exportFile, string serializedObject)
        {
            return PlatformDataProvider.Instance().ExecuteScalar<int>(
                "ExportImportJobs_Add", portalId, (int)jobType, userId, exportFile, serializedObject);
        }

        public void UpdateJobStatus(int jobId, JobStatus jobStatus)
        {
            PlatformDataProvider.Instance().ExecuteNonQuery("ExportImportJobs_UpdateStatus", jobId, jobStatus);
        }

        public IDataReader GetFirstActiveJob()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportImportJobs_FirstActive");
        }

        public IDataReader GetAllJobs(int? portalId, int? pageSize, int?pageIndex)
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportImportJobs_GetAll", portalId, pageSize, pageIndex);
        }

        public IDataReader GetAllScopeTypes()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_ScopeTypes");
        }

        public IDataReader GetAllVocabularyTypes()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_VocabularyTypes");
        }

        public IDataReader GetAllTerms()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_Terms");
        }

        public IDataReader GetAllVocabularies()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_Vocabularies");
        }
        #endregion
    }
}