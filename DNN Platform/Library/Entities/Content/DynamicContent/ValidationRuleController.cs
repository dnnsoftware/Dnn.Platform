#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Data;

namespace DotNetNuke.Entities.Content.DynamicContent
{
    public class ValidationRuleController : ControllerBase<ValidationRule, IValidationRuleController, ValidationRuleController>, IValidationRuleController
    {
        protected override Func<IValidationRuleController> GetFactory()
        {
            return () => new ValidationRuleController();
        }

        public ValidationRuleController() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ValidationRuleController(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds a new rule for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        /// <returns>rule id.</returns>
        /// <exception cref="System.ArgumentNullException">rule is null.</exception>
        public int AddValidationRule(ValidationRule rule)
        {
            //Argument Contract
            Requires.PropertyNotNegative(rule, "FieldDefinitionId");
            Requires.PropertyNotNegative(rule, "ValidatorTypeId");

            Add(rule);

            return rule.ValidationRuleId;
        }

        /// <summary>
        /// Deletes the rule for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="rule">The rule to delete.</param>
        /// <exception cref="System.ArgumentNullException">rule is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">rule id is less than 0.</exception>
        public void DeleteValidationRule(ValidationRule rule)
        {
            Delete(rule);
        }

        /// <summary>
        /// Gets the rules.
        /// </summary>
        /// <param name="fieldDefinitionId">The Id of the parent Field Definition</param>
        /// <returns>rule collection.</returns>
        public IQueryable<ValidationRule> GetValidationRules(int fieldDefinitionId)
        {
            return Get(fieldDefinitionId).AsQueryable();
        }

        /// <summary>
        /// Updates the rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <exception cref="System.ArgumentNullException">rule is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">rule id is less than 0.</exception>
        public void UpdateValidationRule(ValidationRule rule)
        {
            //Argument Contract
            Requires.PropertyNotNegative(rule, "FieldDefinitionId");
            Requires.PropertyNotNegative(rule, "ValidatorTypeId");

            Update(rule);
        }
    }
}
