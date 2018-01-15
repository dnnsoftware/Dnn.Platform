#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Vocabularies.Exceptions;
using Dnn.PersonaBar.Vocabularies.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using Constants = Dnn.PersonaBar.Vocabularies.Components.Constants;

namespace Dnn.PersonaBar.Vocabularies.Services
{
    [MenuPermission(MenuName = Constants.MenuIdentifier)]
    public class VocabulariesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(VocabulariesController));
        private Components.VocabulariesController _controller = new Components.VocabulariesController();
        private static string LocalResourcesFile => Path.Combine(Library.Constants.PersonaBarRelativePath, "Modules/Dnn.Vocabularies/App_LocalResources/Vocabularies.resx");
        private const string AuthFailureMessage = "Authorization has been denied for this request.";

        /// GET: api/Vocabularies/GetVocabularies
        /// <summary>
        /// Gets an overall list of vocabularies
        /// </summary>
        /// <param></param>
        /// <returns>List of vocabularies</returns>
        [HttpGet]
        public HttpResponseMessage GetVocabularies(int pageIndex, int pageSize, int scopeTypeId = -1)
        {
            try
            {
                int total = 0;
                var vocabularies = _controller.GetVocabularies(PortalId, pageIndex, pageSize, scopeTypeId, out total).Select(v => new
                {
                    v.VocabularyId,
                    v.Name,
                    v.Description,
                    Type = v.Type.ToString(),
                    TypeId = (int)v.Type,
                    v.ScopeType.ScopeType,
                    v.ScopeTypeId,
                    v.IsSystem
                });

                var response = new
                {
                    Success = true,
                    Results = vocabularies,
                    TotalResults = total
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/CreateVocabulary
        /// <summary>
        /// Creates a new vocabulary
        /// </summary>
        /// <param name="vocabularyDto">Data of a new vocabulary</param>
        /// <returns>Id of the new added vocabulary</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage CreateVocabulary(VocabularyDto vocabularyDto)
        {
            try
            {
                var vocabulary = new Vocabulary(vocabularyDto.Name, vocabularyDto.Description);
                vocabulary.Type = vocabularyDto.TypeId == 1
                    ? VocabularyType.Simple
                    : VocabularyType.Hierarchy;
                vocabulary.ScopeTypeId = vocabularyDto.ScopeTypeId;

                int vocabularyId = _controller.AddVocabulary(vocabulary);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, VocabularyId = vocabularyId });
            }
            catch (VocabularyNameAlreadyExistsException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("VocabularyExists.Error", LocalResourcesFile), vocabularyDto.Name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/UpdateVocabulary
        /// <summary>
        /// Updates an existing vocabulary
        /// </summary>
        /// <param name="vocabularyDto">Data of an existing vocabulary</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage UpdateVocabulary(VocabularyDto vocabularyDto)
        {
            try
            {
                if (_controller.IsSystemVocabulary(vocabularyDto.VocabularyId) && !UserInfo.IsSuperUser)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }

                var vocabulary = new Vocabulary(vocabularyDto.Name, vocabularyDto.Description);
                vocabulary.Type = vocabularyDto.Type == Constants.VocabularyTypeSimple ? VocabularyType.Simple : VocabularyType.Hierarchy;
                vocabulary.ScopeTypeId = vocabularyDto.ScopeTypeId;
                vocabulary.VocabularyId = vocabularyDto.VocabularyId;

                _controller.UpdateVocabulary(vocabulary);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (VocabularyValidationException exc)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/DeleteVocabulary
        /// <summary>
        /// Removes an existing vocabulary
        /// </summary>
        /// <param name="vocabularyId">Id of an existing vocabulary that will be deleted</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage DeleteVocabulary(int vocabularyId)
        {
            try
            {
                if (_controller.IsSystemVocabulary(vocabularyId))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = "CannotDeleteSystemVocabulary" });
                }
                _controller.DeleteVocabulary(new Vocabulary() { VocabularyId = vocabularyId });
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// GET: api/Vocabularies/GetTermsByVocabularyId
        /// <summary>
        /// Gets a list of terms belonging to a specific vocabulary
        /// </summary>
        /// <param name="vocabularyId">Id of an existing vocabulary</param>
        /// <returns>List of terms</returns>
        [HttpGet]
        public HttpResponseMessage GetTermsByVocabularyId(int vocabularyId)
        {
            try
            {
                var terms = _controller.GetTermsByVocabulary(vocabularyId);

                var response = new
                {
                    Success = true,
                    Results = terms.Select(t => new 
                    {
                        t.TermId,
                        t.Description,
                        t.Name,
                        ParentTermId = t.ParentTermId ?? Null.NullInteger,
                        t.VocabularyId
                    }),
                    TotalResults = terms.Count
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// GET: api/Vocabularies/GetTerm
        /// <summary>
        /// Gets a term
        /// </summary>
        /// <param name="termId">Id of an existing term</param>
        /// <returns>Data of a term</returns>
        [HttpGet]
        public HttpResponseMessage GetTerm(int termId)
        {
            try
            {
                var term = _controller.GetTerm(termId);
                var response = new
                {
                    term.TermId,
                    term.ParentTermId,
                    TermPath = term.GetTermPath(),
                    term.VocabularyId,
                    term.Name,
                    term.Description
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/CreateTerm
        /// <summary>
        /// Creates a new term
        /// </summary>
        /// <param name="termDto">Data of a new term</param>
        /// <returns>Id of the new created term</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage CreateTerm(TermDto termDto)
        {
            try
            {
                if (_controller.IsSystemVocabulary(termDto.VocabularyId) && !UserInfo.IsSuperUser)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }
                var term = new Term(termDto.Name, termDto.Description, termDto.VocabularyId);
                if (termDto.ParentTermId != Null.NullInteger)
                {
                    term.ParentTermId = termDto.ParentTermId;
                }
                int termId = _controller.AddTerm(term);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, TermId = termId });
            }
            catch (TermValidationException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("TermExists.Error", LocalResourcesFile), termDto.Name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Vocabularies/UpdateTerm
        /// <summary>
        /// Updates an existing term
        /// </summary>
        /// <param name="termDto">Data of an existing term</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage UpdateTerm(TermDto termDto)
        {
            try
            {
                if (_controller.IsSystemVocabulary(termDto.VocabularyId) && !UserInfo.IsSuperUser)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }
                var term = new Term(termDto.Name, termDto.Description, termDto.VocabularyId);
                term.TermId = termDto.TermId;
                if (termDto.ParentTermId != Null.NullInteger)
                {
                    term.ParentTermId = termDto.ParentTermId;
                }
                _controller.UpdateTerm(term);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (TermValidationException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("TermExists.Error", LocalResourcesFile), termDto.Name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/DeleteTerm
        /// <summary>
        /// Removes an existing term
        /// </summary>
        /// <param name="termId">Id of an existing term</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage DeleteTerm(int termId)
        {
            try
            {
                var term = _controller.GetTerm(termId);
                if (_controller.IsSystemVocabulary(term.VocabularyId) && !UserInfo.IsSuperUser)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }
                _controller.DeleteTerm(term);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }
    }
}
