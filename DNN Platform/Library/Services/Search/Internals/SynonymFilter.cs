// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Tokenattributes;

    /// <summary>
    /// SynonymFilter.
    /// </summary>
    /// <remarks>Implementation is inspired by sample code in Manning Lucene In Action 2nd Edition, pg. 133.</remarks>
    internal sealed class SynonymFilter : TokenFilter
    {
        private readonly Stack<string> _synonymStack = new Stack<string>();
        private readonly TermAttribute _termAtt;
        private readonly PositionIncrementAttribute _posIncrAtt;
        private State _current;

        public SynonymFilter(TokenStream input)
            : base(input)
        {
            this._termAtt = (TermAttribute)this.AddAttribute<ITermAttribute>();
            this._posIncrAtt = (PositionIncrementAttribute)this.AddAttribute<IPositionIncrementAttribute>();
        }

        public override bool IncrementToken()
        {
            // Pop buffered synonyms
            if (this._synonymStack.Count > 0)
            {
                var syn = this._synonymStack.Pop();
                this.RestoreState(this._current);
                this._termAtt.SetTermBuffer(syn);

                // set position increment to 0
                this._posIncrAtt.PositionIncrement = 0;
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
                this._current = this.CaptureState(); // save current token
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

            var synonyms = SearchHelper.Instance.GetSynonyms(portalId, cultureCode, this._termAtt.Term).ToArray();
            if (!synonyms.Any())
            {
                return false;
            }

            var cultureInfo = new CultureInfo(cultureCode);
            foreach (var synonym in synonyms)
            {
                this._synonymStack.Push(synonym.ToLower(cultureInfo));
            }

            return true;
        }
    }
}
