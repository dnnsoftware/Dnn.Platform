// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy;

using System;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

/// <summary>Class of Vocabulary.</summary>
/// <seealso cref="TermController"/>
[Serializable]
public class Vocabulary : BaseEntityInfo, IHydratable
{
    private static readonly PortalSecurity Security = PortalSecurity.Instance;

    private string description;
    private bool isSystem;
    private string name;
    private int scopeId;
    private ScopeType scopeType;
    private int scopeTypeId;
    private List<Term> terms;
    private VocabularyType type;
    private int vocabularyId;

    private int weight;

    /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
    public Vocabulary()
        : this(Null.NullString, Null.NullString, VocabularyType.Simple)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
    /// <param name="name"></param>
    public Vocabulary(string name)
        : this(name, Null.NullString, VocabularyType.Simple)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    public Vocabulary(string name, string description)
        : this(name, description, VocabularyType.Simple)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
    /// <param name="type"></param>
    public Vocabulary(VocabularyType type)
        : this(Null.NullString, Null.NullString, type)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="type"></param>
    public Vocabulary(string name, string description, VocabularyType type)
    {
        this.Description = description;
        this.Name = name;
        this.Type = type;

        this.ScopeId = Null.NullInteger;
        this.ScopeTypeId = Null.NullInteger;
        this.VocabularyId = Null.NullInteger;
        this.Weight = 0;
    }

    public bool IsHeirarchical
    {
        get
        {
            return this.Type == VocabularyType.Hierarchy;
        }
    }

    public ScopeType ScopeType
    {
        get
        {
            if (this.scopeType == null)
            {
                this.scopeType = this.GetScopeType(this.scopeTypeId);
            }

            return this.scopeType;
        }
    }

    public List<Term> Terms
    {
        get
        {
            if (this.terms == null)
            {
                this.terms = this.GetTerms(this.vocabularyId);
            }

            return this.terms;
        }
    }

    public string Description
    {
        get
        {
            return this.description;
        }

        set
        {
            this.description = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
        }
    }

    public bool IsSystem
    {
        get
        {
            return this.isSystem;
        }

        set
        {
            this.isSystem = value;
        }
    }

    public string Name
    {
        get
        {
            return this.name;
        }

        set
        {
            if (HtmlUtils.ContainsEntity(value))
            {
                value = System.Net.WebUtility.HtmlDecode(value);
            }

            this.name = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
        }
    }

    public int ScopeId
    {
        get
        {
            return this.scopeId;
        }

        set
        {
            this.scopeId = value;
        }
    }

    public int ScopeTypeId
    {
        get
        {
            return this.scopeTypeId;
        }

        set
        {
            this.scopeTypeId = value;
        }
    }

    public VocabularyType Type
    {
        get
        {
            return this.type;
        }

        set
        {
            this.type = value;
        }
    }

    public int VocabularyId
    {
        get
        {
            return this.vocabularyId;
        }

        set
        {
            this.vocabularyId = value;
        }
    }

    public int Weight
    {
        get
        {
            return this.weight;
        }

        set
        {
            this.weight = value;
        }
    }

    /// <inheritdoc/>
    public virtual int KeyID
    {
        get
        {
            return this.VocabularyId;
        }

        set
        {
            this.VocabularyId = value;
        }
    }

    /// <inheritdoc/>
    public virtual void Fill(IDataReader dr)
    {
        this.VocabularyId = Null.SetNullInteger(dr["VocabularyID"]);
        switch (Convert.ToInt16(dr["VocabularyTypeID"]))
        {
            case 1:
                this.Type = VocabularyType.Simple;
                break;
            case 2:
                this.Type = VocabularyType.Hierarchy;
                break;
        }

        this.IsSystem = Null.SetNullBoolean(dr["IsSystem"]);
        this.Name = Null.SetNullString(dr["Name"]);
        this.Description = Null.SetNullString(dr["Description"]);
        this.ScopeId = Null.SetNullInteger(dr["ScopeID"]);
        this.ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
        this.Weight = Null.SetNullInteger(dr["Weight"]);

        // Fill base class properties
        this.FillInternal(dr);
    }
}
