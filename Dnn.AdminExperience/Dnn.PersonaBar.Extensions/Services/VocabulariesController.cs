// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Vocabularies.Services
{
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

    [MenuPermission(MenuName = Constants.MenuIdentifier)]
    public class VocabulariesController : PersonaBarApiController
    {
        private const string AuthFailureMessage = "Authorization has been denied for this request.";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(VocabulariesController));
        private Components.VocabulariesController _controller = new Components.VocabulariesController();
        private static string LocalResourcesFile => Path.Combine(Library.Constants.PersonaBarRelativePath, "Modules/Dnn.Vocabularies/App_LocalResources/Vocabularies.resx");

        /// GET: api/Vocabularies/GetVocabularies
        /// <summary>
        /// Gets an overall list of vocabularies.
        /// </summary>
        /// <param></param>
        /// <returns>List of vocabularies.</returns>
        [HttpGet]
        public HttpResponseMessage GetVocabularies(int pageIndex, int pageSize, int scopeTypeId = -1)
        {
            try
            {
                int total = 0;
                var vocabularies = this._controller.GetVocabularies(this.PortalId, pageIndex, pageSize, scopeTypeId, out total).Select(v => new
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
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/CreateVocabulary
        /// <summary>
        /// Creates a new vocabulary.
        /// </summary>
        /// <param name="vocabularyDto">Data of a new vocabulary.</param>
        /// <returns>Id of the new added vocabulary.</returns>
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

                int vocabularyId = this._controller.AddVocabulary(vocabulary);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, VocabularyId = vocabularyId });
            }
            catch (VocabularyNameAlreadyExistsException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("VocabularyExists.Error", LocalResourcesFile), vocabularyDto.Name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/UpdateVocabulary
        /// <summary>
        /// Updates an existing vocabulary.
        /// </summary>
        /// <param name="vocabularyDto">Data of an existing vocabulary.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage UpdateVocabulary(VocabularyDto vocabularyDto)
        {
            try
            {
                if (this._controller.IsSystemVocabulary(vocabularyDto.VocabularyId) && !this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }

                var vocabulary = new Vocabulary(vocabularyDto.Name, vocabularyDto.Description);
                vocabulary.Type = vocabularyDto.Type == Constants.VocabularyTypeSimple ? VocabularyType.Simple : VocabularyType.Hierarchy;
                vocabulary.ScopeTypeId = vocabularyDto.ScopeTypeId;
                vocabulary.VocabularyId = vocabularyDto.VocabularyId;

                this._controller.UpdateVocabulary(vocabulary);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (VocabularyValidationException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/DeleteVocabulary
        /// <summary>
        /// Removes an existing vocabulary.
        /// </summary>
        /// <param name="vocabularyId">Id of an existing vocabulary that will be deleted.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage DeleteVocabulary(int vocabularyId)
        {
            try
            {
                if (this._controller.IsSystemVocabulary(vocabularyId))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = "CannotDeleteSystemVocabulary" });
                }
                this._controller.DeleteVocabulary(new Vocabulary() { VocabularyId = vocabularyId });
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// GET: api/Vocabularies/GetTermsByVocabularyId
        /// <summary>
        /// Gets a list of terms belonging to a specific vocabulary.
        /// </summary>
        /// <param name="vocabularyId">Id of an existing vocabulary.</param>
        /// <returns>List of terms.</returns>
        [HttpGet]
        public HttpResponseMessage GetTermsByVocabularyId(int vocabularyId)
        {
            try
            {
                var terms = this._controller.GetTermsByVocabulary(vocabularyId);

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
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// GET: api/Vocabularies/GetTerm
        /// <summary>
        /// Gets a term.
        /// </summary>
        /// <param name="termId">Id of an existing term.</param>
        /// <returns>Data of a term.</returns>
        [HttpGet]
        public HttpResponseMessage GetTerm(int termId)
        {
            try
            {
                var term = this._controller.GetTerm(termId);
                var response = new
                {
                    term.TermId,
                    term.ParentTermId,
                    TermPath = term.GetTermPath(),
                    term.VocabularyId,
                    term.Name,
                    term.Description
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/CreateTerm
        /// <summary>
        /// Creates a new term.
        /// </summary>
        /// <param name="termDto">Data of a new term.</param>
        /// <returns>Id of the new created term.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage CreateTerm(TermDto termDto)
        {
            try
            {
                if (this._controller.IsSystemVocabulary(termDto.VocabularyId) && !this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }
                var term = new Term(termDto.Name, termDto.Description, termDto.VocabularyId);
                if (termDto.ParentTermId != Null.NullInteger)
                {
                    term.ParentTermId = termDto.ParentTermId;
                }
                int termId = this._controller.AddTerm(term);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, TermId = termId });
            }
            catch (TermValidationException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("TermExists.Error", LocalResourcesFile), termDto.Name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Vocabularies/UpdateTerm
        /// <summary>
        /// Updates an existing term.
        /// </summary>
        /// <param name="termDto">Data of an existing term.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage UpdateTerm(TermDto termDto)
        {
            try
            {
                if (this._controller.IsSystemVocabulary(termDto.VocabularyId) && !this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }
                var term = new Term(termDto.Name, termDto.Description, termDto.VocabularyId);
                term.TermId = termDto.TermId;
                if (termDto.ParentTermId != Null.NullInteger)
                {
                    term.ParentTermId = termDto.ParentTermId;
                }
                this._controller.UpdateTerm(term);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (TermValidationException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("TermExists.Error", LocalResourcesFile), termDto.Name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        /// POST: api/Vocabularies/DeleteTerm
        /// <summary>
        /// Removes an existing term.
        /// </summary>
        /// <param name="termId">Id of an existing term.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Constants.MenuIdentifier, Permission = "Edit")]
        public HttpResponseMessage DeleteTerm(int termId)
        {
            try
            {
                var term = this._controller.GetTerm(termId);
                if (this._controller.IsSystemVocabulary(term.VocabularyId) && !this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Success = true, Message = AuthFailureMessage });
                }
                this._controller.DeleteTerm(term);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }
    }
}
