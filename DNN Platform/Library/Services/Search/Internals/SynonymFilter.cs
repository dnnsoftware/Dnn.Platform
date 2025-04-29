// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Search.Entities;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

/// <summary>SynonymFilter.</summary>
/// <remarks>Implementation is inspired by sample code in Manning Lucene In Action 2nd Edition, pg. 133.</remarks>
internal sealed class SynonymFilter : TokenFilter
{
    private readonly Stack<string> synonymStack = new Stack<string>();
    private readonly TermAttribute termAtt;
    private readonly PositionIncrementAttribute posIncrAtt;
    private State current;

    /// <summary>Initializes a new instance of the <see cref="SynonymFilter"/> class.</summary>
    /// <param name="input"></param>
    public SynonymFilter(TokenStream input)
        : base(input)
    {
        this.termAtt = (TermAttribute)this.AddAttribute<ITermAttribute>();
        this.posIncrAtt = (PositionIncrementAttribute)this.AddAttribute<IPositionIncrementAttribute>();
    }

    /// <inheritdoc/>
    public override bool IncrementToken()
    {
        // Pop buffered synonyms
        if (this.synonymStack.Count > 0)
        {
            var syn = this.synonymStack.Pop();
            this.RestoreState(this.current);
            this.termAtt.SetTermBuffer(syn);

            // set position increment to 0
            this.posIncrAtt.PositionIncrement = 0;
            return true;
        }

        // read next token
        if (!this.input.IncrementToken())
        {
            return false;
        }

        // push synonyms onto stack
        if (this.AddAliasesToStack())
        {
            this.current = this.CaptureState(); // save current token
        }

        return true;
    }

    private bool AddAliasesToStack()
    {
        var portalId = 0; // default
        string cultureCode;
        var searchDoc = Thread.GetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo)) as SearchDocument;
        if (searchDoc != null)
        {
            portalId = searchDoc.PortalId;
            cultureCode = searchDoc.CultureCode;
            if (string.IsNullOrEmpty(cultureCode))
            {
                var portalInfo = PortalController.Instance.GetPortal(portalId);
                if (portalInfo != null)
                {
                    cultureCode = portalInfo.DefaultLanguage;
                }
            }
        }
        else
        {
            cultureCode = Thread.CurrentThread.CurrentCulture.Name;
        }

        var synonyms = SearchHelper.Instance.GetSynonyms(portalId, cultureCode, this.termAtt.Term).ToArray();
        if (!synonyms.Any())
        {
            return false;
        }

        var cultureInfo = new CultureInfo(cultureCode);
        foreach (var synonym in synonyms)
        {
            this.synonymStack.Push(synonym.ToLower(cultureInfo));
        }

        return true;
    }
}
