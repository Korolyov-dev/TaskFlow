using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskFlow.Core.Common;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    // Безопасная десериализация
    public static Dictionary<string, object>? SafeDeserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json, _options);
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    // Безопасная сериализация
    public static string SafeSerialize(Dictionary<string, object> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
            return "{}";

        try
        {
            return JsonSerializer.Serialize(dictionary, _options);
        }
        catch
        {
            return "{}";
        }
    }

    // Получение значения по ключу с преобразованием типа
    public static T? GetValue<T>(Dictionary<string, object> metadata, string key)
    {
        if (metadata == null || !metadata.ContainsKey(key))
            return default;

        try
        {
            var value = metadata[key];
            if (value is JsonElement jsonElement)
            {
                // Преобразуем JsonElement в нужный тип
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String when typeof(T) == typeof(string) => (T)(object)jsonElement.GetString()!,
                    JsonValueKind.String when typeof(T) == typeof(Guid) => (T)(object)Guid.Parse(jsonElement.GetString()!),
                    JsonValueKind.Number when typeof(T) == typeof(int) => (T)(object)jsonElement.GetInt32(),
                    JsonValueKind.Number when typeof(T) == typeof(long) => (T)(object)jsonElement.GetInt64(),
                    JsonValueKind.True when typeof(T) == typeof(bool) => (T)(object)true,
                    JsonValueKind.False when typeof(T) == typeof(bool) => (T)(object)false,
                    _ => default
                };
            }

            // Если уже не JsonElement, пробуем прямое преобразование
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    // Создание метаданных для активности
    public static Dictionary<string, object> CreateMetadata(params (string Key, object Value)[] values)
    {
        var metadata = new Dictionary<string, object>();

        foreach (var (key, value) in values)
        {
            metadata[key] = value;
        }

        return metadata;
    }
}