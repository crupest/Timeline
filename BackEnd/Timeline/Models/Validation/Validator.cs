using System;
using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Validation
{
    /// <summary>
    /// A validator to validate value.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validate given value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>Validation success or not and message.</returns>
        (bool, string) Validate(object? value);
    }

    public static class ValidatorExtensions
    {
        public static bool Validate(this IValidator validator, object? value, out string message)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            var (r, m) = validator.Validate(value);
            message = m;
            return r;
        }
    }

    /// <summary>
    /// Convenient base class for validator.
    /// </summary>
    /// <typeparam name="T">The type of accepted value.</typeparam>
    /// <remarks>
    /// Subclass should override <see cref="DoValidate(T)"/> to do the real validation.
    /// This class will check the nullity and type of value.
    /// If value is null, it will pass or fail depending on <see cref="PermitNull"/>.
    /// If value is not null and not of type <typeparamref name="T"/>
    /// it will fail and not call <see cref="DoValidate(T)"/>.
    /// 
    /// <see cref="PermitNull"/> is true by default.
    /// 
    /// If you want some other behaviours, write the validator from scratch.
    /// </remarks>
    public abstract class Validator<T> : IValidator
    {
        protected bool PermitNull { get; set; } = true;

        public (bool, string) Validate(object? value)
        {
            if (value is null)
            {
                if (PermitNull)
                    return (true, GetSuccessMessage());
                else
                    return (false, Resource.CantBeNull);
            }

            if (value is T v)
            {
                return DoValidate(v);
            }
            else
            {
                return (false, string.Format(Resource.NotOfType, typeof(T).Name));
            }
        }

        protected static string GetSuccessMessage() => Resource.ValidationPassed;

        protected abstract (bool, string) DoValidate(T value);
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class ValidateWithAttribute : ValidationAttribute
    {
        protected readonly IValidator _validator;

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
                throw new ArgumentException(Resource.ValidateWithAttributeExceptionNotValidator, nameof(validatorType));

            try
            {
                _validator = (Activator.CreateInstance(validatorType) as IValidator)!;
            }
            catch (Exception e)
            {
                throw new ArgumentException(Resource.ValidateWithAttributeExceptionCreateFail, e);
            }
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var (result, message) = _validator.Validate(value);
            if (result)
            {
                return ValidationResult.Success!;
            }
            else
            {
                return new ValidationResult(message);
            }
        }
    }
}
