// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Vocabularies.Services.Dto
{
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
