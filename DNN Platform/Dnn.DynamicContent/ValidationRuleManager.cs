// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Data;

namespace Dnn.DynamicContent
{
    public class ValidationRuleManager : ControllerBase<ValidationRule, IValidationRuleManager, ValidationRuleManager>, IValidationRuleManager
    {
        internal const string ValidationRuleCacheKey = "ContentTypes_ValidationRules";
        internal const string ValidationRuleScope = "FieldDefinitionId";

        protected override Func<IValidationRuleManager> GetFactory()
        {
            return () => new ValidationRuleManager();
        }

        public ValidationRuleManager() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ValidationRuleManager(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds a new rule for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        /// <returns>rule id.</returns>
        /// <exception cref="System.ArgumentNullException">rule is null.</exception>
        public int AddValidationRule(ValidationRule rule)
        {
            //Argument Contract
            Requires.NotNull(rule);
            Requires.PropertyNotNegative(rule, "FieldDefinitionId");
            Requires.PropertyNotNegative(rule, "ValidatorTypeId");

            using (DataContext)
            {
                var rep = DataContext.GetRepository<ValidationRule>();

                rep.Insert(rule);

                //Add any Validation Settings
                var settingRep = DataContext.GetRepository<ValidatorSetting>();
                foreach (var setting in rule.ValidationSettings.Values)
                {
                    setting.ValidationRuleId = rule.ValidationRuleId;
                    settingRep.Insert(setting);
                }
            }

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
            //Argument Contract
            Requires.NotNull(rule);
            Requires.PropertyNotNull(rule, "ValidationRuleId");
            Requires.PropertyNotNegative(rule, "ValidationRuleId");

            using (DataContext)
            {
                var rep = DataContext.GetRepository<ValidationRule>();

                rep.Delete(rule);
            }
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
        /// Gets the settings for a validation rule.
        /// </summary>
        /// <param name="validationRuleId">The Id of the parent Validation Rule</param>
        /// <returns>setting dictionary.</returns>
        public IDictionary<string, ValidatorSetting> GetValidationSettings(int validationRuleId)
        {
            var settings = new Dictionary<string, ValidatorSetting>();
            using (DataContext)
            {
                var settingRep = DataContext.GetRepository<ValidatorSetting>();

                foreach (var setting in settingRep.Get(validationRuleId))
                {
                    settings.Add(setting.SettingName, setting);
                }
            }
            return settings;
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
            Requires.NotNull(rule);
            Requires.PropertyNotNull(rule, "ValidationRuleId");
            Requires.PropertyNotNegative(rule, "ValidationRuleId");
            Requires.PropertyNotNegative(rule, "FieldDefinitionId");
            Requires.PropertyNotNegative(rule, "ValidatorTypeId");

            using (DataContext)
            {
                var rep = DataContext.GetRepository<ValidationRule>();

                rep.Update(rule);
            }
        }
    }
}
