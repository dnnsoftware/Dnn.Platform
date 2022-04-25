// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary.Impl
{
    using System;
    using System.Linq;

    /// <summary>
    /// A base class that implements <see cref="IStep"/>.
    /// </summary>
    internal abstract class StepBase : IStep
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public bool? Success { get; protected set; }

        /// <inheritdoc/>
        public string Notes { get; protected set; }

        /// <inheritdoc/>
        public void Execute()
        {
            try
            {
                this.CheckRequired();
                this.ExecuteInternal();
            }
            catch
            {
                // this.log.Error(ex);
                this.Success = false;
                this.Notes = "Internal error.";
            }
        }

        /// <summary>
        /// Performs the actual step execution.
        /// </summary>
        protected abstract void ExecuteInternal();

        private void CheckRequired()
        {
            var nullProperties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                .Where(p => string.IsNullOrEmpty($"{p.GetValue(this)}"))
                .Select(p => p.Name)
                .ToArray();

            if (nullProperties.Length > 0)
            {
                throw new InvalidOperationException(string.Format(
                    "Following required properties are not set: {0}",
                    string.Join(", ", nullProperties)));
            }
        }
    }
}
