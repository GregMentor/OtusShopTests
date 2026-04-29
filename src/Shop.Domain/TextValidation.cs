namespace Shop.Domain;

public static class TextValidation
{
    public static string EnsureNotBlank(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Значение не может быть пустым", paramName);

        return value;
    }
}

