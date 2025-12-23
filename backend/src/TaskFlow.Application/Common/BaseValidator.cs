using FluentValidation;

namespace TaskFlow.Application.Common;

public class BaseValidator<T> : AbstractValidator<T>
{
    protected bool BeValidHexColor(string color)
    {
        if (string.IsNullOrEmpty(color)) return false;
        if (color.Length != 7 || color[0] != '#') return false;

        return color.Substring(1).All(c =>
            (c >= '0' && c <= '9') ||
            (c >= 'a' && c <= 'f') ||
            (c >= 'A' && c <= 'F'));
    }

    protected bool BeValidPriority(int priority)
    {
        return priority >= 0 && priority <= 3;
    }
}
