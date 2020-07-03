// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Vocabularies.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class TermDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int TermId { get; set; }

        public int ParentTermId { get; set; }

        public int VocabularyId { get; set; }

        public IList<TermDto> ChildTerms { get; set; }
    }
}
