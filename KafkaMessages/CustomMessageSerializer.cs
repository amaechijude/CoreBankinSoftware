using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KafkaMessages;

public static class CustomMessageSerializer
{
    private static readonly ConcurrentDictionary<
        Type,
        JsonSerializerOptions
    > _serializerOptionsCache = new();

    private static JsonSerializerOptions GetSerializerOptions(Type type)
    {
        return _serializerOptionsCache.GetOrAdd(
            type,
            t =>
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true,
                };
                return options;
            }
        );
    }

    public static string Serialize<T>(T message)
    {
        var options = GetSerializerOptions(typeof(T));
        return JsonSerializer.Serialize(message, options);
    }

    public static T Deserialize<T>(string message)
    {
        var options = GetSerializerOptions(typeof(T));
        return JsonSerializer.Deserialize<T>(message, options)!;
    }
}
