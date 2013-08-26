#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Content.Taxonomy;

namespace DotNetNuke.Subscriptions.Components.Common
{
    public class ObjectKey
    {
        #region Constructors

        public ObjectKey(IEnumerable<Term> terms)
        {
            Tags = terms.ToList();
        }

        public ObjectKey(string encoded)
        {
            Tags = new List<Term>();

            var termController = new TermController() as ITermController;

            if (!string.IsNullOrEmpty(encoded))
            {
                foreach (var tag in encoded.Split(';'))
                {
                    var term =
                        termController.GetTermsByVocabulary("Tags")
                                      .SingleOrDefault(
                                          x => string.Equals(tag, x.Name, StringComparison.InvariantCultureIgnoreCase));

                    Tags.Add(term);
                }
            }
        }

        #endregion

        #region Public members

        public IList<Term> Tags { get; set; }

        #endregion
    }
}