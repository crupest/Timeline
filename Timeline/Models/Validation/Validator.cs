using System;
using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Validation
{
    /// <summary>
    /// A validator to validate value.
    /// See <see cref="Validate(object, out string)"/>.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validate given value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="message">The validation message.</param>
        /// <returns>True if validation passed. Otherwise false.</returns>
        bool Validate(object value, out string message);
    }

    public static class ValidationConstants
    {
        public const string SuccessMessage = "Validation succeeded.";
    }

    /// <summary>
    /// Convenient base class for validator.
    /// </summary>
    /// <typeparam name="T">The type of accepted value.</typeparam>
    /// <remarks>
    /// Subclass should override <see cref="DoValidate(T, out string)"/> to do the real validation.
    /// This class will check the nullity and type of value. If value is null or not of type <typeparamref name="T"/>
    /// it will return false and not call <see cref="DoValidate(T, out string)"/>.
    /// 
    /// If you want some other behaviours, write the validator from scratch.
    /// </remarks>
    public abstract class Validator<T> : IValidator
    {
        public bool Validate(object value, out string message)
        {
            if (value == null)
            {
                message = "Value is null.";
                return false;
            }

            if (value is T v)
            {

                return DoValidate(v, out message);
            }
            else
            {
                message = $"Value is not of type {typeof(T).Name}";
                return false;
            }
        }

        protected abstract bool DoValidate(T value, out string message);
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class ValidateWithAttribute : ValidationAttribute
    {
        private readonly IValidator _validator;

        /// <summary>
        /// Create with a given validator.
        /// </summary>
        /// <param name="validator">The validator used to validate.</param>
        public ValidateWithAttribute(IValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Create the validator with default constructor.
        /// </summary>
        /// <param name="validatorType">The type of the validator.</param>
        public ValidateWithAttribute(Type validatorType)
        {
            if (validatorType == null)
                throw new ArgumentNullException(nameof(validatorType));

            if (!typeof(IValidator).IsAssignableFrom(validatorType))
                throw new ArgumentException("Given type is not assignable to IValidator.", nameof(validatorType));

            try
            {
                _validator = Activator.CreateInstance(validatorType) as IValidator;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to create a validator instance from default constructor. See inner exception.", e);
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (_validator.Validate(value, out var message))
                return ValidationResult.Success;
            else
                return new ValidationResult(string.Format("Field {0} is bad. {1}", validationContext.DisplayName, message));
        }
    }
}
