// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Dnn.DynamicContent
{
    public interface IValidationRuleManager
    {
        /// <summary>
        /// Adds a new rule for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        /// <returns>rule id.</returns>
        /// <exception cref="System.ArgumentNullException">rule is null.</exception>
        int AddValidationRule(ValidationRule rule);

        /// <summary>
        /// Deletes the rule for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="rule">The rule to delete.</param>
        /// <exception cref="System.ArgumentNullException">rule is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">rule id is less than 0.</exception>
        void DeleteValidationRule(ValidationRule rule);

        /// <summary>
        /// Gets the rules.
        /// </summary>
        /// <param name="fieldDefinitionId">The Id of the parent Field Definition</param>
        /// <returns>rule collection.</returns>
        IQueryable<ValidationRule> GetValidationRules(int fieldDefinitionId);

        /// <summary>
        /// Gets the settings for a validation rule.
        /// </summary>
        /// <param name="validationRuleId">The Id of the parent Validation Rule</param>
        /// <returns>setting dictionary.</returns>
        IDictionary<string, ValidatorSetting> GetValidationSettings(int validationRuleId);

        /// <summary>
        /// Updates the rule.
        /// </summary>
        /// <param name="rule">The rule to update.</param>
        /// <exception cref="System.ArgumentNullException">rule is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">rule id is less than 0.</exception>
        void UpdateValidationRule(ValidationRule rule);
    }
}
