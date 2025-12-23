using FluentValidation;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Validators.UserValidators;
using TaskFlow.Core.Common;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserRequest> _createUserValidator;
    private readonly IValidator<UpdateUserProfileRequest> _updateProfileValidator;

    public UserService(
        IUserRepository userRepository,
        IValidator<CreateUserRequest> createUserValidator,
        IValidator<UpdateUserProfileRequest> updateProfileValidator)
    {
        _userRepository = userRepository;
        _createUserValidator = createUserValidator;
        _updateProfileValidator = updateProfileValidator;
    }

    public async Task<ValidationResult<User>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Валидация
        var validationResult = await _createUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return ValidationResult<User>.Failure(errors);
        }

        try
        {
            // Создание пользователя через доменную модель
            var user = User.Create(
                request.Email.Trim(),
                request.UserName.Trim(),
                request.FullName?.Trim()
            );

            // Сохранение
            var savedUser = await _userRepository.AddAsync(user, cancellationToken);
            return ValidationResult<User>.Success(savedUser);
        }
        catch (DomainException ex)
        {
            return ValidationResult<User>.Failure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            // Логирование
            return ValidationResult<User>.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult<User?>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return ValidationResult<User?>.Success(user);
        }
        catch (Exception ex)
        {
            return ValidationResult<User?>.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult<User?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ValidationResult<User?>.Failure(new[] { "Email is required" });
        }

        try
        {
            var user = await _userRepository.GetByEmailAsync(email.Trim(), cancellationToken);
            return ValidationResult<User?>.Success(user);
        }
        catch (Exception ex)
        {
            return ValidationResult<User?>.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult> UpdateUserProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        // Валидация
        var validationResult = await _updateProfileValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return ValidationResult.Failure(errors);
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return ValidationResult.Failure(new[] { $"User with ID '{userId}' not found" });
            }

            user.UpdateProfile(request.FullName?.Trim(), request.AvatarUrl?.Trim());
            await _userRepository.UpdateAsync(user, cancellationToken);

            return ValidationResult.Success();
        }
        catch (DomainException ex)
        {
            return ValidationResult.Failure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult> UpdateUserNameAsync(
        Guid userId,
        UpdateUserNameRequest request,
        CancellationToken cancellationToken = default)
    {
        // Создаем валидатор с userId (для проверки уникальности)
        var validator = new UpdateUserNameValidator(_userRepository, userId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return ValidationResult.Failure(errors);
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return ValidationResult.Failure(new[] { $"User with ID '{userId}' not found" });
            }

            user.UpdateUserName(request.UserName.Trim());
            await _userRepository.UpdateAsync(user, cancellationToken);

            return ValidationResult.Success();
        }
        catch (DomainException ex)
        {
            return ValidationResult.Failure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return ValidationResult.Failure(new[] { $"User with ID '{userId}' not found" });
            }

            await _userRepository.DeleteAsync(user, cancellationToken);
            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure(new[] { "An unexpected error occurred" });
        }
    }
}

// Обобщенный ValidationResult
public class ValidationResult<T> : ValidationResult
{
    public T? Data { get; }

    private ValidationResult(bool isValid, T? data, IEnumerable<string> errors)
        : base(isValid, errors)
    {
        Data = data;
    }

    public static ValidationResult<T> Success(T data) => new(true, data, null);
    public new static ValidationResult<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
}