using FluentValidation.Results;
using System.Diagnostics.CodeAnalysis;

namespace Application.Shared.Models
{
    [ExcludeFromCodeCoverage]
    public class Output
    {
        private readonly List<string> _errorMessages = new List<string>();
        private readonly List<string> _messages = new List<string>();

        public object Result { get; private set; }
        public bool IsValid => !_errorMessages.Any();
        public IReadOnlyCollection<string> ErrorMessages => _errorMessages;
        public IReadOnlyCollection<string> Messages => _messages;

        public void AddErrorMessage(string message) => _errorMessages.Add(message);
        public void AddMessage(string message) => _messages.Add(message);
        public void AddResult(object result) => Result = result;
        public void AddValidationResult(ValidationResult validationResult) =>
            _errorMessages.AddRange(validationResult.Errors.Select(s => s.ErrorMessage).ToList());
    }
}
