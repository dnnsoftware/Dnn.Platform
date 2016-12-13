#region Copyright
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
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.Taxonomy.Services
{
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
	[SupportedModules("DotNetNuke.Taxonomy")]
    public class ServicesController : DnnApiController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ServicesController));

		[HttpGet]
		public HttpResponseMessage Search(int vocabularyId, int termId, int parentId, string termName)
		{
			IList<SearchResult> results = new List<SearchResult>();

			var controller = new TermController();
			var vocabulary = new VocabularyController().GetVocabularies().FirstOrDefault(v => v.VocabularyId == vocabularyId);
			if (vocabulary != null && !string.IsNullOrEmpty(termName))
			{
				var terms = controller.GetTermsByVocabulary(vocabularyId);
				var relatedTerms = terms.Where(t => t.Name.ToLowerInvariant().Contains(termName.Trim().ToLowerInvariant()) && t.TermId != termId && (parentId < 0 || t.ParentTermId == parentId));

				foreach (Term term in relatedTerms)
				{
					results.Add(new SearchResult(){label = term.Name, value = term.Name});
				}
			}

			return Request.CreateResponse(HttpStatusCode.OK, results);
		}

		[HttpGet]
		public HttpResponseMessage Exist(int vocabularyId, int termId, int parentId, string termName)
		{
			var exists = false;

			var controller = new TermController();
			var vocabulary = new VocabularyController().GetVocabularies().FirstOrDefault(v => v.VocabularyId == vocabularyId);
			if (vocabulary != null && !string.IsNullOrEmpty(termName))
			{
				var terms = controller.GetTermsByVocabulary(vocabularyId);
				exists = terms.Any(t => t.Name.Equals(termName.Trim(), StringComparison.InvariantCultureIgnoreCase) && t.TermId != termId && (parentId < 0 || t.ParentTermId == parentId));
			}

			return Request.CreateResponse(HttpStatusCode.OK, exists);
		}
    }

	class SearchResult
	{
		// ReSharper disable InconsistentNaming
		public string label;
		public string value;
		// ReSharper restore InconsistentNaming
	}
}