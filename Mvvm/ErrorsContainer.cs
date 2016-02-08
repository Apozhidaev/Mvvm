using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mvvm
{
    /// <summary>
    ///     Manages validation errors for an object, notifying when the error state changes.
    /// </summary>
    /// <typeparam name="T">The type of the error object.</typeparam>
    public class ErrorsContainer<T>
    {
        private static readonly T[] NoErrors = new T[0];
        protected readonly Action<string> RaiseErrorsChanged;
        protected readonly Dictionary<string, List<T>> ValidationResults;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorsContainer{T}" /> class.
        /// </summary>
        /// <param name="raiseErrorsChanged">
        ///     The action that invoked if when errors are added for an object./>
        ///     event.
        /// </param>
        public ErrorsContainer(Action<string> raiseErrorsChanged)
        {
            if (raiseErrorsChanged == null)
            {
                throw new ArgumentNullException(nameof(raiseErrorsChanged));
            }

            RaiseErrorsChanged = raiseErrorsChanged;
            ValidationResults = new Dictionary<string, List<T>>();
        }

        /// <summary>
        ///     Gets a value indicating whether the object has validation errors.
        /// </summary>
        public bool HasErrors => ValidationResults.Count != 0;

        /// <summary>
        ///     Gets the validation errors for a specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The validation errors of type <typeparamref name="T" /> for the property.</returns>
        public IEnumerable<T> GetErrors(string propertyName)
        {
            var localPropertyName = propertyName ?? string.Empty;
            List<T> currentValidationResults;
            if (ValidationResults.TryGetValue(localPropertyName, out currentValidationResults))
            {
                return currentValidationResults;
            }
            return NoErrors;
        }

        /// <summary>
        ///     Clears the errors for the property indicated by the property expression.
        /// </summary>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="propertyExpression">The expression indicating a property.</param>
        /// <example>
        ///     container.ClearErrors(()=>SomeProperty);
        /// </example>
        public void ClearErrors<TProperty>(Expression<Func<TProperty>> propertyExpression)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            ClearErrors(propertyName);
        }

        /// <summary>
        ///     Clears the errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of th property for which to clear errors.</param>
        /// <example>
        ///     container.ClearErrors("SomeProperty");
        /// </example>
        public void ClearErrors(string propertyName)
        {
            SetErrors(propertyName, new List<T>());
        }

        /// <summary>
        ///     Sets the validation errors for the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The property type for which to set errors.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression" /> indicating the property.</param>
        /// <param name="propertyErrors">The list of errors to set for the property.</param>
        public void SetErrors<TProperty>(Expression<Func<TProperty>> propertyExpression, IEnumerable<T> propertyErrors)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            SetErrors(propertyName, propertyErrors);
        }

        /// <summary>
        ///     Sets the validation errors for the specified property.
        /// </summary>
        /// <remarks>
        ///     If a change is detected then the errors changed event is raised.
        /// </remarks>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="newValidationResults">The new validation errors.</param>
        public void SetErrors(string propertyName, IEnumerable<T> newValidationResults)
        {
            var localPropertyName = propertyName ?? string.Empty;
            var hasCurrentValidationResults = ValidationResults.ContainsKey(localPropertyName);
            var validationResults = newValidationResults as T[] ?? newValidationResults.ToArray();
            var hasNewValidationResults = newValidationResults != null && validationResults.Any();

            if (hasCurrentValidationResults || hasNewValidationResults)
            {
                if (hasNewValidationResults)
                {
                    ValidationResults[localPropertyName] = new List<T>(validationResults);
                    RaiseErrorsChanged(localPropertyName);
                }
                else
                {
                    ValidationResults.Remove(localPropertyName);
                    RaiseErrorsChanged(localPropertyName);
                }
            }
        }
    }
}